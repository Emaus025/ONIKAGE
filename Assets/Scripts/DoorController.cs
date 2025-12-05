using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class DoorController : MonoBehaviour
{
    [Header("Configuración de la Puerta")]
    public string sceneToLoad; // Nombre de la escena a cargar
    private bool hasLoaded;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasLoaded) return;

        bool isPlayer = other.CompareTag("Player") || other.GetComponentInParent<PlayerController>() != null;
        if (!isPlayer) return;

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("DoorController: 'sceneToLoad' vacío en " + gameObject.name);
            return;
        }

        hasLoaded = true;
        SceneManager.LoadScene(sceneToLoad);
    }
}