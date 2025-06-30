using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitTrigger : MonoBehaviour
{
    [Header("Level Settings")]
    public int currentLevel = 1;
    public int maxLevels = 4;
    
    [Header("Extraction Settings")]
    public float extractionEffectTime = 2f;
    public float fadeOutTime = 1f;
    public float centerRadius = 1f;
    
    [Header("Audio")]
    public AudioClip activationSound;
    public AudioClip teleportSound;
    
    private bool isActivated = false;
    private bool hasTriggered = false;
    private Transform playerInPortal = null;
    private GameManager gameManager;
    private Renderer platformRenderer;
    private AudioSource audioSource;
    private PortalBeacon beacon;
    
    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.Contains("Level"))
        {
            string levelNumber = sceneName.Replace("Level", "");
            if (int.TryParse(levelNumber, out int level))
            {
                currentLevel = level;
            }
        }
        
        platformRenderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();
        beacon = GetComponent<PortalBeacon>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.volume = 0.7f;
        
        gameManager = FindFirstObjectByType<GameManager>();
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        
        SetExitState(false);
        
        Debug.Log($"Exit trigger initialized for Level {currentLevel}");
    }
    
    void Update()
    {
        if (playerInPortal != null && !hasTriggered && isActivated)
        {
            float distanceFromCenter = Vector3.Distance(
                new Vector3(playerInPortal.position.x, transform.position.y, playerInPortal.position.z),
                transform.position
            );
            
            if (distanceFromCenter <= centerRadius)
            {
                Debug.Log("Player reached portal center! Starting extraction...");
                hasTriggered = true;
                StartExtractionSequence(playerInPortal);
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            Debug.Log($"=== PLAYER TOUCHED PORTAL ===");
            Debug.Log($"isActivated: {isActivated}");
            
            if (!isActivated)
            {
                Debug.Log("Portal not yet activated - collect all fragments first!");
                if (gameManager != null)
                {
                    gameManager.UpdateStatusMessage("Collect all memory fragments before escaping!");
                }
                return;
            }
            
            playerInPortal = other.transform;
            Debug.Log("Player entered portal area - walk to center to activate extraction...");
            
            if (gameManager != null)
            {
                gameManager.UpdateStatusMessage("Move to the center of the portal to begin extraction!");
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInPortal = null;
            Debug.Log("Player left portal area");
            
            if (gameManager != null && isActivated && !hasTriggered)
            {
                gameManager.UpdateStatusMessage("ALL FRAGMENTS COLLECTED! Proceed to the glowing EXIT PORTAL!");
            }
        }
    }
    
    void StartExtractionSequence(Transform player)
    {
        Debug.Log("Player reached portal center! Starting extraction...");
        
        // IMMEDIATELY stop the game to prevent virus from catching player
        Time.timeScale = 0f;
        
        // Disable player movement
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // LOCK THE CAMERA
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.transform.SetParent(null);
            Vector3 lockedPosition = mainCamera.transform.position;
            Quaternion lockedRotation = mainCamera.transform.rotation;
            
            StartCoroutine(LockCameraRealTime(mainCamera, lockedPosition, lockedRotation));
        }
        
        // Show cursor since game is paused
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Play teleport sound
        if (audioSource != null && teleportSound != null)
        {
            audioSource.PlayOneShot(teleportSound);
        }
        
        // Show extraction message
        if (gameManager != null)
        {
            gameManager.UpdateStatusMessage("EXTRACTION COMPLETE! Well done!");
        }
        
        // Start effects and completion
        StartCoroutine(ExtractionEffectsAndComplete(player));
    }
    
    System.Collections.IEnumerator LockCameraRealTime(Camera camera, Vector3 position, Quaternion rotation)
    {
        while (camera != null)
        {
            camera.transform.position = position;
            camera.transform.rotation = rotation;
            yield return new WaitForSecondsRealtime(0.02f);
        }
    }
    
    System.Collections.IEnumerator ExtractionEffectsAndComplete(Transform player)
    {
        // Create extraction effect
        GameObject extractionEffect = CreateExtractionEffect(player);
        
        // Wait using real time
        yield return new WaitForSecondsRealtime(extractionEffectTime);
        
        // Start fade out
        StartCoroutine(FadeOut());
        
        // Wait for fade to complete
        yield return new WaitForSecondsRealtime(fadeOutTime);
        
        // Clean up effect
        if (extractionEffect != null)
        {
            Destroy(extractionEffect);
        }
        
        // Restore time scale before scene transition
        Time.timeScale = 1f;
        
        // Complete the level
        CompleteLevel();
    }
    
    GameObject CreateExtractionEffect(Transform player)
    {
        GameObject effect = new GameObject("ExtractionEffect");
        effect.transform.position = player.position;
        
        // Create upward particle beam with cyan color
        ParticleSystem particles = effect.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startColor = Color.cyan;
        main.startSpeed = 10f;
        main.startSize = 0.2f;
        main.maxParticles = 150;
        main.startLifetime = 3f;
        
        var emission = particles.emission;
        emission.rateOverTime = 75f;
        
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.8f;
        
        var velocityOverLifetime = particles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.y = 20f;
        
        // Add bright cyan light
        Light extractionLight = effect.AddComponent<Light>();
        extractionLight.type = LightType.Point;
        extractionLight.color = Color.cyan;
        extractionLight.intensity = 12f;
        extractionLight.range = 10f;
        
        // Make player glow cyan
        StartCoroutine(MakePlayerGlow(player));
        
        return effect;
    }
    
    System.Collections.IEnumerator MakePlayerGlow(Transform player)
    {
        Renderer playerRenderer = player.GetComponentInChildren<Renderer>();
        if (playerRenderer != null)
        {
            Material originalMaterial = playerRenderer.material;
            
            // Create cyan glowing material
            Material glowMaterial = new Material(originalMaterial);
            glowMaterial.EnableKeyword("_EMISSION");
            glowMaterial.SetColor("_EmissionColor", Color.cyan * 1.5f);
            
            playerRenderer.material = glowMaterial;
            
            yield return new WaitForSecondsRealtime(extractionEffectTime);
            
            // Restore original material
            playerRenderer.material = originalMaterial;
        }
    }
    
    System.Collections.IEnumerator FadeOut()
    {
        // Create a black overlay to fade to black
        GameObject fadePanel = new GameObject("FadePanel");
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            fadePanel.transform.SetParent(canvas.transform);
        }
        
        UnityEngine.UI.Image fadeImage = fadePanel.AddComponent<UnityEngine.UI.Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        
        // Set to full screen
        RectTransform rect = fadePanel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // Fade to black
        float timer = 0f;
        while (timer < fadeOutTime)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = timer / fadeOutTime;
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        fadeImage.color = Color.black;
    }
    
    void CompleteLevel()
    {
        // Setup story transition
        PlayerPrefs.SetInt("CompletedLevel", currentLevel);
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
        
        Debug.Log($"Level {currentLevel} completed! Loading story transition...");
        
        // Notify GameManager of completion
        if (gameManager != null)
        {
            gameManager.PlayerReachedExit();
        }
        
        // Load story transition
        SceneManager.LoadScene("StoryTransition");
    }
    
    public void ActivateExit()
    {
        if (isActivated) return;
        
        isActivated = true;
        Debug.Log($"Exit portal activated for Level {currentLevel}!");
        
        SetExitState(true);
        
        if (beacon != null)
        {
            beacon.ActivateBeacon();
        }
        
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }
        
        StartCoroutine(PulseEffect());
    }
    
    void SetExitState(bool active)
    {
        if (platformRenderer != null)
        {
            if (active)
            {
                Material glowMat = new Material(Shader.Find("Standard"));
                glowMat.color = Color.cyan;
                glowMat.EnableKeyword("_EMISSION");
                glowMat.SetColor("_EmissionColor", Color.cyan * 0.8f);
                platformRenderer.material = glowMat;
            }
            else
            {
                Material inactiveMat = new Material(Shader.Find("Standard"));
                inactiveMat.color = Color.gray;
                platformRenderer.material = inactiveMat;
            }
        }
    }
    
    System.Collections.IEnumerator PulseEffect()
    {
        while (isActivated && platformRenderer != null)
        {
            if (platformRenderer.material.HasProperty("_EmissionColor"))
            {
                float intensity = (Mathf.Sin(Time.time * 2f) + 1f) * 0.4f + 0.4f;
                platformRenderer.material.SetColor("_EmissionColor", Color.cyan * intensity);
            }
            yield return null;
        }
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