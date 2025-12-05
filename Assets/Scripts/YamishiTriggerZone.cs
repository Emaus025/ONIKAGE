using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class YamishiTriggerZone : MonoBehaviour
{
    [Header("Configuración de Zona de Activación")]
    public YamishiNarrator yamishiNarrator;
    public string sceneToLoadAfter; // Escena a cargar después del encuentro (opcional)
    
    [Header("Mensajes de Yamishi")]
    // Mensajes predeterminados en código
    private readonly string[] introductionMessages = {
        "Ssss... Bienvenido al santuario del engaño...",
        "He estado observándote... tus luchas, tus dudas...",
        "Soy Yamishi, la voz que susurra en las sombras...",
        "Tu camino está lleno de ilusiones... como esta..."
    };
    
    private bool hasTriggered = false;
    private int currentMessageIndex = 0;
    
    private void Reset()
    {
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Objeto entró en zona Yamishi: {other.name} | Tag: {other.tag}"); // DEBUG

        if (hasTriggered) return;
        
        // Verificar si es el jugador
        bool isPlayer = other.CompareTag("Player") || 
                        other.GetComponentInParent<PlayerController>() != null;
        
        if (isPlayer)
        {
            Debug.Log("¡JUGADOR DETECTADO EN ZONA YAMISHI!"); // DEBUG
            hasTriggered = true;
            StartYamishiEncounter();
        }
    }

    private void OnDrawGizmos()
    {
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            // Dibujar caja roja semitransparente para verla claramente en la escena
            Gizmos.color = hasTriggered ? new Color(0.5f, 0.5f, 0.5f, 0.5f) : new Color(1f, 0f, 1f, 0.5f);
            Gizmos.DrawCube(transform.position, collider.bounds.size);
            Gizmos.DrawWireCube(transform.position, collider.bounds.size);
        }
    }
    
    private void StartYamishiEncounter()
    {
        Debug.Log("Encuentro con Yamishi iniciado");
        
        // NOTA: Ya NO desactivamos los controles del jugador para evitar bloqueos.
        // El jugador puede moverse libremente mientras escucha a Yamishi.
        
        // Iniciar secuencia de diálogos usando el sistema interno simple
        StartCoroutine(PlaySimpleDialogueSequence());
    }

    // Nuevo sistema simple que usa Texto Flotante
    private System.Collections.IEnumerator PlaySimpleDialogueSequence()
    {
        // Referencia al jugador para saber dónde mostrar el texto
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        foreach (string msg in introductionMessages)
        {
            Debug.Log($"Yamishi dice: {msg}");
            
            // Opción 1: Usar Texto Flotante (Prioridad)
            bool shown = false;
            if (FloatingTextManager.Instance != null && playerTransform != null)
            {
                shown = FloatingTextManager.Instance.ShowFloatingText(msg, playerTransform.position, Color.magenta);
            }
            
            // Opción 2: Usar UI tradicional como respaldo (si falló el texto flotante o no existe el manager)
            if (!shown && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.ShowDialogue("Yamishi", msg);
            }
            
            // Esperar 4 segundos por mensaje (lectura cómoda)
            yield return new WaitForSecondsRealtime(4f);
            
            // Actualizar posición del jugador por si se movió
            if (playerTransform == null) 
                playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        FinishEncounter();
    }
    
    /* CÓDIGO ANTIGUO ELIMINADO PARA EVITAR ERRORES
    private void ShowNextMessage() { ... }
    private void OnDialogueFinished() { ... }
    private IEnumerator NextMessageWithDelay(...) { ... }
    */
    
    private void FinishEncounter()
    {
        Debug.Log("Encuentro con Yamishi completado");
        
        // Asegurar que el DialogueManager cierre el panel
        if (DialogueManager.Instance != null)
        {
             DialogueManager.Instance.CloseDialoguePanel();
        }

        // Activar el narrador Yamishi permanente
        if (yamishiNarrator != null)
        {
            yamishiNarrator.gameObject.SetActive(true);
            // Pequeño retraso para asegurar que todo esté inicializado antes de forzar el diálogo final
            StartCoroutine(DelayedFinalMessage());
        }
        
        // Opcional: cargar nueva escena
    if (!string.IsNullOrEmpty(sceneToLoadAfter))
    {
        // UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoadAfter);
    }
}

private System.Collections.IEnumerator DelayedFinalMessage()
{
    yield return new WaitForSeconds(0.5f);
    if (yamishiNarrator != null)
    {
        yamishiNarrator.ForceYamishiDialogue("A partir de ahora, seré tu sombra... tu conciencia... ssss...");
    }
}
}