using UnityEngine;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float fadeTime = 1f;
    public float lifeTime = 3f;
    
    // Background configuration
    public bool useBackground = false;
    public Color backgroundColor = Color.black;
    public Vector2 padding = new Vector2(20, 10);
    
    // Border configuration
    public bool useBackgroundBorder = false;
    public Color backgroundBorderColor = Color.white;
    
    private Text textComponent;
    private Image backgroundImage;
    private float alpha = 1f;
    private float timer = 0f;

    private void OnDestroy()
    {
        // Notificar al manager si soy el texto actual
        if (FloatingTextManager.Instance != null && FloatingTextManager.Instance.GetCurrentFloatingText() == this)
        {
            FloatingTextManager.Instance.ClearReference(this);
        }
    }

    public void SetText(string text, bool useBg = false, Color? bgColor = null, bool useBorder = false, Color? borderColor = null)
    {
        // Update settings
        this.useBackground = useBg;
        if (bgColor.HasValue) this.backgroundColor = bgColor.Value;
        this.useBackgroundBorder = useBorder;
        if (borderColor.HasValue) this.backgroundBorderColor = borderColor.Value;

        // Find Text component
        textComponent = GetComponent<Text>();
        if (textComponent == null)
        {
            textComponent = GetComponentInChildren<Text>();
        }

        if (textComponent != null)
        {
            textComponent.text = text;
            // Configure alignment for better background centering
            textComponent.alignment = TextAnchor.MiddleCenter; 
            SetupBackground();
        }
        else
        {
            Debug.LogError("FloatingText: No se encontró componente Text en el prefab.");
        }
    }

    private void SetupBackground()
    {
        // Validar que no estemos destruidos
        if (this == null || gameObject == null) return;

        if (!useBackground)
        {
            Transform bg = transform.Find("Background");
            if (bg != null) bg.gameObject.SetActive(false);
            return;
        }

        Transform bgTransform = transform.Find("Background");
        if (bgTransform == null)
        {
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(transform, false);
            bgTransform = bgObj.transform;
        }
        
        if (bgTransform == null) return; // Safety check
        
        bgTransform.gameObject.SetActive(true);

        // Setup Image
        backgroundImage = bgTransform.GetComponent<Image>();
        if (backgroundImage == null) backgroundImage = bgTransform.gameObject.AddComponent<Image>();
        backgroundImage.color = backgroundColor;

        // Setup Border (Outline)
        Outline outline = bgTransform.GetComponent<Outline>();
        if (useBackgroundBorder)
        {
            if (outline == null) outline = bgTransform.gameObject.AddComponent<Outline>();
            outline.enabled = true;
            outline.effectColor = backgroundBorderColor;
            outline.effectDistance = new Vector2(2, -2);
        }
        else
        {
            if (outline != null) outline.enabled = false;
        }

        // Render Order & Positioning
        bgTransform.SetAsFirstSibling();
        bgTransform.localPosition = new Vector3(0, 0, 1f); 
        
        // Adjust size to fit text
        if (textComponent != null)
        {
             // Validar componente text
             RectTransform textRect = textComponent.GetComponent<RectTransform>();
             if (textRect == null) return;

             RectTransform bgRect = bgTransform.GetComponent<RectTransform>();
             if (bgRect == null) return;

            // Calculate size based on text preferred size
            float width = textComponent.preferredWidth + padding.x;
            float height = textComponent.preferredHeight + padding.y;
            
            bgRect.sizeDelta = new Vector2(width, height);
            
            // Center background on text
            bgRect.anchorMin = new Vector2(0.5f, 0.5f);
            bgRect.anchorMax = new Vector2(0.5f, 0.5f);
            bgRect.pivot = new Vector2(0.5f, 0.5f);
            
            // If textComponent is on a child, match that child's local position
            if (textComponent.gameObject != gameObject)
            {
                bgRect.localPosition = textComponent.transform.localPosition;
                textComponent.transform.SetSiblingIndex(1);
                bgTransform.SetSiblingIndex(0);
            }
            else
            {
                 bgRect.localPosition = new Vector3(0, 0, 0.1f); 
            }
        }
    }

    public void ResetTimer()
    {
        timer = 0f;
        alpha = 1f;
        
        if (textComponent == null) 
        {
            textComponent = GetComponent<Text>();
            if (textComponent == null) textComponent = GetComponentInChildren<Text>();
        }

        if (textComponent != null)
        {
            Color color = textComponent.color;
            color.a = 1f;
            textComponent.color = color;
        }
        
        if (backgroundImage != null)
        {
            Color bgColor = backgroundImage.color;
            bgColor.a = 1f; // Ensure BG is visible
            backgroundImage.color = bgColor;
        }
    }

    private void Update()
    {
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
        timer += Time.deltaTime;
        
        if (timer > (lifeTime - fadeTime))
        {
            alpha -= Time.deltaTime / fadeTime;
            
            // Fade Text
            if (textComponent != null)
            {
                Color color = textComponent.color;
                color.a = alpha;
                textComponent.color = color;
            }
            
            // Fade Background
            if (backgroundImage != null)
            {
                Color bgColor = backgroundImage.color;
                bgColor.a = alpha;
                backgroundImage.color = bgColor;
                
                // Fade Outline?
                Outline outline = backgroundImage.GetComponent<Outline>();
                if (outline != null)
                {
                    Color olColor = outline.effectColor;
                    olColor.a = alpha;
                    outline.effectColor = olColor;
                }
            }
        }
        
        // Desactivar en lugar de destruir para permitir reutilización
        if (timer >= lifeTime)
        {
            gameObject.SetActive(false);
        }
    }
}
