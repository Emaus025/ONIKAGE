using UnityEngine;

public class TraumaShadowTrigger : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Arrastra aquí el objeto TraumaShadow que tienes en la escena (debe estar desactivado inicialmente)")]
    public GameObject traumaShadowObject;
    public bool soloUnaVez = true;

    [Header("Aislamiento Visual (Pantalla Negra)")]
    [Tooltip("Activa esto si quieres que TODO desaparezca excepto el Background y la Sombra.")]
    public bool activarAislamientoVisual = true; // Renombrado para forzar actualización a true en Inspector
    [Tooltip("Selecciona aquí las capas que se deben VER (ej: Player, Background). Todo lo demás se ocultará.")]
    public LayerMask capasVisibles;
    [Tooltip("Color de fondo cuando se activa el aislamiento (normalmente Negro).")]
    public Color colorFondoAislamiento = Color.black;

    [Header("Aislamiento Específico (Grid/SolidObjects)")]
    [Tooltip("Si activas esto, el script buscará específicamente el objeto 'SolidObjects' dentro de 'Grid' y se asegurará de que sea visible.")]
    public bool mantenerSolidObjectsVisible = true; // Renombrado para forzar actualización a true
    [Tooltip("Nombre exacto del objeto padre (ej: Grid)")]
    public string nombreGridPadre = "Grid";
    [Tooltip("Nombre exacto del objeto hijo (ej: SolidObjects)")]
    public string nombreSolidObjects = "SolidObjects";

    private bool yaActivado = false;
    private bool restaurado = false; // Nuevo flag para control de seguridad

    // Variables para restaurar el estado
    private System.Collections.Generic.List<SpriteRenderer> renderersOcultados = new System.Collections.Generic.List<SpriteRenderer>();
    private CameraClearFlags originalClearFlags;
    private Color originalBackgroundColor;
    private Camera mainCameraUsed;

    private void Update()
    {
        // SAFETY NET: Si la sombra se desactivó (terminó) y no hemos restaurado el mundo, forzar restauración.
        // Esto evita que el jugador se quede invisible si el evento falla o la sombra se apaga externamente.
        if (yaActivado && !restaurado && traumaShadowObject != null)
        {
            if (!traumaShadowObject.activeSelf)
            {
                Debug.LogWarning("TraumaShadowTrigger: Detectado cierre de sombra sin evento (Safety Net). Restaurando mundo...");
                RestaurarMundo();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (yaActivado && soloUnaVez) return;

        restaurado = false;

        if (other.CompareTag("Player"))
        {
            Debug.Log("TraumaShadowTrigger: Jugador detectado.");
            if (traumaShadowObject != null)
            {
                // FIX: Verificar si se asignó EXACTAMENTE el mismo objeto que entró (el jugador real)
                if (traumaShadowObject == other.gameObject)
                {
                     Debug.LogError("TraumaShadowTrigger: ¡ERROR CRÍTICO! Has asignado al propio JUGADOR (el que se mueve) en el campo 'TraumaShadowObject'. Debes asignar el OTRO objeto (la sombra/copia) que está desactivado en la escena.");
                     return;
                }

                // FIX: Si el objeto sombra es DIFERENTE pero tiene el tag "Player" por error, corregirlo.
                if (traumaShadowObject.CompareTag("Player"))
                {
                     Debug.LogWarning($"TraumaShadowTrigger: El objeto sombra '{traumaShadowObject.name}' tiene el tag 'Player'. Esto causa conflictos. Cambiando tag a 'Untagged' automáticamente.");
                     traumaShadowObject.tag = "Untagged";
                }

                Debug.Log($"TraumaShadowTrigger: Activando objeto '{traumaShadowObject.name}'.");
                traumaShadowObject.SetActive(true);

                // FIX: Desactivar Cámara y AudioListener si existen para evitar "Pantalla Amarilla/Azul"
                // Buscamos en los hijos también, ya que la cámara suele estar emparentada al jugador
                Camera[] cams = traumaShadowObject.GetComponentsInChildren<Camera>(true);
                foreach (Camera c in cams)
                {
                    Debug.LogWarning($"TraumaShadowTrigger: Se encontró una CÁMARA en '{c.gameObject.name}' (hijo de la sombra). Desactivándola para evitar conflictos de pantalla.");
                    c.enabled = false;
                    c.gameObject.SetActive(false); // Desactivar el objeto de la cámara también por seguridad
                }
                
                AudioListener[] listeners = traumaShadowObject.GetComponentsInChildren<AudioListener>(true);
                foreach (AudioListener l in listeners)
                {
                     Destroy(l); // Mejor destruir el listener extra
                }

                // FIX: Asegurar que el componente TraumaShadow esté presente
                TraumaShadow ts = traumaShadowObject.GetComponent<TraumaShadow>();
                if (ts == null)
                {
                    Debug.LogWarning($"TraumaShadowTrigger: El objeto '{traumaShadowObject.name}' NO tiene el script 'TraumaShadow'. Añadiéndolo automáticamente.");
                    ts = traumaShadowObject.AddComponent<TraumaShadow>();
                }
                
                // Suscribirse al evento de finalización para restaurar todo
                ts.OnDesaparecerCompleto = RestaurarMundo;

                // NUEVO: Efecto de Aislamiento (Ocultar todo menos Background y Sombra)
                if (activarAislamientoVisual)
                {
                    renderersOcultados.Clear();

                    // MODO 1: Intentar configurar la cámara (Culling Mask + Color)
                    Camera mainCam = Camera.main;
                    if (mainCam != null)
                    {
                        Debug.Log("TraumaShadowTrigger: Configurando Cámara Principal (Fondo Negro).");
                        
                        // Guardar estado original
                        mainCameraUsed = mainCam;
                        originalClearFlags = mainCam.clearFlags;
                        originalBackgroundColor = mainCam.backgroundColor;

                        mainCam.clearFlags = CameraClearFlags.SolidColor;
                        // FIX: Asegurar que el color sea 100% negro
                        mainCam.backgroundColor = Color.black; 
                    }
                    else
                    {
                         Debug.LogError("TraumaShadowTrigger: No se encontró Camera.main. Busca una cámara etiquetada como MainCamera.");
                    }

                    // MODO 2: DESACTIVACIÓN BRUTA DE RENDERERS (Más seguro si las capas fallan)
                    // Buscamos TODOS los SpriteRenderers de la escena y apagamos los que no queremos ver.
                    SpriteRenderer[] allRenderers = FindObjectsOfType<SpriteRenderer>();
                    
                    // Identificar objetos a MANTENER
                    GameObject backgroundObj = GameObject.Find("Background"); // Nombre común
                    if (backgroundObj == null) backgroundObj = GameObject.Find("Fondo"); // Intento alternativo
                    
                    GameObject gridObj = GameObject.Find(nombreGridPadre);
                    Transform solidObj = null;
                    if (gridObj != null) solidObj = gridObj.transform.Find(nombreSolidObjects);

                    foreach (SpriteRenderer sr in allRenderers)
                    {
                        if (sr == null) continue;

                        // 1. Es la Sombra? (Mantener)
                        if (sr.gameObject == traumaShadowObject || sr.transform.IsChildOf(traumaShadowObject.transform))
                        {
                            sr.enabled = true;
                            continue;
                        }

                        // 2. Es el Background? (Mantener)
                        if (backgroundObj != null && (sr.gameObject == backgroundObj || sr.transform.IsChildOf(backgroundObj.transform)))
                        {
                            sr.enabled = true;
                            continue;
                        }

                        // 3. Son los SolidObjects? (Mantener si se solicita)
                        if (mantenerSolidObjectsVisible && solidObj != null && (sr.gameObject == solidObj.gameObject || sr.transform.IsChildOf(solidObj)))
                        {
                            sr.enabled = true;
                            continue;
                        }

                        // 4. Es el Jugador original? (Ocultar explícitamente para evitar conflictos)
                        if (sr.CompareTag("Player"))
                        {
                            sr.enabled = false;
                            renderersOcultados.Add(sr); // Guardar para restaurar
                            continue;
                        }

                        // Si no es ninguno de los anteriores, OCULTAR
                        if (sr.enabled)
                        {
                            sr.enabled = false;
                            renderersOcultados.Add(sr); // Guardar para restaurar
                        }
                    }
                    
                    Debug.Log($"TraumaShadowTrigger: Aislamiento completado. {renderersOcultados.Count} objetos ocultados temporalmente.");
                }

                yaActivado = true;
            }
            else
            {
                Debug.LogError($"TraumaShadowTrigger: El objeto '{gameObject.name}' no tiene asignado el 'TraumaShadowObject' en el inspector. Por favor asígnalo.");
            }
        }
    }

    private void RestaurarMundo()
    {
        if (restaurado) return; // Evitar doble restauración
        restaurado = true;

        Debug.Log("TraumaShadowTrigger: Restaurando mundo...");

        // 1. Restaurar Cámara
        if (mainCameraUsed != null)
        {
            mainCameraUsed.clearFlags = originalClearFlags;
            mainCameraUsed.backgroundColor = originalBackgroundColor;
        }

        // 2. Restaurar Renderers ocultados
        foreach (SpriteRenderer sr in renderersOcultados)
        {
            if (sr != null)
            {
                sr.enabled = true;
            }
        }
        renderersOcultados.Clear();

        Debug.Log("TraumaShadowTrigger: Mundo restaurado.");
    }

    private void OnDrawGizmos()
    {
        // Gizmos desactivados para no mostrar el cuadro azul
        // Gizmos.color = new Color(0.5f, 0, 0.5f, 0.4f);
        // Gizmos.DrawCube(transform.position, transform.localScale);
    }
}
