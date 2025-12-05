using UnityEngine;
using System.Collections;

public class YamishiNarrator : MonoBehaviour
{
    [Header("Configuración de Yamishi")]
    // Diálogos predeterminados en código
    private readonly string[] yamishiDialogues = new string[] {
        "Tu destino está entrelazado con el mío...",
        "Las sombras susurran tus secretos...",
        "¿Crees que eres libre? Todos bailamos al ritmo del engaño...",
        "Ssss... tu furia te consume tanto como a mí...",
        "El camino del engaño es tortuoso, pero necesario...",
        "Tus decisiones resuenan en el vacío...",
        "¿Qué es real? ¿Qué es ilusión? Ssss...",
        "La serpiente siempre se muerde la cola... como tu destino..."
    };

    public float minTimeBetweenVoices = 10f;
    public float maxTimeBetweenVoices = 30f;
    public float triggerDistance = 5f; // Distancia para activar los diálogos
    
    [Header("Efectos Opcionales")]
    public AudioClip textSoundEffect; // Sonido genérico al aparecer texto (opcional)
    
    private Transform player;
    private AudioSource audioSource;
    private bool playerInRange = false;
    private Coroutine voiceCoroutine;
    
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        // Configuración básica de audio solo para efectos de sonido (si se asignan)
        if (textSoundEffect != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f; // 2D sound para UI
        }
    }
    
    private void Update()
    {
        if (player == null) return;
        
        float distance = Vector2.Distance(transform.position, player.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= triggerDistance;
        
        if (playerInRange && !wasInRange)
        {
            // Jugador entró en el rango
            StartYamishiVoices();
        }
        else if (!playerInRange && wasInRange)
        {
            // Jugador salió del rango
            StopYamishiVoices();
        }
    }
    
    private void StartYamishiVoices()
    {
        if (voiceCoroutine != null)
            StopCoroutine(voiceCoroutine);
        
        voiceCoroutine = StartCoroutine(YamishiVoiceRoutine());
        
        // Primer mensaje de introducción al entrar en zona
        ShowYamishiDialogue("Sssss... ¿Puedes oírme, pequeño samurái?");
    }
    
    private void StopYamishiVoices()
    {
        if (voiceCoroutine != null)
        {
            StopCoroutine(voiceCoroutine);
            voiceCoroutine = null;
        }
    }
    
    private IEnumerator YamishiVoiceRoutine()
    {
        while (playerInRange)
        {
            yield return new WaitForSeconds(Random.Range(minTimeBetweenVoices, maxTimeBetweenVoices));
            
            // Solo mostrar diálogo si el jugador sigue en rango y no hay otro diálogo activo
            if (playerInRange && !DialogueManager.Instance.IsDialogueActive())
            {
                ShowRandomYamishiDialogue();
            }
        }
    }
    
    private void ShowRandomYamishiDialogue()
    {
        if (yamishiDialogues.Length == 0) return;
        
        string dialogue = yamishiDialogues[Random.Range(0, yamishiDialogues.Length)];
        ShowYamishiDialogue(dialogue);
    }

    private void ShowYamishiDialogue(string text)
    {
        bool shown = false;
        // Opción 1: Texto Flotante (Nuevo y mejorado)
        if (FloatingTextManager.Instance != null && player != null)
        {
            shown = FloatingTextManager.Instance.ShowFloatingText(text, player.position, Color.magenta);
        }
        
        // Opción 2: UI Clásica (Respaldo si falla lo anterior)
        if (!shown && DialogueManager.Instance != null)
        {
             DialogueManager.Instance.ShowDialogue("Yamishi", text);
        }
        
        // Reproducir sonido sutil de "pensamiento" o "susurro" si existe
        if (audioSource != null && textSoundEffect != null)
        {
            audioSource.PlayOneShot(textSoundEffect);
        }
    }
    
    // Para debug en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
    
    public void ForceYamishiDialogue(string message)
    {
        ShowYamishiDialogue(message);
    }
}