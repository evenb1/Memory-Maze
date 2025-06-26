using UnityEngine;

public class GlowPulse : MonoBehaviour
{
    public Color baseColor = Color.cyan;
    public float pulseSpeed = 2f;
    public float minIntensity = 0.2f;
    public float maxIntensity = 0.8f;
    
    private Renderer wallRenderer;
    private Material wallMaterial;
    
    void Start()
    {
        wallRenderer = GetComponent<Renderer>();
        if (wallRenderer != null)
        {
            wallMaterial = wallRenderer.material;
        }
    }
    
    void Update()
    {
        if (wallMaterial != null && wallMaterial.HasProperty("_EmissionColor"))
        {
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);
            wallMaterial.SetColor("_EmissionColor", baseColor * intensity);
        }
    }
}
