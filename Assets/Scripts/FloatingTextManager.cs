using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance;
    public GameObject floatingTextPrefab;
    
    private FloatingText currentFloatingText;
    private Coroutine textSequenceCoroutine;
    
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
    }
    
    // Helper to get reference
    public FloatingText GetCurrentFloatingText()
    {
        return currentFloatingText;
    }
    
    // Helper to clear reference safely
    public void ClearReference(FloatingText text)
    {
        if (currentFloatingText == text)
        {
            currentFloatingText = null;
        }
    }
    
    public bool ShowFloatingText(string message, Vector3 position, Color? color = null, 
                                bool useBackground = false, Color? backgroundColor = null,
                                bool useBackgroundBorder = false, Color? backgroundBorderColor = null,
                                float duration = 3.0f,
                                AudioClip sound = null, float volume = 1f)
    {
        if (floatingTextPrefab == null)
        {
            Debug.LogWarning("FloatingTextManager: No se ha asignado el Prefab de Texto Flotante. Intentando fallback...");
            return false;
        }
        
        // Detener secuencia anterior si existe
        if (textSequenceCoroutine != null)
        {
            StopCoroutine(textSequenceCoroutine);
        }
        
        // Check if object is actually alive (Unity Object check)
        if (currentFloatingText == null)
        {
            // Need to instantiate
            Vector3 spawnPosition = position + new Vector3(0, 1.5f, 0); // Más cerca del jugador
            GameObject textObj = Instantiate(floatingTextPrefab, spawnPosition, Quaternion.identity);
            currentFloatingText = textObj.GetComponent<FloatingText>();
            
            if (currentFloatingText == null)
            {
                Debug.LogError("FloatingTextManager: El prefab no tiene componente FloatingText.");
                return false;
            }
        }
        else
        {
            // Reposicionar texto existente justo encima del jugador
            currentFloatingText.transform.position = position + new Vector3(0, 1.5f, 0);
            currentFloatingText.gameObject.SetActive(true);
        }
        
        // Configurar texto, color y fondo usando el método helper
        currentFloatingText.SetText(message, useBackground, backgroundColor, useBackgroundBorder, backgroundBorderColor);
        
        // Configurar duración
        currentFloatingText.lifeTime = duration;

        // Sobreescribir color del texto si se especifica
        if (color.HasValue)
        {
            var textComp = currentFloatingText.GetComponentInChildren<Text>();
            if (textComp != null) textComp.color = color.Value;
        }
        
        // Reproducir sonido si existe
        if (sound != null)
        {
            AudioSource audioSource = currentFloatingText.GetComponent<AudioSource>();
            if (audioSource == null) audioSource = currentFloatingText.gameObject.AddComponent<AudioSource>();
            
            audioSource.clip = sound;
            audioSource.volume = volume;
            audioSource.spatialBlend = 1f; // 3D Sound
            audioSource.Play();
        }
        
        // Reiniciar temporizador
        currentFloatingText.ResetTimer();
        
        return true;
    }
    
    public void ShowFloatingTextSequence(string[] messages, Vector3 position, Color? color = null, float delayBetweenMessages = 4f, float durationOverride = -1f, AudioClip sequenceSound = null, float sequenceVolume = 1f)
    {
        if (textSequenceCoroutine != null)
        {
            StopCoroutine(textSequenceCoroutine);
        }
        
        // Si no se especifica duración, usar el delay como base
        float duration = (durationOverride > 0) ? durationOverride : delayBetweenMessages;

        textSequenceCoroutine = StartCoroutine(TextSequenceRoutine(messages, position, color, delayBetweenMessages, duration, sequenceSound, sequenceVolume));
    }
    
    private IEnumerator TextSequenceRoutine(string[] messages, Vector3 position, Color? color, float delay, float duration, AudioClip sound, float volume)
    {
        bool isFirst = true;
        foreach (string message in messages)
        {
            // Solo reproducir sonido en el primer mensaje
            AudioClip clipToPlay = isFirst ? sound : null;
            
            ShowFloatingText(message, position, color, false, null, false, null, duration, clipToPlay, volume);
            isFirst = false;
            yield return new WaitForSeconds(delay);
        }
    }
    
    public void ClearFloatingText()
    {
        if (currentFloatingText != null)
        {
            Destroy(currentFloatingText.gameObject);
            currentFloatingText = null;
        }
        
        if (textSequenceCoroutine != null)
        {
            StopCoroutine(textSequenceCoroutine);
            textSequenceCoroutine = null;
        }
    }
}
