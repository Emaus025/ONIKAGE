using UnityEngine;

public class Playersnake : MonoBehaviour
{
    // --- Variables de Flotación ---
    // Distancia máxima que sube/baja el personaje (ej: 0.5 unidades)
    public float amplitud = 0.5f; 
    
    // Velocidad del movimiento (ej: 1 ciclo por segundo)
    public float frecuencia = 1f;  

    // Almacenará la posición vertical inicial para calcular el desplazamiento
    private Vector3 posicionOriginal;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 1. Guardamos la posición inicial del objeto al comenzar el juego.
        posicionOriginal = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // 2. Calculamos el desplazamiento vertical usando la función Seno.
        // Time.time avanza continuamente. Multiplicarlo por frecuencia controla la velocidad.
        // Multiplicarlo por amplitud controla la altura.
        float desplazamientoY = Mathf.Sin(Time.time * frecuencia) * amplitud;

        // 3. Aplicamos el desplazamiento a la posición original, 
        // solo modificando el componente Y (vertical).
        transform.position = posicionOriginal + new Vector3(0, desplazamientoY, 0);
    }
}
