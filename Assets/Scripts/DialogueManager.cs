using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public Text dialogueText;
    public Text speakerNameText;
    public Image speakerPortrait;

    [Header("Configuración")]
    public float typingSpeed = 0.05f;
    public KeyCode continueKey = KeyCode.Space;

    // Evento cuando termina el diálogo
    public System.Action OnDialogueFinished;

    private Queue<DialogueLine> dialogueQueue;
    
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private string currentSentence;

    [System.Serializable]
    public struct DialogueLine
    {
        public string speaker;
        public string message;
        public Sprite portrait;
    }

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

        dialogueQueue = new Queue<DialogueLine>();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Verificación de seguridad
        if (dialogueText == null)
            Debug.LogError("¡ERROR CRÍTICO! No se ha asignado 'Dialogue Text' en el Inspector de DialogueManager. Los diálogos no funcionarán.");
        if (dialoguePanel == null)
            Debug.LogError("¡ADVERTENCIA! No se ha asignado 'Dialogue Panel' en el Inspector de DialogueManager.");
    }

    private void Start()
    {
        // Diagnóstico de inicio
        Debug.Log($"[DialogueManager] Inicializado. Panel asignado: {(dialoguePanel != null ? "SÍ" : "NO")}. Texto asignado: {(dialogueText != null ? "SÍ" : "NO")}.");
        
        if (dialoguePanel == null || dialogueText == null)
        {
            Debug.LogError("⚠️ [ATENCIÓN] Faltan referencias en el DialogueManager. Arrastra el Panel y el Texto en el Inspector.");
        }
    }

    private void Update()
    {
        // Debug para ver si detecta la tecla
        if (isDialogueActive && Input.GetKeyDown(continueKey))
        {
            Debug.Log("Tecla de continuación detectada");
            if (isTyping)
            {
                CompleteSentence();
            }
            else
            {
                DisplayNextLine();
            }
        }
    }

    public void ShowDialogue(string speaker, string message)
    {
        DialogueLine line = new DialogueLine
        {
            speaker = speaker,
            message = message,
            portrait = null
        };

        ShowDialogue(line);
    }

    public void ShowDialogue(DialogueLine line)
    {
        dialogueQueue.Enqueue(line);

        if (!isDialogueActive)
        {
            StartDialogue();
        }
    }

    public void ShowDialogueSequence(List<DialogueLine> lines)
    {
        foreach (DialogueLine line in lines)
        {
            dialogueQueue.Enqueue(line);
        }

        if (!isDialogueActive)
        {
            StartDialogue();
        }
    }

    private void StartDialogue()
    {
        isDialogueActive = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        DisplayNextLine();
    }

    private void DisplayNextLine()
    {
        if (dialogueQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = dialogueQueue.Dequeue();
        StartCoroutine(TypeSentence(line));
    }

    private IEnumerator TypeSentence(DialogueLine line)
    {
        isTyping = true;

        // Configurar UI
        if (speakerNameText != null)
            speakerNameText.text = line.speaker;

        if (speakerPortrait != null && line.portrait != null)
            speakerPortrait.sprite = line.portrait;

        // Escribir texto caracter por caracter
        if (dialogueText != null)
        {
            dialogueText.text = "";
            currentSentence = line.message;

            foreach (char letter in currentSentence.ToCharArray())
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }
        }
        else
        {
            Debug.LogError("DialogueManager: 'Dialogue Text' es NULL. No se puede mostrar el texto.");
            yield return new WaitForSeconds(0.5f); // Evitar bloqueo si falta la UI
        }

        isTyping = false;
    }

    private void CompleteSentence()
    {
        StopAllCoroutines();
        if (dialogueText != null)
        {
            dialogueText.text = currentSentence;
        }
        isTyping = false;
    }

    private void EndDialogue()
    {
        isDialogueActive = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        Debug.Log("Diálogo terminado");
        OnDialogueFinished?.Invoke();
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    public void CloseDialoguePanel()
    {
        isDialogueActive = false;
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    // Método rápido para diálogos del Nivel 2
    public void ShowFukurōDialogue(string message)
    {
        ShowDialogue("Fukurō", message);
    }

    public void ShowYamishiDialogue(string message)
    {
        ShowDialogue("Yamishi", message);
    }
}