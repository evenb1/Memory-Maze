using UnityEngine;

public class DataFlicker : MonoBehaviour
{
    public float flickerSpeed = 0.1f;
    private Renderer panelRenderer;
    private Material originalMaterial;
    private bool isFlickering = false;
    
    void Start()
    {
        panelRenderer = GetComponent<Renderer>();
        if (panelRenderer != null)
        {
            originalMaterial = panelRenderer.material;
        }
        
        InvokeRepeating("FlickerEffect", Random.Range(0f, 2f), flickerSpeed);
    }
    
    void FlickerEffect()
    {
        if (panelRenderer == null || originalMaterial == null) return;
        
        if (Random.Range(0f, 1f) < 0.1f) // 10% chance to flicker
        {
            StartCoroutine(Flicker());
        }
    }
    
    System.Collections.IEnumerator Flicker()
    {
        if (isFlickering) yield break;
        isFlickering = true;
        
        // Turn off emission briefly
        Color originalEmission = originalMaterial.GetColor("_EmissionColor");
        originalMaterial.SetColor("_EmissionColor", Color.black);
        
        yield return new WaitForSeconds(0.05f);
        
        // Turn back on
        originalMaterial.SetColor("_EmissionColor", originalEmission);
        
        yield return new WaitForSeconds(0.02f);
        
        // Quick flicker again
        originalMaterial.SetColor("_EmissionColor", Color.black);
        yield return new WaitForSeconds(0.03f);
        originalMaterial.SetColor("_EmissionColor", originalEmission);
        
        isFlickering = false;
    }
}