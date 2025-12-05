using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour, IInteractable
{
    [Header("Configuración del Enemigo")]
    public int maxHealth = 50;
    public int currentHealth;
    public int damage = 15;
    public float moveSpeed = 2f;
    public float detectionRange = 5f;
    public float attackRange = 1.5f;

    [Header("Sistema de Detección")]
    public GameObject detectionIndicator;
    public bool playerDetected = false;

    [Header("Recompensas")]
    public int luzOnDefeat = 5;
    public int sombraOnDefeat = 5;

    // Componentes
    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Estados
    private enum EnemyState { Idle, Patrol, Chase, Attack, Hurt, Dead }
    private EnemyState currentState = EnemyState.Patrol;

    // Variables temporales
    private bool isHurting = false;
    private Vector2 patrolDirection;
    private float patrolTimer;
    private bool isAttacking = false;

    private void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Inicializar patrulla
        patrolDirection = Random.insideUnitCircle.normalized;
        patrolTimer = Random.Range(2f, 5f);

        // Configurar indicador de detección
        if (detectionIndicator != null)
            detectionIndicator.SetActive(false);
    }

    private void Update()
    {
        if (currentState == EnemyState.Dead) return;

        CheckForPlayer();
        UpdateStateMachine();
        UpdateAnimations();
    }

    private void CheckForPlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool wasDetected = playerDetected;
        playerDetected = distanceToPlayer <= detectionRange;

        // Mostrar/ocultar indicador de detección
        if (detectionIndicator != null && wasDetected != playerDetected)
        {
            detectionIndicator.SetActive(playerDetected);

            // Comentario de Fukurō cuando es detectado
            if (playerDetected && Level2Manager.Instance != null)
            {
                Level2Manager.Instance.TriggerCombate();
            }
        }
    }

    private void UpdateStateMachine()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState();
                break;
            case EnemyState.Patrol:
                HandlePatrolState();
                break;
            case EnemyState.Chase:
                HandleChaseState();
                break;
            case EnemyState.Attack:
                HandleAttackState();
                break;
            case EnemyState.Hurt:
                HandleHurtState();
                break;
        }
    }

    private void HandleIdleState()
    {
        // Transición a patrulla después de un tiempo
        patrolTimer -= Time.deltaTime;
        if (patrolTimer <= 0)
        {
            currentState = EnemyState.Patrol;
            patrolDirection = Random.insideUnitCircle.normalized;
            patrolTimer = Random.Range(2f, 5f);
        }

        // Transición a chase si detecta jugador
        if (playerDetected)
        {
            currentState = EnemyState.Chase;
        }
    }

    private void HandlePatrolState()
    {
        // Movimiento de patrulla
        rb.linearVelocity = patrolDirection * moveSpeed * 0.5f;

        // Cambiar dirección periódicamente
        patrolTimer -= Time.deltaTime;
        if (patrolTimer <= 0)
        {
            patrolDirection = Random.insideUnitCircle.normalized;
            patrolTimer = Random.Range(2f, 5f);
        }

        // Transición a chase si detecta jugador
        if (playerDetected)
        {
            currentState = EnemyState.Chase;
        }
    }

    private void HandleChaseState()
    {
        if (player == null) return;

        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        rb.linearVelocity = directionToPlayer * moveSpeed;

        // Flip sprite según dirección
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = directionToPlayer.x < 0;
        }

        // Verificar si está en rango de ataque
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            currentState = EnemyState.Attack;
            rb.linearVelocity = Vector2.zero;
        }

        // Volver a patrulla si pierde al jugador
        if (!playerDetected)
        {
            currentState = EnemyState.Patrol;
            patrolDirection = Random.insideUnitCircle.normalized;
        }
    }

    private void HandleAttackState()
    {
        // Lógica de ataque (puedes implementar animaciones aquí)
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer > attackRange)
            {
                currentState = EnemyState.Chase;
            }
            else
            {
                if (!isAttacking)
                {
                    StartCoroutine(AttackPlayer());
                }
            }
        }
    }

    private void HandleHurtState()
    {
        // Estado de daño - breve invulnerabilidad
        if (!isHurting)
        {
            currentState = EnemyState.Chase;
        }
    }

    private IEnumerator AttackPlayer()
    {
        isAttacking = true;
        if (animator != null)
            animator.SetTrigger("attack");

        var pc = player.GetComponent<PlayerController>();
        if (pc != null && !pc.IsInvulnerable)
        {
            pc.TakeDamage(damage);
            Vector2 dir = (pc.transform.position - transform.position).normalized;
            pc.ApplyKnockback(dir, 1f, 10f);
        }

        yield return new WaitForSeconds(1f);
        isAttacking = false;
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        animator.SetBool("isMoving", rb.linearVelocity.magnitude > 0.1f);
        animator.SetBool("isChasing", currentState == EnemyState.Chase);
        animator.SetFloat("moveSpeed", rb.linearVelocity.magnitude);
    }

    public void TakeDamage(int damage, CombatManager.CombatMode attackMode)
    {
        if (currentState == EnemyState.Dead || isHurting) return;

        currentHealth -= damage;

        // Efectos visuales según el modo de ataque
        switch (attackMode)
        {
            case CombatManager.CombatMode.Sombra:
                // Efecto sutil
                StartCoroutine(HurtEffect(Color.blue));
                break;
            case CombatManager.CombatMode.Furia:
                // Efecto intenso
                StartCoroutine(HurtEffect(Color.red));
                break;
        }

        Debug.Log($"Espíritu Engañado recibe {damage} de daño. Vida: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            currentState = EnemyState.Hurt;
            StartCoroutine(HurtCooldown());
        }
    }

    private IEnumerator HurtEffect(Color flashColor)
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }

    private IEnumerator HurtCooldown()
    {
        isHurting = true;
        yield return new WaitForSeconds(0.5f);
        isHurting = false;
    }

    public void ApplyKnockback(Vector2 direction, float distance, float speed)
    {
        StartCoroutine(KnockbackCoroutine(direction, distance, speed));
    }

    private IEnumerator KnockbackCoroutine(Vector2 direction, float distance, float speed)
    {
        rb.linearVelocity = Vector2.zero;
        Vector3 dir = ((Vector3)direction).normalized;
        float remaining = distance;
        while (remaining > 0f)
        {
            float step = speed * Time.deltaTime;
            transform.position += dir * step;
            remaining -= step;
            yield return null;
        }
    }

    private void Die()
    {
        currentState = EnemyState.Dead;
        rb.linearVelocity = Vector2.zero;

        // Animación de muerte
        if (animator != null)
            animator.SetTrigger("die");

        // Recompensas morales
        if (MoralSystem.Instance != null)
        {
            MoralSystem.Instance.AddLuz(luzOnDefeat);
            MoralSystem.Instance.AddSombra(sombraOnDefeat);
        }

        var pc = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.GainExperience(5);
        }

        // Desactivar colisiones
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;

        // Destruir después de animación
        Destroy(gameObject, 2f);

        int alive = 0;
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null && enemies[i] != this && enemies[i].currentHealth > 0)
                alive++;
        }
        if (alive == 0 && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ShowDialogue("Nivel Completado", "Has derrotado a todos los enemigos.");
        }

        Debug.Log("Espíritu Engañado derrotado!");
    }

    // Implementación de IInteractable (para diálogos o interacciones pacíficas)
    public void Interact()
    {
        // Posible interacción pacífica con el espíritu
        DialogueManager.Instance.ShowDialogue("Espíritu Engañado", "No puedo descansar... La oscuridad me consume...");
    }

    public bool CanInteract()
    {
        return currentState != EnemyState.Dead && !playerDetected;
    }

    private void OnDrawGizmosSelected()
    {
        // Rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Rango de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}