using UnityEngine;
using UnityEngine.Rendering;

public class LevelSkyboxManager : MonoBehaviour
{
    [Header("Imported Skybox Materials")]
    public Material skyboxSciFi2;  // Skybox_SBS Sci-Fi & Fantasy 2 Large
    public Material skyboxSciFi3;  // Skybox_SBS Sci-Fi & Fantasy 3 Large  
    public Material skyboxSciFi4;  // Skybox_SBS Sci-Fi & Fantasy 4 Large (Level 3 - bright)
    public Material skyboxSciFi5;  // Skybox_SBS Sci-Fi & Fantasy 5 Large (Level 2 - corruption)
    
    [Header("Level 2 Corruption Effects")]
    public bool enableLevel2Corruption = true;
    public Color level2CorruptionTint = new Color(1f, 0.3f, 0.2f, 0.3f); // Red/orange tint
    public float corruptionIntensity = 0.5f;
    public bool corruptionIncreasesWithFragments = true;
    
    [Header("Level 3 Brightness Effects")]
    public bool enableLevel3Brightness = true;
    public Color level3BrightnessTint = new Color(1.2f, 1.1f, 0.9f, 0.2f); // Bright golden tint
    public float brightnessIntensity = 0.3f;
    
    [Header("Dynamic Effects")]
    public bool enableDynamicEffects = true;
    public float effectSpeed = 1f;
    
    private Material currentSkyboxMaterial;
    private Material runtimeSkyboxMaterial; // Copy for runtime modifications
    private GameManager gameManager;
    private int currentLevel = 1;
    private float fragmentProgress = 0f;
    
    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        DetectCurrentLevel();
        SetupSkyboxForLevel();
        
        Debug.Log($"Skybox Manager initialized for Level {currentLevel}");
    }
    
    void DetectCurrentLevel()
    {
        // Detect current level from scene name
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        if (sceneName.Contains("Level"))
        {
            string levelNumber = sceneName.Replace("Level", "");
            if (int.TryParse(levelNumber, out int level))
            {
                currentLevel = level;
            }
        }
        
        Debug.Log($"Detected Level: {currentLevel}");
    }
    
    void SetupSkyboxForLevel()
    {
        Material targetSkybox = null;
        
        switch (currentLevel)
        {
            case 1:
                targetSkybox = skyboxSciFi2; // Clean/normal sci-fi
                break;
                
            case 2:
                targetSkybox = skyboxSciFi5; // Corruption theme
                break;
                
            case 3:
                targetSkybox = skyboxSciFi4; // Bright theme
                break;
                
            case 4:
                targetSkybox = skyboxSciFi3; // Final level
                break;
                
            default:
                targetSkybox = skyboxSciFi2; // Fallback
                break;
        }
        
        if (targetSkybox != null)
        {
            // Create runtime copy for modifications
            runtimeSkyboxMaterial = new Material(targetSkybox);
            runtimeSkyboxMaterial.name = $"Runtime_Skybox_Level{currentLevel}";
            
            // Apply to scene
            RenderSettings.skybox = runtimeSkyboxMaterial;
            currentSkyboxMaterial = runtimeSkyboxMaterial;
            
            // Apply level-specific modifications
            ApplyLevelSpecificEffects();
            
            // Update lighting
            DynamicGI.UpdateEnvironment();
            
            Debug.Log($"Applied skybox for Level {currentLevel}: {targetSkybox.name}");
        }
        else
        {
            Debug.LogWarning($"No skybox assigned for Level {currentLevel}!");
        }
    }
    
    void ApplyLevelSpecificEffects()
    {
        switch (currentLevel)
        {
            case 2:
                ApplyLevel2CorruptionEffect();
                break;
                
            case 3:
                ApplyLevel3BrightnessEffect();
                break;
        }
    }
    
    void ApplyLevel2CorruptionEffect()
    {
        if (!enableLevel2Corruption || currentSkyboxMaterial == null) return;
        
        // Apply red/orange corruption tint
        if (currentSkyboxMaterial.HasProperty("_Tint"))
        {
            Color originalTint = currentSkyboxMaterial.GetColor("_Tint");
            Color corruptedTint = Color.Lerp(originalTint, level2CorruptionTint, corruptionIntensity);
            currentSkyboxMaterial.SetColor("_Tint", corruptedTint);
        }
        
        // Adjust exposure for corruption
        if (currentSkyboxMaterial.HasProperty("_Exposure"))
        {
            float originalExposure = currentSkyboxMaterial.GetFloat("_Exposure");
            float corruptedExposure = originalExposure * (1f + corruptionIntensity * 0.3f);
            currentSkyboxMaterial.SetFloat("_Exposure", corruptedExposure);
        }
        
        Debug.Log("Applied Level 2 corruption effects to skybox");
    }
    
    void ApplyLevel3BrightnessEffect()
    {
        if (!enableLevel3Brightness || currentSkyboxMaterial == null) return;
        
        // Apply bright golden tint
        if (currentSkyboxMaterial.HasProperty("_Tint"))
        {
            Color originalTint = currentSkyboxMaterial.GetColor("_Tint");
            Color brightTint = Color.Lerp(originalTint, level3BrightnessTint, brightnessIntensity);
            currentSkyboxMaterial.SetColor("_Tint", brightTint);
        }
        
        // Increase exposure for brightness
        if (currentSkyboxMaterial.HasProperty("_Exposure"))
        {
            float originalExposure = currentSkyboxMaterial.GetFloat("_Exposure");
            float brightExposure = originalExposure * (1f + brightnessIntensity * 0.5f);
            currentSkyboxMaterial.SetFloat("_Exposure", brightExposure);
        }
        
        Debug.Log("Applied Level 3 brightness effects to skybox");
    }
    
    void Update()
    {
        if (enableDynamicEffects && currentSkyboxMaterial != null)
        {
            UpdateDynamicEffects();
        }
        
        UpdateFragmentProgress();
    }
    
    void UpdateFragmentProgress()
    {
        if (gameManager != null)
        {
            float newProgress = (float)gameManager.GetCollectedFragments() / gameManager.GetTotalFragments();
            if (newProgress != fragmentProgress)
            {
                fragmentProgress = newProgress;
                OnFragmentProgressChanged();
            }
        }
    }
    
    void UpdateDynamicEffects()
    {
        float time = Time.time * effectSpeed;
        
        switch (currentLevel)
        {
            case 2:
                UpdateLevel2DynamicCorruption(time);
                break;
                
            case 3:
                UpdateLevel3DynamicBrightness(time);
                break;
        }
    }
    
    void UpdateLevel2DynamicCorruption(float time)
    {
        if (!enableLevel2Corruption) return;
        
        // Pulsating corruption effect
        float corruption = corruptionIntensity + (fragmentProgress * 0.3f);
        float pulsation = (Mathf.Sin(time * 0.5f) + 1f) * 0.5f; // 0 to 1
        float dynamicCorruption = corruption + (pulsation * 0.2f);
        
        // Apply dynamic tint
        if (currentSkyboxMaterial.HasProperty("_Tint"))
        {
            Color baseTint = Color.white;
            Color corruptedTint = Color.Lerp(baseTint, level2CorruptionTint, dynamicCorruption);
            currentSkyboxMaterial.SetColor("_Tint", corruptedTint);
        }
        
        // Dynamic exposure flickering
        if (currentSkyboxMaterial.HasProperty("_Exposure"))
        {
            float baseExposure = 1f;
            float flicker = Mathf.Sin(time * 2f) * 0.1f * fragmentProgress;
            float dynamicExposure = baseExposure + (dynamicCorruption * 0.3f) + flicker;
            currentSkyboxMaterial.SetFloat("_Exposure", dynamicExposure);
        }
    }
    
    void UpdateLevel3DynamicBrightness(float time)
    {
        if (!enableLevel3Brightness) return;
        
        // Gentle brightness pulsation
        float brightness = brightnessIntensity;
        float pulsation = (Mathf.Sin(time * 0.3f) + 1f) * 0.5f;
        float dynamicBrightness = brightness + (pulsation * 0.1f);
        
        // Apply dynamic bright tint
        if (currentSkyboxMaterial.HasProperty("_Tint"))
        {
            Color baseTint = Color.white;
            Color brightTint = Color.Lerp(baseTint, level3BrightnessTint, dynamicBrightness);
            currentSkyboxMaterial.SetColor("_Tint", brightTint);
        }
    }
    
    void OnFragmentProgressChanged()
    {
        Debug.Log($"Fragment progress: {fragmentProgress:F2} - updating skybox effects");
        
        if (currentLevel == 2 && corruptionIncreasesWithFragments)
        {
            // Increase corruption with each fragment
            StartCoroutine(CorruptionPulse());
        }
    }
    
    System.Collections.IEnumerator CorruptionPulse()
    {
        if (currentSkyboxMaterial == null) yield break;
        
        // Brief intense corruption pulse when fragment collected
        float originalIntensity = corruptionIntensity;
        float pulseIntensity = originalIntensity + 0.4f;
        
        // Store original values
        Color originalTint = currentSkyboxMaterial.HasProperty("_Tint") ? 
            currentSkyboxMaterial.GetColor("_Tint") : Color.white;
        
        // Pulse to intense corruption
        if (currentSkyboxMaterial.HasProperty("_Tint"))
        {
            Color pulseTint = Color.Lerp(Color.white, level2CorruptionTint, pulseIntensity);
            currentSkyboxMaterial.SetColor("_Tint", pulseTint);
        }
        
        yield return new WaitForSeconds(0.2f);
        
        // Fade back over 1 second
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime;
            float progress = timer / 1f;
            
            if (currentSkyboxMaterial.HasProperty("_Tint"))
            {
                Color currentTint = Color.Lerp(level2CorruptionTint, originalTint, progress);
                currentSkyboxMaterial.SetColor("_Tint", currentTint);
            }
            
            yield return null;
        }
    }
    
    // Public methods for game events
    public void OnFragmentCollected()
    {
        Debug.Log("Fragment collected - skybox responding");
    }
    
    public void OnAllFragmentsCollected()
    {
        Debug.Log("All fragments collected - maximum skybox effect");
        
        if (currentLevel == 2)
        {
            StartCoroutine(FinalCorruptionEffect());
        }
    }
    
    System.Collections.IEnumerator FinalCorruptionEffect()
    {
        // Dramatic final corruption when all fragments collected
        for (int i = 0; i < 3; i++)
        {
            if (currentSkyboxMaterial.HasProperty("_Tint"))
            {
                currentSkyboxMaterial.SetColor("_Tint", level2CorruptionTint);
            }
            yield return new WaitForSeconds(0.3f);
            
            if (currentSkyboxMaterial.HasProperty("_Tint"))
            {
                currentSkyboxMaterial.SetColor("_Tint", Color.white);
            }
            yield return new WaitForSeconds(0.3f);
        }
        
        // Final corrupted state
        if (currentSkyboxMaterial.HasProperty("_Tint"))
        {
            currentSkyboxMaterial.SetColor("_Tint", level2CorruptionTint);
        }
    }
    
    public void OnGameOver()
    {
        // Reset skybox effects on game over
        if (runtimeSkyboxMaterial != null)
        {
            if (runtimeSkyboxMaterial.HasProperty("_Tint"))
            {
                runtimeSkyboxMaterial.SetColor("_Tint", Color.white);
            }
        }
    }
    
    // Manual skybox switching for testing
    [System.Obsolete("Use for testing only")]
    public void SwitchToSkybox(int skyboxIndex)
    {
        Material[] skyboxes = { skyboxSciFi2, skyboxSciFi3, skyboxSciFi4, skyboxSciFi5 };
        
        if (skyboxIndex >= 0 && skyboxIndex < skyboxes.Length && skyboxes[skyboxIndex] != null)
        {
            RenderSettings.skybox = skyboxes[skyboxIndex];
            DynamicGI.UpdateEnvironment();
            Debug.Log($"Manually switched to skybox: {skyboxes[skyboxIndex].name}");
        }
    }
}