using UnityEngine;
using UnityEngine.UI;

public class ButtonEffects : MonoBehaviour
{
    private Button button;
    private Vector3 originalScale;
    
    void Start()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale;
    }
    
    void Update()
    {
        // Simple hover detection
        if(button.IsInteractable())
        {
            Vector3 mousePos = Input.mousePosition;
            RectTransform rect = GetComponent<RectTransform>();
            
            if(RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos))
            {
                transform.localScale = originalScale * 1.1f;
            }
            else
            {
                transform.localScale = originalScale;
            }
        }
    }
    
    public void OnButtonClick()
    {
        // Scale down briefly when clicked
        transform.localScale = originalScale * 0.9f;
        Invoke("ResetScale", 0.1f);
    }
    
    void ResetScale()
    {
        transform.localScale = originalScale * 1.1f;
    }
}