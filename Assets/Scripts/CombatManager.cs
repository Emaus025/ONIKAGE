using UnityEngine;
using System.Collections;

public class CombatManager : MonoBehaviour
{
    public enum CombatMode { Sombra, Furia }

    [Header("Configuración Combate")]
    public CombatMode currentMode = CombatMode.Sombra;
    public float furiaMeter = 0f;
    public float maxFuria = 100f;
    public float furiaGainPerHit = 10f;
    public float attackCooldown = 0.5f;

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
        playerController = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();

        if (attackHitbox != null)
            attackHitbox.SetActive(false);
    }

    private void Update()
    {
        HandleCombatInput();
        UpdateModeEffects();
    }

    private void HandleCombatInput()
    {
        // Click izquierdo - Ataque básico
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
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

        StartCoroutine(AttackCooldown());

        // Animación de ataque
        if (animator != null)
        {
            animator.SetTrigger("attack");
        }

        // Activar hitbox temporal
        if (attackHitbox != null)
        {
            StartCoroutine(ShowAttackHitbox());
        }

        // Detectar enemigos u obstáculos
        CheckAttackHit();

        Debug.Log($"Ataque básico ejecutado en modo: {currentMode}");
    }

    private IEnumerator ShowAttackHitbox()
    {
        attackHitbox.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        attackHitbox.SetActive(false);
    }

    private IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void CheckAttackHit()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.5f, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(10, currentMode);

                // Ganar furia si está en modo Furia
                if (currentMode == CombatMode.Furia)
                {
                    AddFuria(furiaGainPerHit);
                }
            }

            // También verificar obstáculos rompibles
            BreakableObstacle obstacle = hit.GetComponent<BreakableObstacle>();
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
                // Movimiento silencioso, detección reducida
                if (playerController != null)
                {
                    playerController.moveSpeed = 3f;
                }
                break;

            case CombatMode.Furia:
                // Movimiento más rápido, más detectable
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