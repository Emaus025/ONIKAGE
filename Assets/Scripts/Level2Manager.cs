using UnityEngine;
using System.Collections;

public class Level2Manager : MonoBehaviour
{
    public static Level2Manager Instance;

    [Header("Referencias Nivel 2")]
    public GameObject fukurōMentor;
    public GameObject samuraiHerido;
    public GameObject[] plataformasSombras;
    public DoorController puertaTemplo;

    [Header("Estados del Nivel")]
    public bool tutorialCompletado = false;
    public bool combateIniciado = false;
    public bool decisionTomada = false;
    public bool acertijoCompletado = false;

    [Header("Configuración Climática")]
    public string climaActual = "Normal";
    public ParticleSystem sistemaParticulasClima;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeLevel2();
        SetupClimateFromLevel1();
    }

    private void InitializeLevel2()
    {
        Debug.Log("Iniciando Nivel 2: Camino del Engaño");

        // Verificar que el Player tenga los sistemas necesarios
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Asegurar que tiene los componentes necesarios
            if (player.GetComponent<CombatManager>() == null)
                player.AddComponent<CombatManager>();

            if (player.GetComponent<CombatManager>() == null)
                player.AddComponent<CombatManager>();
        }

        // Iniciar tutorial
        StartCoroutine(TutorialSecuencia());
    }

    private void SetupClimateFromLevel1()
    {
        // Simular decisión del Nivel 1 (debes conectar esto con tu sistema real)
        if (GameManager.Instance != null)
        {
            // Aquí obtendrías las decisiones reales del Nivel 1
            climaActual = "Niebla"; // Ejemplo
            ApplyClimateEffects();
        }
    }

    private void ApplyClimateEffects()
    {
        if (sistemaParticulasClima != null)
        {
            switch (climaActual)
            {
                case "Lluvia":
                    sistemaParticulasClima.Play();
                    break;
                case "Niebla":
                    // Configurar niebla
                    RenderSettings.fog = true;
                    break;
            }
        }
    }

    private IEnumerator TutorialSecuencia()
    {
        yield return new WaitForSeconds(1f);

        // Fukurō da instrucciones iniciales
        if (fukurōMentor != null)
        {
            // Activar diálogo de tutorial
            DialogueManager.Instance.ShowDialogue("Fukurō", "Observa tu entorno, Onikage. Usa tu click izquierdo para romper obstáculos.");
            yield return new WaitForSeconds(3f);

            DialogueManager.Instance.ShowDialogue("Fukurō", "Presiona Shift para cambiar entre modos Sombra y Furia.");
            yield return new WaitForSeconds(3f);

            DialogueManager.Instance.ShowDialogue("Fukurō", "Usa E para interactuar con objetos y NPCs.");
        }

        tutorialCompletado = true;
    }

    public void TriggerCombate()
    {
        if (!combateIniciado)
        {
            combateIniciado = true;
            StartCoroutine(SecuenciaCombate());
        }
    }

    private IEnumerator SecuenciaCombate()
    {
        DialogueManager.Instance.ShowDialogue("Fukurō", "¡Espíritus Engañados se acercan! Recuerda: La furia ciega, la sombra engaña.");
        yield return new WaitForSeconds(2f);

        // Aquí activarías los enemigos
        ActivarEspiritusEngañados();
    }

    private void ActivarEspiritusEngañados()
    {
        GameObject[] espiritus = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject espiritu in espiritus)
        {
            espiritu.SetActive(true);
        }
    }

    public void OnMoralDecisionTaken(string decision)
    {
        decisionTomada = true;

        switch (decision)
        {
            case "Curar":
                MoralSystem.Instance.AddLuz(15);
                RevelarAtajoSecreto();
                break;
            case "Ignorar":
                // Camino normal - no hacer cambios
                break;
            case "Matar":
                MoralSystem.Instance.AddSombra(20);
                OtenerObjetoUnico();
                ActivarNPCsHostiles();
                break;
        }
    }

    private void RevelarAtajoSecreto()
    {
        Debug.Log("Atajo secreto revelado!");
        // Activar plataformas o camino oculto
    }

    private void OtenerObjetoUnico()
    {
        Debug.Log("Objeto único obtenido: Amuleto Oscuro");
        // Dar objeto al jugador
    }

    private void ActivarNPCsHostiles()
    {
        Debug.Log("NPCs se vuelven hostiles");
        // Cambiar comportamiento de NPCs
    }

    public void OnAcertijoCompletado()
    {
        acertijoCompletado = true;
        DialogueManager.Instance.ShowDialogue("Fukurō", "Tu pasado te persigue, pero puedes usarlo. Bien hecho.");

        // Activar transición al templo
        if (puertaTemplo != null)
        {
            puertaTemplo.gameObject.SetActive(true);
        }
    }
}