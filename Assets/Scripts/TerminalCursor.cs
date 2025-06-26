using UnityEngine;
using TMPro;

public class TerminalCursor : MonoBehaviour
{
    [Header("Cursor Settings")]
    public float blinkSpeed = 1f;
    public string cursorCharacter = "█"; // Block cursor
    public Color cursorColor = Color.white;
    
    [Header("Position")]
    public TextMeshProUGUI targetText; // Text to follow
    public Vector2 offset = Vector2.zero;
    
    private TextMeshProUGUI cursorText;
    private bool isVisible = true;
    private float blinkTimer = 0f;
    
    void Start()
    {
        // Get or create the cursor text component
        cursorText = GetComponent<TextMeshProUGUI>();
        if (cursorText == null)
        {
            cursorText = gameObject.AddComponent<TextMeshProUGUI>();
        }
        
        // Setup cursor appearance
        cursorText.text = cursorCharacter;
        cursorText.color = cursorColor;
        cursorText.fontSize = targetText != null ? targetText.fontSize : 24;
        cursorText.font = targetText != null ? targetText.font : null;
        
        // Position cursor
        PositionCursor();
    }
    
    void Update()
    {
        // Handle blinking
        blinkTimer += Time.deltaTime;
        if (blinkTimer >= 1f / blinkSpeed)
        {
            isVisible = !isVisible;
            cursorText.alpha = isVisible ? 1f : 0f;
            blinkTimer = 0f;
        }
        
        // Update position if following text
        if (targetText != null)
        {
            PositionCursor();
        }
    }
    
    void PositionCursor()
    {
        if (targetText == null) return;
        
        // Position cursor at the end of the text
        RectTransform textRect = targetText.rectTransform;
        RectTransform cursorRect = cursorText.rectTransform;
        
        // Copy text alignment and positioning
        cursorRect.anchorMin = textRect.anchorMin;
        cursorRect.anchorMax = textRect.anchorMax;
        cursorRect.anchoredPosition = textRect.anchoredPosition + offset;
        
        // Size the cursor appropriately
        cursorRect.sizeDelta = new Vector2(cursorText.fontSize * 0.6f, cursorText.fontSize);
    }
    
    public void SetCursorVisible(bool visible)
    {
        isVisible = visible;
        cursorText.alpha = visible ? 1f : 0f;
    }
    
    public void SetBlinkSpeed(float speed)
    {
        blinkSpeed = speed;
    }
}