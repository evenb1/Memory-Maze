using UnityEngine;
using UnityEngine.Rendering;

public class SkyboxController : MonoBehaviour
{
    [Header("Level Skyboxes")]
    public Material level1Skybox;  // Clean/normal skybox
    public Material level2Skybox;  // Corrupted red/orange skybox
    public Material level3Skybox;  // Future: More corrupted
    public Material level4Skybox;  // Future: Completely corrupted
    
    [Header("Level 2 Corruption Settings")]
    public bool enableDynamicCorruption = true;
    public float corruptionIntensity = 1f;
    public Color baseCorruptionColor = new Color(0.8f, 0.2f, 0.1f); // Red/orange
    public float pulsationSpeed = 0.5f;
    
    [Header("Fragment Collection Effects")]
    public bool corruptionIncreasesWithFragments = true;
    public float corruptionPerFragment = 0.1f;
    
    private Material currentSkyboxMaterial;
    private GameManager gameManager;
    private int currentLevel = 2;
    private float baseAtmosphereThickness = 0.8f;
    private Color baseSkyTint;
    private bool isInitialized = false;
    
    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        InitializeSkybox();
    }
    
    void InitializeSkybox()
    {
        // Determine current level
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName.Contains("Level"))
        {
            string levelNumber = sceneName.Replace("Level", "");
            if (int.TryParse(levelNumber, out int level))
            {
                currentLevel = level;
            }
        }
        
        // Set appropriate skybox for current level
        SetSkyboxForLevel(currentLevel);
        
        Debug.Log($"Skybox initialized for Level {currentLevel}");
    }
    
    void SetSkyboxForLevel(int level)
    {
        Material targetSkybox = null;
        
        switch (level)
        {
            case 1:
                targetSkybox = level1Skybox;
                break;
            case 2:
                targetSkybox = level2Skybox;
                if (targetSkybox == null)
                {
                    CreateLevel2ProceduralSkybox();
                }
                break;
            case 3:
                targetSkybox = level3Skybox;
                break;
            case 4:
                targetSkybox = level4Skybox;
                break;
            default:
                Debug.LogWarning($"No skybox defined for Level {level}");
                break;
        }
        
        if (targetSkybox != null)
        {
            RenderSettings.skybox = targetSkybox;
            currentSkyboxMaterial = targetSkybox;
            
            // Store base values for corruption effects
            if (currentSkyboxMaterial.HasProperty("_SkyTint"))
            {
                baseSkyTint = currentSkyboxMaterial.GetColor("_SkyTint");
            }
            if (currentSkyboxMaterial.HasProperty("_AtmosphereThickness"))
            {
                baseAtmosphereThickness = currentSkyboxMaterial.GetFloat("_AtmosphereThickness");
            }
            
            // Force lighting update
            DynamicGI.UpdateEnvironment();
            isInitialized = true;
        }
    }
    
    void CreateLevel2ProceduralSkybox()
    {
        // Create a procedural skybox for Level 2 if none is assigned
        Material proceduralSky = new Material(Shader.Find("Skybox/Procedural"));
        
        // Level 2 corruption theme
        proceduralSky.SetColor("_SkyTint", new Color(0.3f, 0.1f, 0.05f));      // Dark red tint
        proceduralSky.SetColor("_GroundColor", new Color(0.1f, 0.02f, 0.02f)); // Very dark ground
        proceduralSky.SetFloat("_Exposure", 1.3f);                             // Slightly bright
        proceduralSky.SetFloat("_AtmosphereThickness", 0.6f);                  // Thin atmosphere
        proceduralSky.SetFloat("_SunDisk", 0f);                                // No sun (digital world)
        proceduralSky.SetFloat("_SunSize", 0f);                                // Remove sun completely
        
        level2Skybox = proceduralSky;
        currentSkyboxMaterial = proceduralSky;
        RenderSettings.skybox = proceduralSky;
        
        Debug.Log("Created procedural Level 2 skybox");
    }
    
    void Update()
    {
        if (isInitialized && currentLevel == 2 && enableDynamicCorruption)
        {
            UpdateLevel2CorruptionEffects();
        }
    }
    
    void UpdateLevel2CorruptionEffects()
    {
        if (currentSkyboxMaterial == null) return;
        
        // Get current fragment progress
        float fragmentProgress = 0f;
        if (gameManager != null)
        {
            fragmentProgress = (float)gameManager.GetCollectedFragments() / gameManager.GetTotalFragments();
        }
        
        // Pulsating corruption effect
        float time = Time.time * pulsationSpeed;
        float pulsation = (Mathf.Sin(time) + 1f) * 0.5f; // 0 to 1
        
        // Increase corruption with fragment collection
        float totalCorruption = corruptionIntensity;
        if (corruptionIncreasesWithFragments)
        {
            totalCorruption += fragmentProgress * corruptionPerFragment;
        }
        
        // Apply corruption to skybox
        if (currentSkyboxMaterial.HasProperty("_SkyTint"))
        {
            Color corruptedTint = Color.Lerp(baseSkyTint, baseCorruptionColor, totalCorruption);
            Color pulsatingTint = Color.Lerp(corruptedTint, baseCorruptionColor, pulsation * 0.3f);
            currentSkyboxMaterial.SetColor("_SkyTint", pulsatingTint);
        }
        
        // Atmosphere corruption
        if (currentSkyboxMaterial.HasProperty("_AtmosphereThickness"))
        {
            float corruptedThickness = baseAtmosphereThickness * (1f - totalCorruption * 0.5f);
            currentSkyboxMaterial.SetFloat("_AtmosphereThickness", corruptedThickness);
        }
        
        // Exposure corruption (flickering)
        if (currentSkyboxMaterial.HasProperty("_Exposure"))
        {
            float baseExposure = 1.3f;
            float corruptedExposure = baseExposure + (pulsation * totalCorruption * 0.4f);
            currentSkyboxMaterial.SetFloat("_Exposure", corruptedExposure);
        }
    }
    
    // Public methods for game events
    public void OnFragmentCollected()
    {
        if (currentLevel == 2 && corruptionIncreasesWithFragments)
        {
            Debug.Log("Fragment collected - increasing skybox corruption");
            
            // Optional: Brief flash effect when fragment collected
            StartCoroutine(CorruptionFlash());
        }
    }
    
    System.Collections.IEnumerator CorruptionFlash()
    {
        if (currentSkyboxMaterial == null) yield break;
        
        // Brief intense corruption flash
        Color originalTint = currentSkyboxMaterial.GetColor("_SkyTint");
        
        // Flash to intense corruption
        currentSkyboxMaterial.SetColor("_SkyTint", baseCorruptionColor);
        yield return new WaitForSeconds(0.1f);
        
        // Fade back over 0.5 seconds
        float timer = 0f;
        while (timer < 0.5f)
        {
            timer += Time.deltaTime;
            float progress = timer / 0.5f;
            Color flashColor = Color.Lerp(baseCorruptionColor, originalTint, progress);
            currentSkyboxMaterial.SetColor("_SkyTint", flashColor);
            yield return null;
        }
    }
    
    public void OnAllFragmentsCollected()
    {
        Debug.Log("All fragments collected - maximum skybox corruption");
        
        // Optional: Dramatic final corruption effect
        if (currentLevel == 2)
        {
            StartCoroutine(FinalCorruptionEffect());
        }
    }
    
    System.Collections.IEnumerator FinalCorruptionEffect()
    {
        if (currentSkyboxMaterial == null) yield break;
        
        // Intense pulsing corruption when all fragments collected
        for (int i = 0; i < 5; i++)
        {
            currentSkyboxMaterial.SetColor("_SkyTint", baseCorruptionColor);
            yield return new WaitForSeconds(0.2f);
            currentSkyboxMaterial.SetColor("_SkyTint", baseSkyTint);
            yield return new WaitForSeconds(0.2f);
        }
        
        // Final corrupted state
        currentSkyboxMaterial.SetColor("_SkyTint", baseCorruptionColor);
    }
    
    public void OnGameOver()
    {
        // Optional: Reset corruption on game over
        if (currentSkyboxMaterial != null && currentSkyboxMaterial.HasProperty("_SkyTint"))
        {
            currentSkyboxMaterial.SetColor("_SkyTint", baseSkyTint);
        }
    }
    
    // Manual skybox switching
    public void SwitchToLevel(int level)
    {
        currentLevel = level;
        SetSkyboxForLevel(level);
    }
    
    // Adjust corruption intensity during gameplay
    public void SetCorruptionIntensity(float intensity)
    {
        corruptionIntensity = Mathf.Clamp01(intensity);
    }
}