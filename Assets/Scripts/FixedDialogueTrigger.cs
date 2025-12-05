using UnityEngine;
using System.Collections;

public class FixedDialogueTrigger : MonoBehaviour
{
    [Header("Configuración del Diálogo")]
    [TextArea(3, 5)]
    public string mensajeDialogo = "Texto del diálogo aquí";
    public Color colorTexto = Color.cyan;
    public float duracion = 4f;
    public bool soloUnaVez = true;
    
    [Header("Posición Fija")]
    public Vector3 posicionOffset = new Vector3(0, 2f, 0);
    
    private bool yaActivado = false;
    private GameObject dialogoInstance;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (yaActivado && soloUnaVez) return;
        
        if (other.CompareTag("Player"))
        {
            MostrarDialogoFijo();
            
            if (soloUnaVez)
            {
                yaActivado = true;
                GetComponent<Collider2D>().enabled = false;
            }
        }
    }
    
    private void MostrarDialogoFijo()
    {
        // Destruir diálogo anterior si existe
        if (dialogoInstance != null)
        {
            Destroy(dialogoInstance);
        }
        
        // Crear nuevo diálogo en posición fija
        Vector3 posicionFija = transform.position + posicionOffset;
        
        bool shown = false;
        if (FloatingTextManager.Instance != null)
        {
            shown = FloatingTextManager.Instance.ShowFloatingText(mensajeDialogo, posicionFija, colorTexto);
        }

        if (shown)
        {
            // Iniciar corrutina para ocultar después de 4 segundos
            StartCoroutine(OcultarDialogoDespuesDeTiempo(duracion));
        }
        else if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ShowDialogue("Narrador", mensajeDialogo);
            
            // Cerrar automáticamente después de 4 segundos
            StartCoroutine(CerrarDialogoDespuesDeTiempo(duracion));
        }
    }
    
    private IEnumerator OcultarDialogoDespuesDeTiempo(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        
        if (FloatingTextManager.Instance != null)
        {
            FloatingTextManager.Instance.ClearFloatingText();
        }
    }
    
    private IEnumerator CerrarDialogoDespuesDeTiempo(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive())
        {
            DialogueManager.Instance.CloseDialoguePanel();
        }
    }
    
    // Para debug en el editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider2D>().bounds.size);
        
        // Mostrar posición donde aparecerá el texto
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + posicionOffset, 0.3f);
    }
}