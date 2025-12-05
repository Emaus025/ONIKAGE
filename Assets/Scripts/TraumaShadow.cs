using UnityEngine;

public class TraumaShadow : MonoBehaviour
{
    [Header("Configuración Visual")]
    [Tooltip("Color base de la sombra. La transparencia (Alpha) configurada aquí será la inicial.")]
    public Color colorSombra = new Color(1f, 1f, 1f, 1f); // Alpha 1f = Totalmente opaco
    [Tooltip("Escala relativa al tamaño original (1.0 = tamaño normal)")]
    public float escala = 0.7f;
    
    [Header("Renderizado")]
    [Tooltip("Si es true, copiará la Capa de Ordenamiento (Sorting Layer) del jugador. Si es false, usará la configurada en el Inspector.")]
    public bool copiarCapaJugador = false;
    [Tooltip("Diferencia de orden respecto al jugador (ej. -1 para estar detrás, 1 para estar delante). Solo funciona si copiarCapaJugador es true.")]
    public int ordenOffset = -1;

    [Header("Comportamiento")]
    [Tooltip("Distancia a la que la sombra comienza a desaparecer")]
    public float distanciaDesaparicion = 3.0f;
    [Tooltip("Velocidad a la que se desvanece (Menor valor = Más lento)")]
    public float velocidadDesvanecimiento = 2f;
    [Tooltip("Si es true, la sombra mirará al jugador (usando su Animator). Si es false, copiará exactamente el Sprite del jugador frame a frame.")]
    public bool mirarAlJugador = false;

    private Transform playerTransform;
    private Animator playerAnimator;
    private SpriteRenderer playerSpriteRenderer;
    
    private Animator myAnimator;
    private SpriteRenderer mySpriteRenderer;
    private bool desapareciendo = false;

    void OnEnable()
    {
        desapareciendo = false;
        // Buscar referencia si aún no la tenemos
        if (mySpriteRenderer == null) mySpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // BUSCAR JUGADOR AL ACTIVAR (Por si acaso Start ya ocurrió o el jugador cambió)
        BuscarJugador();

        if (mySpriteRenderer != null)
        {
            // Reinicia usando EXCLUSIVAMENTE el colorSombra (normalizando alpha si es necesario)
            // FIX: Asegurar que el color no sea excesivamente brillante si no es HDR
            Color colorInicial = colorSombra;
            if (colorInicial.a > 1f) colorInicial.a = 1f; 
            
            mySpriteRenderer.color = colorInicial;
            
            // FIX: Copiar sprite INMEDIATAMENTE para evitar que se vea un cuadro blanco/vacío
            ImitarJugador();
        }
    }

    void Start()
    {
        // Inicializar componentes propios primero
        myAnimator = GetComponent<Animator>();
        // Buscar SpriteRenderer en este objeto o en sus hijos
        if (mySpriteRenderer == null) mySpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (mySpriteRenderer == null) Debug.LogError("TraumaShadow: Falta el componente SpriteRenderer (ni en este objeto ni en hijos).");
        
        BuscarJugador();

        // Configurar escala
        transform.localScale = new Vector3(escala, escala, 1f);

        // Asegurar posición Z correcta
        Vector3 pos = transform.position;
        pos.z = 0;
        transform.position = pos;
        
        // Si tenemos Animator y queremos usarlo, intentar copiar el controlador
        if (myAnimator != null && playerAnimator != null && myAnimator.runtimeAnimatorController == null)
        {
             myAnimator.runtimeAnimatorController = playerAnimator.runtimeAnimatorController;
        }
        
        // Primera imitación forzada
        ImitarJugador();
    }

    void BuscarJugador()
    {
        if (playerTransform != null) return; // Ya lo tenemos

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerAnimator = player.GetComponent<Animator>();
            playerSpriteRenderer = player.GetComponent<SpriteRenderer>();
            
            // Copiar la capa de ordenamiento del jugador SOLO si está configurado
            if (copiarCapaJugador && playerSpriteRenderer != null && mySpriteRenderer != null)
            {
                mySpriteRenderer.sortingLayerID = playerSpriteRenderer.sortingLayerID;
                mySpriteRenderer.sortingOrder = playerSpriteRenderer.sortingOrder + ordenOffset;
            }
        }
    }

    void LateUpdate()
    {
        if (playerTransform == null) return;

        // Calcular distancia
        float distancia = Vector3.Distance(transform.position, playerTransform.position);

        // Lógica de desaparición
        if (distancia < distanciaDesaparicion)
        {
            desapareciendo = true;
        }

        if (desapareciendo)
        {
            Desvanecer();
        }
        else
        {
            ImitarJugador();
        }

        // FIX: Forzar el color base (RGB) manteniendo el alpha actual para evitar cambios de color indeseados
        if (mySpriteRenderer != null)
        {
            Color current = mySpriteRenderer.color;
            Color target = colorSombra;
            target.a = current.a; // Mantener el alpha dinámico
            mySpriteRenderer.color = target;
        }
    }

    void ImitarJugador()
    {
        if (mirarAlJugador)
        {
            // MODO OBSERVADOR: Usar Animator para mirar al jugador
            if (myAnimator != null && myAnimator.runtimeAnimatorController != null)
            {
                if (!myAnimator.enabled) myAnimator.enabled = true;
                
                Vector2 direccion = (playerTransform.position - transform.position).normalized;
                myAnimator.SetFloat("moveX", direccion.x);
                myAnimator.SetFloat("moveY", direccion.y);
                myAnimator.SetBool("isMoving", false); // Quieto
            }
        }
        else
        {
            // MODO IMITACIÓN (COPIA DIRECTA): Copiar Sprite del jugador
            // Desactivamos nuestro Animator para que no sobrescriba el sprite
            if (myAnimator != null && myAnimator.enabled) myAnimator.enabled = false;

            if (playerSpriteRenderer != null && mySpriteRenderer != null)
            {
                // 1. Copiar Sprite
                if (mySpriteRenderer.sprite != playerSpriteRenderer.sprite)
                {
                    mySpriteRenderer.sprite = playerSpriteRenderer.sprite;
                }
                
                mySpriteRenderer.flipX = playerSpriteRenderer.flipX;
                mySpriteRenderer.flipY = playerSpriteRenderer.flipY;
                
                // FIX CRÍTICO: Asegurar que usamos el material del jugador para evitar cuadros magenta
                if (mySpriteRenderer.sharedMaterial != playerSpriteRenderer.sharedMaterial)
                {
                     if (playerSpriteRenderer.sharedMaterial != null)
                     {
                         mySpriteRenderer.sharedMaterial = playerSpriteRenderer.sharedMaterial;
                     }
                     else
                     {
                         // Fallback a material por defecto si el jugador tampoco tiene (raro)
                         if (mySpriteRenderer.sharedMaterial == null)
                         {
                             Material defaultMat = new Material(Shader.Find("Sprites/Default"));
                             mySpriteRenderer.sharedMaterial = defaultMat;
                         }
                     }
                }
            }
        }
    }

    public System.Action OnDesaparecerCompleto;

    void Desvanecer()
    {
        if (mySpriteRenderer == null) return;

        Color c = mySpriteRenderer.color;
        c.a -= Time.deltaTime * velocidadDesvanecimiento;
        mySpriteRenderer.color = c;

        if (c.a <= 0)
        {
            // Notificar a quien esté escuchando (el Trigger) que hemos terminado
            OnDesaparecerCompleto?.Invoke();

            // IMPORTANTE: Solo desactivar, NO destruir, para evitar errores de referencia
            // si el trigger intenta acceder a él de nuevo.
            gameObject.SetActive(false);
        }
    }
}