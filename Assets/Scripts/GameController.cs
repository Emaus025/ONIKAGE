using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int nivelActual = 1;
    public string climaActual = "Normal";
    public int progresoHistoria = 0; // ✅ NUEVO: Para tracking de progreso
    
    public bool[] decisionesNivel1 = new bool[3];

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
    
    // ✅ NUEVO: Método para avanzar la historia
    public void AvanzarHistoria()
    {
        progresoHistoria++;
        
        string[] mensajesProgreso = {
            "El camino comienza... confía en tus instintos",
            "El primer misterio se revela... el santuario observa",
            "Las sombras susurran verdades... continúa tu búsqueda",
            "Cada paso te acerca a la verdad final... y a tu destino"
        };
        
        if (progresoHistoria < mensajesProgreso.Length)
        {
            // Usar el narrador de historia si existe
            if (StoryNarrator.Instance != null)
            {
                StoryNarrator.Instance.ShowProgressDialogue(mensajesProgreso[progresoHistoria]);
            }
        }
    }
    
    // ✅ NUEVO: Método para hints de puzzles
    public void MostrarHintPuzzle(int idPuzzle)
    {
        string[] hints = {
            "Observa el patrón en las sombras...",
            "El orden sigue la sabiduría ancestral...",
            "A veces la respuesta está en lo que no ves...",
            "La paciencia revela lo que la prisa oculta..."
        };
        
        if (idPuzzle >= 0 && idPuzzle < hints.Length && StoryNarrator.Instance != null)
        {
            StoryNarrator.Instance.ShowPuzzleHint(idPuzzle);
        }
    }
}