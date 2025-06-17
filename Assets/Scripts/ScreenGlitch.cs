using UnityEngine;

public class ScreenGlitch : MonoBehaviour
{
    public Camera targetCamera;
    private Vector3 originalPosition;
    
    void Start()
    {
        if(targetCamera == null)
            targetCamera = Camera.main;
            
        originalPosition = targetCamera.transform.localPosition;
    }
    
    void Update()
    {
        // Random glitch chance
        if(Random.Range(0f, 1f) < 0.005f) // 0.5% chance per frame
        {
            StartCoroutine(SubtleGlitchShake());
        }
    }
    
    System.Collections.IEnumerator SubtleGlitchShake()
    {
        // Brief camera shake
        targetCamera.transform.localPosition = originalPosition + new Vector3(
            Random.Range(-0.05f, 0.05f),
            Random.Range(-0.05f, 0.05f),
            0
        );
        
        yield return new WaitForSeconds(0.02f);
        
        // Another quick shake
        targetCamera.transform.localPosition = originalPosition + new Vector3(
            Random.Range(-0.03f, 0.03f),
            Random.Range(-0.03f, 0.03f),
            0
        );
        
        yield return new WaitForSeconds(0.02f);
        
        // Return to normal
        targetCamera.transform.localPosition = originalPosition;
    }
}