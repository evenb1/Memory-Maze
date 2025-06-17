using UnityEngine;

public class AtmosphereController : MonoBehaviour
{
    void Start()
    {
        SetupAtmosphere();
    }
    
    void SetupAtmosphere()
    {
        // Enable fog
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.05f, 0.15f, 0.3f); // Dark blue
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 5f;
        RenderSettings.fogEndDistance = 20f;
        
        // Dim lighting for mood
        RenderSettings.ambientLight = new Color(0.1f, 0.2f, 0.4f);
        
        Debug.Log("Atmosphere effects applied!");
    }
}