using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitTrigger : MonoBehaviour
{
    [Header("Level Settings")]
    public int currentLevel = 1;
    public int maxLevels = 4;
    
    [Header("Visual Effects")]
    public GameObject activationEffect;
    public Material inactiveMaterial;
    public Material activeMaterial;
    
    [Header("Audio")]
    public AudioClip activationSound;
    public AudioClip exitSound;
    
    private bool isActivated = false;
    private GameManager gameManager;
    private Renderer platformRenderer;
    private AudioSource audioSource;
    private PortalBeacon beacon;
    
    void Start()
    {
        // Get current level from scene name
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.Contains("Level"))
        {
            string levelNumber = sceneName.Replace("Level", "");
            if (int.TryParse(levelNumber, out int level))
            {
                currentLevel = level;
            }
        }
        
        // Setup components
        platformRenderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();
        beacon = GetComponent<PortalBeacon>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.volume = 0.7f;
        
        // Find GameManager
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
        
        // Ensure it has a trigger collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        
        // Start deactivated
        SetExitState(false);
        
        Debug.Log($"Exit trigger initialized for Level {currentLevel} (Max: {maxLevels})");
    }
    
    public void ActivateExit()
    {
        if (isActivated) return;
        
        isActivated = true;
        Debug.Log($"Exit portal activated for Level {currentLevel}!");
        
        // Change visual state
        SetExitState(true);
        
        // Activate beacon if present
        if (beacon != null)
        {
            beacon.ActivateBeacon();
        }
        
        // Play activation sound
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }
        
        // Spawn activation effect
        if (activationEffect != null)
        {
            GameObject effect = Instantiate(activationEffect, transform.position, transform.rotation);
            Destroy(effect, 3f);
        }
        
        // Start pulsing animation
        StartCoroutine(PulseEffect());
    }
    
    void SetExitState(bool active)
    {
        if (platformRenderer != null)
        {
            if (active)
            {
                // Use level theme color
                Color themeColor = LevelConfigs.GetLevelThemeColor(currentLevel);
                
                if (activeMaterial != null)
                {
                    platformRenderer.material = activeMaterial;
                }
                else
                {
                    // Create glowing material with level theme
                    Material glowMat = new Material(Shader.Find("Standard"));
                    glowMat.color = themeColor;
                    glowMat.EnableKeyword("_EMISSION");
                    glowMat.SetColor("_EmissionColor", themeColor * 0.8f);
                    platformRenderer.material = glowMat;
                }
            }
            else
            {
                if (inactiveMaterial != null)
                {
                    platformRenderer.material = inactiveMaterial;
                }
                else
                {
                    Material inactiveMat = new Material(Shader.Find("Standard"));
                    inactiveMat.color = Color.gray;
                    platformRenderer.material = inactiveMat;
                }
            }
        }
    }
    
    System.Collections.IEnumerator PulseEffect()
    {
        Color themeColor = LevelConfigs.GetLevelThemeColor(currentLevel);
        
        while (isActivated && platformRenderer != null)
        {
            if (platformRenderer.material.HasProperty("_EmissionColor"))
            {
                float intensity = (Mathf.Sin(Time.time * 2f) + 1f) * 0.4f + 0.4f;
                platformRenderer.material.SetColor("_EmissionColor", themeColor * intensity);
            }
            yield return null;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isActivated)
            {
                Debug.Log("Portal not yet activated - collect all fragments first!");
                
                if (gameManager != null)
                {
                    gameManager.PlayerReachedExit();
                }
                return;
            }
            
            Debug.Log($"Player reached the activated portal for Level {currentLevel}!");
            
            // Play exit sound
            if (audioSource != null && exitSound != null)
            {
                audioSource.PlayOneShot(exitSound);
            }
            
            // Handle level completion
            CompleteLevel();
        }
    }
    
void CompleteLevel()
{
    // Setup story data
    PlayerPrefs.SetInt("StoryLevel", currentLevel);
    
    if (currentLevel >= maxLevels)
    {
        PlayerPrefs.SetString("StoryType", "GameComplete");
        PlayerPrefs.SetString("NextScene", "MainMenu");
    }
    else
    {
        PlayerPrefs.SetString("StoryType", "LevelComplete");
        PlayerPrefs.SetString("NextScene", $"Level{currentLevel + 1}");
    }
    
    PlayerPrefs.Save();
    
    // Load story transition scene (index 2)
    SceneManager.LoadScene("StoryTransition");
}
    
    public bool IsActivated()
    {
        return isActivated;
    }
    
    public void OnAllFragmentsCollected()
    {
        ActivateExit();
    }
}