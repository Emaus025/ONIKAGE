using UnityEngine;

public class AreaHintTrigger : MonoBehaviour
{
    [Header("Mensaje Contextual")]
    [TextArea(3, 5)]
    public string mensajeHint = "Sigue el camino para poder llegar a tu destino.";
    
    [Header("Configuración")]
    public Color colorTexto = Color.white; // Changed default to white per request
    public bool soloUnaVez = true;
    
    [Header("Audio")]
    public AudioClip sonidoHint;
    [Range(0f, 1f)]
    public float volumen = 1f;

    private bool yaActivado = false;
    private AudioSource audioSource;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (yaActivado && soloUnaVez) return;
        
        if (other.CompareTag("Player"))
        {
            MostrarHint();
            
            if (soloUnaVez)
            {
                yaActivado = true;
                // Opcional: desactivar el collider después de usarlo
                GetComponent<Collider2D>().enabled = false;
            }
        }
    }
    
    private void MostrarHint()
    {
        // Reproducir sonido en bucle si existe
        if (sonidoHint != null)
        {
            if (audioSource == null)
            {
                // Crear componente AudioSource si no existe
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.clip = sonidoHint;
                audioSource.volume = volumen;
                audioSource.loop = true; // Activar repetición (loop)
                audioSource.spatialBlend = 1f; // 3D Sound (o 0 para 2D si se prefiere)
                audioSource.Play();
            }
            else if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }

        // Usar Floating Text si está disponible
        bool textShown = false;
        if (FloatingTextManager.Instance != null)
        {
            Vector3 posicion = transform.position + new Vector3(0, 2f, 0);
            
            textShown = FloatingTextManager.Instance.ShowFloatingText(
                mensajeHint, 
                posicion, 
                colorTexto,                 
                false,                      // Fondo desactivado
                Color.black,                
                false,                      // Borde desactivado
                Color.white                 
            );
        }
        
        // Fallback al sistema de diálogos si no se mostró el texto flotante (por falta de prefab o manager)
        if (!textShown && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ShowDialogue("Pista", mensajeHint);
        }
        
        Debug.Log("Hint mostrado: " + mensajeHint);
    }
    
    // Para debug en el editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider2D>() ? GetComponent<BoxCollider2D>().size : Vector3.one);
    }
}