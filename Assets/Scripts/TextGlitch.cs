using UnityEngine;
using TMPro;

public class TextGlitch : MonoBehaviour
{
    private TextMeshProUGUI text;
    private string originalText;
    
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        if(text != null)
            originalText = text.text;
        
        InvokeRepeating("GlitchEffect", 2f, 3f);
    }
    
    void GlitchEffect()
    {
        StartCoroutine(DoGlitch());
    }
    
    System.Collections.IEnumerator DoGlitch()
    {
        if(text != null)
        {
            // Brief text corruption
            text.text = "M3M0RY M4Z3\nV1RU5 35C4P3";
            yield return new WaitForSeconds(0.1f);
            text.text = originalText;
        }
    }
}