using UnityEngine;
using UnityEngine.UI;

public class BlinkingCursor : MonoBehaviour
{
    public Text targetText;
    private string originalText;
    public float blinkSpeed = 1f;
    
    void Start()
    {
        if(targetText != null)
            originalText = targetText.text;
    }
    
    void Update()
    {
        if(targetText != null)
        {
            float blink = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            if(blink > 0.5f)
                targetText.text = originalText + " _";
            else
                targetText.text = originalText;
        }
    }
}