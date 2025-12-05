using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class OrcBoss : MonoBehaviour
{
    [Header("Boss Stats")]
    public int maxHealth = 250;
    public int currentHealth;
    public int lightAttackDamage = 20;
    public int heavyAttackDamage = 35;
    public float moveSpeed = 2.2f;
    public float detectionRange = 6f;
    public float attackRange = 1.6f;

    [Header("Centro y Indicadores")]
    public Transform centerPoint;
    public GameObject detectionIndicator;
    public Vector2 indicatorOffset;

    [Header("Ataques")]
    public float lightAttackCooldown = 1.2f;
    public float heavyAttackCooldown = 2.4f;

    [Header("Fin del Juego")]
    public string endSceneName = "Final";

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;

    private bool playerDetected;
    private bool isAttacking;
    private bool isHurting;
    private bool isDead;
    private Coroutine knockbackRoutine;

    private Enemy enemyCore;

    private void Awake()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        enemyCore = GetComponent<Enemy>();
        if (enemyCore != null) enemyCore.enabled = false;
        if (detectionIndicator != null) detectionIndicator.SetActive(false);
    }

    private void Update()
    {
        if (isDead) return;

        UpdateIndicatorPosition();

        if (player != null)
        {
            float dist = Vector2.Distance(GetCenter(), player.position);
            bool wasDetected = playerDetected;
            playerDetected = dist <= detectionRange;
            if (detectionIndicator != null && wasDetected != playerDetected)
                detectionIndicator.SetActive(playerDetected);
        }

        if (isHurting) return;

        if (!playerDetected || player == null)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isMoving", false);
            return;
        }

        float distanceToPlayer = Vector2.Distance(GetCenter(), player.position);
        if (distanceToPlayer > attackRange)
        {
            Vector2 dir = (player.position - GetCenter()).normalized;
            rb.linearVelocity = dir * moveSpeed;
            animator.SetBool("isMoving", true);
            if (sr != null) sr.flipX = dir.x < 0;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isMoving", false);
            if (!isAttacking) StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        bool useHeavy = Random.value > 0.5f;
        if (useHeavy)
        {
            animator.SetTrigger("attack2");
            yield return new WaitForSeconds(0.35f);
            DoDamage(heavyAttackDamage);
            yield return new WaitForSeconds(heavyAttackCooldown);
        }
        else
        {
            animator.SetTrigger("attack1");
            yield return new WaitForSeconds(0.25f);
            DoDamage(lightAttackDamage);
            yield return new WaitForSeconds(lightAttackCooldown);
        }
        isAttacking = false;
    }

    private void DoDamage(int dmg)
    {
        if (player == null) return;
        var pc = player.GetComponent<PlayerController>();
        if (pc == null || pc.IsInvulnerable) return;

        pc.TakeDamage(dmg);
        Vector2 dir = (pc.transform.position - GetCenter()).normalized;
        pc.ApplyKnockback(dir, 1.2f, 12f);
    }

    public void TakeDamage(int damage, CombatManager.CombatMode attackMode)
    {
        if (isDead || isHurting) return;
        currentHealth -= damage;

        if (attackMode == CombatManager.CombatMode.Furia)
        {
            if (sr != null) sr.color = Color.red;
            StartCoroutine(HurtColorReset());
        }
        else
        {
            if (sr != null) sr.color = Color.blue;
            StartCoroutine(HurtColorReset());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            isHurting = true;
            animator.SetTrigger("hurt");
            StartCoroutine(HurtCooldown());
        }
    }

    private IEnumerator HurtColorReset()
    {
        yield return new WaitForSeconds(0.1f);
        if (sr != null) sr.color = Color.white;
    }

    private IEnumerator HurtCooldown()
    {
        yield return new WaitForSeconds(0.5f);
        isHurting = false;
    }

    public void ApplyKnockback(Vector2 direction, float distance, float speed)
    {
        if (knockbackRoutine != null) StopCoroutine(knockbackRoutine);
        knockbackRoutine = StartCoroutine(KnockbackCoroutine(direction, distance, speed));
    }

    private IEnumerator KnockbackCoroutine(Vector2 direction, float distance, float speed)
    {
        rb.linearVelocity = Vector2.zero;
        Vector2 dir = direction.normalized;
        float travelled = 0f;
        float maxTime = (distance / Mathf.Max(speed, 0.0001f)) + 0.15f;
        float t = 0f;
        while (travelled < distance && t < maxTime)
        {
            float currentSpeed = Mathf.Lerp(speed, 0f, travelled / distance);
            Vector2 target = (Vector2)transform.position + dir * (currentSpeed * Time.deltaTime);
            rb.MovePosition(target);
            travelled += currentSpeed * Time.deltaTime;
            t += Time.deltaTime;
            yield return null;
        }
        knockbackRoutine = null;
    }

    private void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger("die");

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        var pc = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.GainExperience(5);
            pc.AddKill(1);
        }

        if (!string.IsNullOrEmpty(endSceneName))
        {
            StartCoroutine(WaitAndLoadEndScene());
            return;
        }

        Destroy(gameObject, 2f);
    }

    private IEnumerator WaitAndLoadEndScene()
    {
        yield return new WaitForSeconds(6f);
        if (!string.IsNullOrEmpty(endSceneName))
            SceneManager.LoadScene(endSceneName);
    }

    private Vector3 GetCenter()
    {
        return centerPoint != null ? centerPoint.position : transform.position;
    }

    private void UpdateIndicatorPosition()
    {
        if (detectionIndicator == null) return;
        detectionIndicator.transform.position = GetCenter() + (Vector3)indicatorOffset;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(centerPoint != null ? centerPoint.position : transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centerPoint != null ? centerPoint.position : transform.position, attackRange);
    }
}