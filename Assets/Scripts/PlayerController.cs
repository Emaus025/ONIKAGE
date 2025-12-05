using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Sistemas Adicionales")]
    public CombatManager combatSystem;
    public InteractionSystem interactionSystem;

    [Header("Movimiento")]
    public float moveSpeed = 3;
    private bool isMoving;
    private bool isDashing;
    private Vector2 input;
    public Vector2 LastMoveDirection { get; private set; }

    [Header("Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public int experience;
    public int level = 1;
    public int lives = 3;
    public int killCount = 0;

    public System.Action<int> OnKillCountChanged;

    [Header("Dash")]
    public float dashDistance = 1.5f;
    public float dashSpeed = 8f;
    public float invulnerabilityDuration = 0.3f;
    private bool isInvulnerable;
    public bool IsInvulnerable => isInvulnerable;
    private Coroutine knockbackRoutine;
    private Animator animator;
    private Rigidbody2D rb;

    public LayerMask solidObjectsLayer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("PlayerController: no Animator component found on the player GameObject.");
        }
        rb = GetComponent<Rigidbody2D>();

        combatSystem = GetComponent<CombatManager>();
        if (combatSystem == null) combatSystem = FindObjectOfType<CombatManager>();
        interactionSystem = GetComponent<InteractionSystem>();

        currentHealth = maxHealth;
        LastMoveDirection = Vector2.down;
    }

    public void Update()
    {
        if (!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            if (input.x != 0) input.y = 0;

            if (input != Vector2.zero)
            {
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);

                LastMoveDirection = input.normalized;

                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;
                if (isWalkable(targetPos))
                    StartCoroutine(Move(targetPos));
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) UseSkill1();
        if (Input.GetKeyDown(KeyCode.Alpha2)) UseSkill2();
        if (Input.GetKeyDown(KeyCode.Alpha3)) UseSkill3();

        animator.SetBool("isMoving", isMoving);
    }

    private IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            Vector2 next = Vector2.MoveTowards((Vector2)transform.position, (Vector2)targetPos, moveSpeed * Time.deltaTime);
            if (rb != null) rb.MovePosition(next); else transform.position = next;
            yield return null;
        }
        if (rb != null) rb.MovePosition((Vector2)targetPos); else transform.position = targetPos;
        isMoving = false;
    }

    private bool isWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer) != null)
        {
            return false;
        }
        return true;
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;
        currentHealth = Mathf.Max(currentHealth - amount, 0);
        if (animator != null) animator.SetTrigger("hurt");
        StartInvulnerability(invulnerabilityDuration);
        if (currentHealth == 0)
        {
            LoseLife();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    private void LoseLife()
    {
        lives = Mathf.Max(lives - 1, 0);
        if (lives > 0)
        {
            currentHealth = maxHealth;
        }
        else
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.ShowDialogue("Game Over", "Has perdido todas tus vidas.");
            }
            enabled = false;
        }
    }

    public void GainExperience(int amount)
    {
        experience += amount;
        int threshold = level * 100;
        if (experience >= threshold)
        {
            experience -= threshold;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level += 1;
        maxHealth += 10;
        currentHealth = maxHealth;
        moveSpeed += 0.1f;
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ShowDialogue("Nivel Ganado", "Has subido al nivel " + level);
        }
    }

    public void AddKill(int amount = 1)
    {
        if (amount <= 0) return;
        killCount += amount;
        OnKillCountChanged?.Invoke(killCount);
    }

    private void UseSkill1()
    {
        if (combatSystem != null && combatSystem.currentMode == CombatManager.CombatMode.Furia)
        {
            if (combatSystem.furiaMeter < combatSystem.furyCostSkill1) return;
            combatSystem.ConsumeFuria(combatSystem.furyCostSkill1);
        }
        PerformSkillAttack(15, 1.8f);
    }

    private void UseSkill2()
    {
        if (combatSystem != null && combatSystem.currentMode == CombatManager.CombatMode.Furia)
        {
            if (combatSystem.furiaMeter < combatSystem.furyCostSkill2) return;
            combatSystem.ConsumeFuria(combatSystem.furyCostSkill2);
        }
        PerformSkillAttack(25, 2.2f);
    }

    private void UseSkill3()
    {
        if (combatSystem != null && combatSystem.currentMode == CombatManager.CombatMode.Furia)
        {
            if (combatSystem.furiaMeter < combatSystem.furyCostSkill3) return;
            combatSystem.ConsumeFuria(combatSystem.furyCostSkill3);
        }
        PerformSkillAttack(40, 2.5f);
    }

    private void PerformSkillAttack(int damage, float radius)
    {
        if (combatSystem == null) return;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, combatSystem.enemyLayer);
        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, combatSystem.currentMode);
            }
        }
    }

    public void StartInvulnerability(float duration)
    {
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(InvulnerabilityCoroutine(duration));
    }

    private IEnumerator InvulnerabilityCoroutine(float duration)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
    }

    public void ApplyKnockback(Vector2 direction, float distance, float speed)
    {
        if (!gameObject.activeInHierarchy) return;
        if (knockbackRoutine != null) StopCoroutine(knockbackRoutine);
        knockbackRoutine = StartCoroutine(KnockbackCoroutine(direction, distance, speed));
    }

    private IEnumerator KnockbackCoroutine(Vector2 direction, float distance, float speed)
    {
        Vector2 dir = direction.normalized;
        float travelled = 0f;
        float maxTime = (distance / Mathf.Max(speed, 0.0001f)) + 0.15f;
        float t = 0f;
        while (travelled < distance && t < maxTime)
        {
            float currentSpeed = Mathf.Lerp(speed, 0f, travelled / distance);
            Vector2 target = (Vector2)transform.position + dir * (currentSpeed * Time.deltaTime);
            if (isWalkable(target))
            {
                if (rb != null) rb.MovePosition(target); else transform.position = target;
                travelled += currentSpeed * Time.deltaTime;
            }
            else
            {
                break;
            }
            t += Time.deltaTime;
            yield return null;
        }
        knockbackRoutine = null;
    }
}