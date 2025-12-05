using UnityEngine;
using System.Collections;

public class CabinDialogueTrigger : MonoBehaviour
{
    [Header("Configuración del Mensaje")]
    [Tooltip("Escribe aquí el mensaje o mensajes. Si es solo uno, se mostrará fijo.")]
    public string[] dialogosNarrador; // Sin valores por defecto para evitar confusión

    [Header("Visualización")]
    [Tooltip("Tiempo en segundos entre cada mensaje (si hay varios)")]
    public float tiempoEntreMensajes = 4.0f;
    [Tooltip("Duración de cada mensaje en pantalla (para mensajes únicos o el último de la secuencia)")]
    public float duracionMensaje = 4.0f;
    [Tooltip("Color del texto del narrador")]
    public Color colorTexto = Color.white;
    
    [Header("Audio")]
    [Tooltip("Sonido opcional al activarse (ej. voz, efecto, susurro)")]
    public AudioClip sonidoActivacion;
    [Range(0f, 1f)]
    public float volumenAudio = 1f;
    
    [Header("Comportamiento")]
    [Tooltip("Si es true, la secuencia solo se activará una vez")]
    public bool soloUnaVez = true;
    [Tooltip("Color para identificar este trigger en la escena (Gizmos)")]
    public Color colorGizmo = Color.cyan;

    private bool yaActivado = false;
    private AudioSource audioSource;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Si ya se activó y es de un solo uso, no hacer nada
        if (yaActivado && soloUnaVez) return;

        // Verificar si es el jugador
        if (other.CompareTag("Player"))
        {
            ReproducirSonido();
            IniciarSecuenciaDialogos();
            
            if (soloUnaVez)
            {
                yaActivado = true;
                // Desactivar collider para evitar múltiples triggers inmediatos
                // aunque yaActivado lo controla, esto es más eficiente para la física
                GetComponent<Collider2D>().enabled = false;
            }
        }
    }
    
    private void ReproducirSonido()
    {
        // Lógica adaptada de AreaHintTrigger.cs
        if (sonidoActivacion != null)
        {
            if (audioSource == null)
            {
                // Crear componente AudioSource nuevo para asegurar configuración limpia
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.clip = sonidoActivacion;
                audioSource.volume = volumenAudio;
                audioSource.spatialBlend = 1f; // 3D Sound (como en AreaHintTrigger)
                audioSource.loop = false;      // Diálogos normalmente no loop, pero aseguramos que suene
                audioSource.Play();
            }
            else if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }

    private void IniciarSecuenciaDialogos()
    {
        if (dialogosNarrador == null || dialogosNarrador.Length == 0) return;

        if (FloatingTextManager.Instance != null)
        {
            // Calcular posición para el texto (encima del jugador o del centro de la cabaña)
            // Usamos la posición del trigger como referencia base
            Vector3 posicionTexto = transform.position;
            
            FloatingTextManager.Instance.ShowFloatingTextSequence(
                dialogosNarrador, 
                posicionTexto, 
                colorTexto, 
                tiempoEntreMensajes,
                duracionMensaje // Pasamos la duración personalizada
            );
            
            Debug.Log("CabinDialogueTrigger: Secuencia de diálogos iniciada.");
        }
        else
        {
            Debug.LogWarning("CabinDialogueTrigger: No se encontró FloatingTextManager en la escena.");
        }
    }

    // Dibujar el área del trigger en el editor para facilitar su colocación
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(colorGizmo.r, colorGizmo.g, colorGizmo.b, 0.3f); 
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            Gizmos.DrawCube(transform.position + (Vector3)box.offset, box.size);
            Gizmos.color = colorGizmo;
            Gizmos.DrawWireCube(transform.position + (Vector3)box.offset, box.size);
        }
        else
        {
            Gizmos.DrawCube(transform.position, Vector3.one);
        }
    }
}
