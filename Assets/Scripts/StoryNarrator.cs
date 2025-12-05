using UnityEngine;
using System.Collections;

public class StoryNarrator : MonoBehaviour
{
    public static StoryNarrator Instance;
    
    [Header("Configuración del Narrador")]
    public float timeBetweenNarrations = 3f;
    public float triggerDistance = 8f;
    
    [Header("Diálogos de la Historia")]
    private readonly string[] storyDialogues = {
        // Introducción
        "El santuario del engaño te da la bienvenida, valiente samurái...",
        "Tu camino comienza aquí, donde las sombras susurran verdades olvidadas...",
        "Busca la luz entre las ilusiones, pero cuidado con lo que encuentres...",
        
        // Guía de progreso
        "Avanza hacia el este, donde el templo antiguo guarda los primeros secretos...",
        "Escucha el sonido del agua, te guiará hacia tu siguiente prueba...",
        "Las estatuas no son lo que parecen, observa con atención...",
        
        // Pistas de puzzles
        "El orden de los elementos sigue el ciclo natural: agua, tierra, fuego, aire...",
        "A veces retroceder es la única forma de avanzar...",
        "La verdad se esconde en lo que eliges ignorar...",
        
        // Momentos climáticos
        "El engaño se revela cuando menos lo esperas...",
        "¿Eres tú quien controla tu destino, o solo una pieza en este juego?",
        "Las decisiones que tomas ahora resonarán en toda la eternidad..."
    };
    
    private Transform player;
    private int currentDialogueIndex = 0;
    private Coroutine narrationCoroutine;
    private bool isNarrating = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Comenzar narración automáticamente
        StartNarration();
    }
    
    public void StartNarration()
    {
        if (narrationCoroutine != null)
            StopCoroutine(narrationCoroutine);
        
        narrationCoroutine = StartCoroutine(NarrationRoutine());
    }
    
    public void StopNarration()
    {
        if (narrationCoroutine != null)
        {
            StopCoroutine(narrationCoroutine);
            narrationCoroutine = null;
        }
        isNarrating = false;
    }
    
    private IEnumerator NarrationRoutine()
    {
        isNarrating = true;
        
        // Narración inicial
        yield return new WaitForSeconds(2f);
        ShowStoryDialogue(storyDialogues[0]);
        
        while (isNarrating)
        {
            yield return new WaitForSeconds(timeBetweenNarrations);
            
            if (player != null)
            {
                // Mostrar siguiente diálogo de la historia
                ShowNextStoryDialogue();
            }
        }
    }
    
    private void ShowNextStoryDialogue()
    {
        if (storyDialogues.Length == 0) return;
        
        currentDialogueIndex = (currentDialogueIndex + 1) % storyDialogues.Length;
        ShowStoryDialogue(storyDialogues[currentDialogueIndex]);
    }
    
    private void ShowStoryDialogue(string message)
    {
        Debug.Log($"Narrador: {message}");
        
        bool shown = false;
        // Usar el sistema de texto flotante (prioridad)
        if (FloatingTextManager.Instance != null && player != null)
        {
            shown = FloatingTextManager.Instance.ShowFloatingText(message, player.position, Color.cyan);
        }
        
        // Usar sistema de diálogo tradicional (respaldo si falla el texto flotante)
        if (!shown && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ShowDialogue("Narrador", message);
        }
    }
    
    // Para narraciones específicas de progreso
    public void ShowProgressDialogue(string message)
    {
        ShowStoryDialogue(message);
    }
    
    public void ShowPuzzleHint(int puzzleId)
    {
        string[] puzzleHints = {
            "Observa el patrón en las sombras...",
            "El orden sigue la sabiduría ancestral...",
            "A veces la respuesta está en lo que no ves...",
            "La paciencia revela lo que la prisa oculta..."
        };
        
        if (puzzleId >= 0 && puzzleId < puzzleHints.Length)
        {
            ShowStoryDialogue(puzzleHints[puzzleId]);
        }
    }
    
    // Para debug en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}