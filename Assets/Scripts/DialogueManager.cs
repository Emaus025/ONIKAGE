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
    }

    private void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(continueKey))
        {
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
        dialogueText.text = "";
        currentSentence = line.message;

        foreach (char letter in currentSentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void CompleteSentence()
    {
        StopAllCoroutines();
        dialogueText.text = currentSentence;
        isTyping = false;
    }

    private void EndDialogue()
    {
        isDialogueActive = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        Debug.Log("Diálogo terminado");
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
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