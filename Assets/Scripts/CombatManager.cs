using UnityEngine;
using System.Collections;

public class CombatManager : MonoBehaviour
{
    public enum CombatMode { Sombra, Furia }

    [Header("Configuraci�n Combate")]
    public CombatMode currentMode = CombatMode.Sombra;
    public float furiaMeter = 0f;
    public float maxFuria = 100f;
    public float furiaGainPerHit = 10f;
    public float attackCooldown = 0.5f;
    public float dashDistance = 2f;
    public float dashSpeed = 8f;

    public float furyCostBasicAttack = 5f;
    public float furyCostSkill1 = 10f;
    public float furyCostSkill2 = 15f;
    public float furyCostSkill3 = 20f;

    [Header("Referencias")]
    public Animator animator;
    public GameObject attackHitbox;
    public LayerMask enemyLayer;

    private bool canAttack = true;
    private PlayerController playerController;

    // Eventos para UI
    public System.Action<CombatMode> OnModeChanged;
    public System.Action<float> OnFuriaChanged;

    private void Start()
    {
        // No sobrescribas referencias del inspector: solo busca si están vacías
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
            if (playerController == null) playerController = GetComponentInParent<PlayerController>();
            if (playerController == null) playerController = FindObjectOfType<PlayerController>();
            if (playerController == null) Debug.LogWarning("CombatManager: no se encontró PlayerController en este GameObject, su padre, ni en la escena.");
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (animator == null && playerController != null) animator = playerController.GetComponent<Animator>();
        }
        if (animator == null)
            Debug.LogWarning("CombatManager: no Animator encontrado; la animación de ataque no se reproducirá.");

        if (attackHitbox != null)
            attackHitbox.SetActive(false);
        else
            Debug.LogWarning("CombatManager: 'attackHitbox' no asignado; solo se usará OverlapCircle para daño.");
    }

    private void Update()
    {
        HandleCombatInput();
        UpdateModeEffects();
    }

    private void HandleCombatInput()
    {
        // Click izquierdo - Ataque básico (fallback tecla J para debug)
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J)) && canAttack)
        {
            Debug.Log("Input ataque detectado");
            PerformBasicAttack();
        }

        // Shift - Cambio de modos
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            SwitchCombatMode();
        }
    }

    private void PerformBasicAttack()
    {
        if (!canAttack) return;

        if (currentMode == CombatMode.Furia && furiaMeter < furyCostBasicAttack)
        {
            Debug.Log("Furia insuficiente para ataque básico");
            return;
        }

        StartCoroutine(AttackCooldown());
        StartCoroutine(DashForward());

        if (currentMode == CombatMode.Furia)
        {
            ConsumeFuria(furyCostBasicAttack);
        }

        if (animator != null)
        {
            Vector2 dir = playerController != null && playerController.LastMoveDirection != Vector2.zero ? playerController.LastMoveDirection : Vector2.down;
            animator.SetFloat("moveX", dir.x);
            animator.SetFloat("moveY", dir.y);
            animator.SetTrigger("attack");
        }

        if (attackHitbox != null)
        {
            StartCoroutine(ShowAttackHitbox());
        }

        CheckAttackHit();

        Debug.Log($"Ataque básico ejecutado en modo: {currentMode}");
    }

    private IEnumerator ShowAttackHitbox()
    {
        attackHitbox.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        attackHitbox.SetActive(false);
    }

    private IEnumerator DashForward()
    {
        if (playerController == null) yield break;
        Vector2 dir = playerController.LastMoveDirection;
        if (dir == Vector2.zero) dir = Vector2.down;
        float remaining = dashDistance;
        while (remaining > 0f)
        {
            float step = dashSpeed * Time.deltaTime;
            playerController.transform.position += (Vector3)(dir * step);
            remaining -= step;
            yield return null;
        }
        playerController.StartInvulnerability(0.2f);
    }

    public void OnAttackHitFrame()
    {
        if (attackHitbox != null)
        {
            StartCoroutine(ShowAttackHitbox());
        }
        CheckAttackHit();
    }

    private IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void CheckAttackHit()
    {
        if (enemyLayer == 0) Debug.LogWarning("CombatManager: 'enemyLayer' no configurado. Asigna la capa de enemigos en el inspector.");
        Vector2 facing = playerController != null && playerController.LastMoveDirection != Vector2.zero ? playerController.LastMoveDirection : Vector2.down;
        Vector3 center = playerController != null ? playerController.transform.position + (Vector3)(facing * 0.5f) : transform.position;
        int mask = enemyLayer == 0 ? ~0 : enemyLayer;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, 2.0f, mask);

        if (hits.Length == 0) Debug.LogWarning("CombatManager: ataque no impactó a ningún enemigo. Revisa 'Enemy Layer' y distancia.");
        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy == null) enemy = hit.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(10, currentMode);
                Vector2 dir = (enemy.transform.position - center).normalized;
                enemy.ApplyKnockback(dir, 1f, 10f);

                if (currentMode == CombatMode.Furia)
                {
                    if (playerController != null) playerController.Heal(10);
                }
                else
                {
                    AddFuria(furiaGainPerHit);
                }
            }
            else
            {
                OrcBoss boss = hit.GetComponent<OrcBoss>();
                if (boss == null) boss = hit.GetComponentInParent<OrcBoss>();
                if (boss != null)
                {
                    boss.TakeDamage(10, currentMode);
                    Vector2 dirB = (boss.transform.position - center).normalized;
                    boss.ApplyKnockback(dirB, 1f, 10f);

                    if (currentMode == CombatMode.Furia)
                    {
                        if (playerController != null) playerController.Heal(10);
                    }
                    else
                    {
                        AddFuria(furiaGainPerHit);
                    }
                }
            }

            BreakableObstacle obstacle = hit.GetComponent<BreakableObstacle>();
            if (obstacle == null) obstacle = hit.GetComponentInParent<BreakableObstacle>();
            if (obstacle != null)
            {
                obstacle.Break();
            }
        }
    }

    private void SwitchCombatMode()
    {
        currentMode = currentMode == CombatMode.Sombra ? CombatMode.Furia : CombatMode.Sombra;

        // Efectos visuales/sonoros del cambio de modo
        Debug.Log($"Modo cambiado a: {currentMode}");

        OnModeChanged?.Invoke(currentMode);

        // Actualizar UI o efectos
        if (animator != null)
        {
            animator.SetBool("isFuriaMode", currentMode == CombatMode.Furia);
        }
    }

    private void UpdateModeEffects()
    {
        // Efectos pasivos de cada modo
        switch (currentMode)
        {
            case CombatMode.Sombra:
                // Movimiento silencioso, detecci�n reducida
                if (playerController != null)
                {
                    playerController.moveSpeed = 3f;
                }
                break;

            case CombatMode.Furia:
                // Movimiento m�s r�pido, m�s detectable
                if (playerController != null)
                {
                    playerController.moveSpeed = 4f;
                }
                break;
        }
    }

    public void AddFuria(float amount)
    {
        furiaMeter = Mathf.Clamp(furiaMeter + amount, 0, maxFuria);
        OnFuriaChanged?.Invoke(furiaMeter / maxFuria);
    }

    public void ConsumeFuria(float amount)
    {
        furiaMeter = Mathf.Clamp(furiaMeter - amount, 0, maxFuria);
        OnFuriaChanged?.Invoke(furiaMeter / maxFuria);
    }

    // Para debug
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
}