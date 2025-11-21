using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorController : MonoBehaviour
{
    [Header("Configuración de la Puerta")]
    public string sceneToLoad; // Nombre de la escena a cargar
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Si el jugador toca la puerta, cambiar de escena
        if (other.CompareTag("Player"))
        {
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.LogWarning("No se ha asignado una escena para cargar en la puerta: " + gameObject.name);
            }
        }
    }
}