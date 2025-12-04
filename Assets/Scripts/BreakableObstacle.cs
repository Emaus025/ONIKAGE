using UnityEngine;
using System.Collections;

public class BreakableObstacle : MonoBehaviour
{
    [Header("Configuración del Obstáculo")]
    public int hitsRequired = 1;
    public bool respawns = false;
    public float respawnTime = 10f;

    [Header("Efectos")]
    public GameObject breakEffect;
    public AudioClip breakSound;
    public ItemDrop[] possibleDrops;

    [System.Serializable]
    public struct ItemDrop
    {
        public GameObject itemPrefab;
        public float dropChance;
    }

    // Componentes
    private Collider2D obstacleCollider;
    private SpriteRenderer spriteRenderer;
    private int currentHits = 0;
    private bool isBroken = false;

    private void Start()
    {
        obstacleCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Break()
    {
        if (isBroken) return;

        currentHits++;

        // Efecto visual de golpe
        StartCoroutine(HitEffect());

        if (currentHits >= hitsRequired)
        {
            DestroyObstacle();
        }
        else
        {
            Debug.Log($"Obstáculo golpeado: {currentHits}/{hitsRequired}");
        }
    }

    private IEnumerator HitEffect()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.gray;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }

    private void DestroyObstacle()
    {
        isBroken = true;

        // Desactivar componentes
        if (obstacleCollider != null)
            obstacleCollider.enabled = false;

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        // Efecto de destrucción
        if (breakEffect != null)
        {
            Instantiate(breakEffect, transform.position, Quaternion.identity);
        }

        // Sonido
        if (breakSound != null)
        {
            AudioSource.PlayClipAtPoint(breakSound, transform.position);
        }

        // Soltar items
        DropItems();

        Debug.Log("Obstáculo destruido!");

        // Respawn si está configurado
        if (respawns)
        {
            StartCoroutine(RespawnObstacle());
        }
        else
        {
            Destroy(gameObject, 2f);
        }
    }

    private void DropItems()
    {
        if (possibleDrops == null || possibleDrops.Length == 0) return;

        foreach (ItemDrop drop in possibleDrops)
        {
            if (Random.Range(0f, 1f) <= drop.dropChance && drop.itemPrefab != null)
            {
                Instantiate(drop.itemPrefab, transform.position, Quaternion.identity);
            }
        }
    }

    private IEnumerator RespawnObstacle()
    {
        yield return new WaitForSeconds(respawnTime);

        // Reactivar componentes
        if (obstacleCollider != null)
            obstacleCollider.enabled = true;

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        // Resetear estado
        currentHits = 0;
        isBroken = false;

        Debug.Log("Obstáculo reaparecido!");
    }

    // Para debug en el editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.8f);
    }
}