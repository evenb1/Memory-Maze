using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int totalFragments = 7; // Level 3 has 7 fragments
    public int collectedFragments = 0;
    
    [Header("UI Elements")]
    public TextMeshProUGUI fragmentCounterText;
    public TextMeshProUGUI winText;
    public TextMeshProUGUI statusText;  
    
    [Header("Exit Platform")]
    public GameObject exitPlatform;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip fragmentCollectedSound;
    public AudioClip allFragmentsSound;
    public AudioClip winSound;
    public AudioClip gameOverSound;
    
    [Header("Level 3 Virus Integration")]
    public Level3VirusSpawner virusSpawner; // Assign in inspector or auto-find
    
    private bool allFragmentsCollected = false;
    private bool gameEnded = false;
    private PortalBeacon portalBeacon;
    
    public static GameManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Auto-find virus spawner if not assigned
        if (virusSpawner == null)
        {
            virusSpawner = FindFirstObjectByType<Level3VirusSpawner>();
        }
        
        collectedFragments = 0;
        allFragmentsCollected = false;
        gameEnded = false;
        
        portalBeacon = FindFirstObjectByType<PortalBeacon>();
        
        UpdateUI();
        UpdateStatusMessage("Collect all memory fragments to activate the exit portal!");
        
        if(exitPlatform != null)
        {
            SetExitPlatformState(false);
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log($"GameManager initialized - Need to collect {totalFragments} fragments");
        Debug.Log($"Fragment Counter Text assigned: {(fragmentCounterText != null)}");
        Debug.Log($"Level 3 Virus Spawner found: {(virusSpawner != null)}");
    }
    
    public void CollectFragment()
    {
        if (gameEnded) return;
        
        collectedFragments++;
        Debug.Log($"=== FRAGMENT COLLECTED ===");
        Debug.Log($"Progress: {collectedFragments}/{totalFragments}");
        Debug.Log($"fragmentCounterText is: {(fragmentCounterText != null ? "ASSIGNED" : "NULL")}");
        
        if (audioSource != null && fragmentCollectedSound != null)
        {
            audioSource.PlayOneShot(fragmentCollectedSound);
        }
        
        UpdateUI();
        
        if(collectedFragments >= totalFragments)
        {
            AllFragmentsCollected();
        }
        else
        {
            int remaining = totalFragments - collectedFragments;
            UpdateStatusMessage($"Fragment acquired! {remaining} fragments remaining...");
            
            // Notify Level 3 virus spawner about fragment collection
            if (virusSpawner != null)
            {
                virusSpawner.OnFragmentCollected();
            }
            
            // Also increase individual virus speeds (legacy support)
            VirusAI virus = FindFirstObjectByType<VirusAI>();
            if (virus != null)
            {
                virus.IncreaseSpeed(0.3f);
            }
        }
        
        // Notify skybox manager
        LevelSkyboxManager skyboxManager = FindFirstObjectByType<LevelSkyboxManager>();
        if (skyboxManager != null)
        {
            skyboxManager.OnFragmentCollected();
        }
        
        // Notify power-up spawner
        PowerUpSpawner powerUpSpawner = FindFirstObjectByType<PowerUpSpawner>();
        if (powerUpSpawner != null)
        {
            powerUpSpawner.OnFragmentCollected();
        }
    }
    
    void UpdateUI()
    {
        Debug.Log("=== UPDATE UI CALLED ===");
        if(fragmentCounterText != null)
        {
            string newText = $"Memory Fragments: {collectedFragments}/{totalFragments}";
            fragmentCounterText.text = newText;
            Debug.Log($"UI text should now be: {newText}");
            Debug.Log($"Actual UI text is: {fragmentCounterText.text}");
            
            if (collectedFragments == totalFragments)
            {
                fragmentCounterText.color = Color.green;
            }
            else if (collectedFragments > totalFragments / 2)
            {
                fragmentCounterText.color = Color.yellow;
            }
            else
            {
                fragmentCounterText.color = Color.white;
            }
        }
        else
        {
            Debug.LogError("fragmentCounterText is NULL! UI not assigned in GameManager!");
        }
    }
    
    void AllFragmentsCollected()
    {
        allFragmentsCollected = true;
        Debug.Log("All fragments collected! Exit portal activated!");
        
        if (audioSource != null && allFragmentsSound != null)
        {
            audioSource.PlayOneShot(allFragmentsSound);
        }
        
        // Notify Level 3 virus spawner about all fragments collected
        if (virusSpawner != null)
        {
            virusSpawner.OnAllFragmentsCollected();
        }
        
        // ACTIVATE THE PORTAL BEACON
        if (portalBeacon != null)
        {
            portalBeacon.OnAllFragmentsCollected();
            Debug.Log("Beacon activated!");
        }
        
        // ALSO ACTIVATE THE EXIT TRIGGER
        ExitTrigger exitTrigger = FindFirstObjectByType<ExitTrigger>();
        if (exitTrigger != null)
        {
            exitTrigger.OnAllFragmentsCollected();
            Debug.Log("Exit trigger activated!");
        }
        else
        {
            Debug.LogError("ExitTrigger not found!");
        }
        
        // Activate exit platform
        if(exitPlatform != null)
        {
            SetExitPlatformState(true);
        }
        
        // Notify skybox manager
        LevelSkyboxManager skyboxManager = FindFirstObjectByType<LevelSkyboxManager>();
        if (skyboxManager != null)
        {
            skyboxManager.OnAllFragmentsCollected();
        }
        
        // Notify power-up spawner
        PowerUpSpawner powerUpSpawner = FindFirstObjectByType<PowerUpSpawner>();
        if (powerUpSpawner != null)
        {
            powerUpSpawner.OnLevelComplete();
        }
        
        UpdateStatusMessage("ALL FRAGMENTS COLLECTED! Proceed to the glowing EXIT PORTAL!");
        
        if(fragmentCounterText != null)
        {
            fragmentCounterText.text = "All fragments collected! Find the glowing portal!";
            fragmentCounterText.color = Color.green;
        }
        
        StartCoroutine(FlashStatusMessage());
    }
    
    System.Collections.IEnumerator FlashStatusMessage()
    {
        if (statusText == null) yield break;
        
        Color originalColor = statusText.color;
        
        for (int i = 0; i < 6; i++)
        {
            statusText.color = Color.green;
            yield return new WaitForSeconds(0.4f);
            statusText.color = originalColor;
            yield return new WaitForSeconds(0.4f);
        }
        
        statusText.color = Color.green;
    }
    
    void SetExitPlatformState(bool active)
    {
        if (exitPlatform != null && active)
        {
            Renderer renderer = exitPlatform.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = renderer.material;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.green * 0.5f);
            }
            
            Debug.Log("Exit portal activated and glowing!");
        }
    }
    
    public void PlayerReachedExit()
    {
        if (gameEnded) return;
        
        if(allFragmentsCollected)
        {
            WinGame();
        }
        else
        {
            Debug.Log("Cannot exit yet - collect all memory fragments first!");
            UpdateStatusMessage("Collect all memory fragments before escaping!");
        }
    }
    
    public void UpdateStatusMessage(string message)
    {
        if(statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log("Status: " + message);
    }
    
    void WinGame()
    {
        if (gameEnded) return;
        
        gameEnded = true;
        Debug.Log("YOU WIN! Player has escaped!");
        
        if (audioSource != null && winSound != null)
        {
            audioSource.PlayOneShot(winSound);
        }
        
        if(winText != null)
        {
            winText.text = "SUCCESS! You have escaped!";
            winText.gameObject.SetActive(true);
        }
        
        UpdateStatusMessage("ESCAPE SUCCESSFUL! Game complete!");
        
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        StartCoroutine(AutoRestart());
    }
    
    System.Collections.IEnumerator AutoRestart()
    {
        yield return new WaitForSecondsRealtime(5f);
        RestartLevel();
    }
    
    public void GameOver()
    {
        if (gameEnded) return;
        
        gameEnded = true;
        Debug.Log("GAME OVER! Player was caught!");
        
        if (audioSource != null && gameOverSound != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }
        
        // Notify Level 3 virus spawner about game over
        if (virusSpawner != null)
        {
            virusSpawner.OnGameOver();
        }
        
        // Notify other systems
        LevelSkyboxManager skyboxManager = FindFirstObjectByType<LevelSkyboxManager>();
        if (skyboxManager != null)
        {
            skyboxManager.OnGameOver();
        }
        
        PowerUpSpawner powerUpSpawner = FindFirstObjectByType<PowerUpSpawner>();
        if (powerUpSpawner != null)
        {
            powerUpSpawner.OnPlayerCaught();
        }
        
        if(winText != null)
        {
            winText.text = "GAME OVER! You were caught!";
            winText.color = Color.red;
            winText.gameObject.SetActive(true);
        }
        
        UpdateStatusMessage("SYSTEM COMPROMISED! Try again!");
        
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        StartCoroutine(AutoRestartGameOver());
    }
    
    System.Collections.IEnumerator AutoRestartGameOver()
    {
        yield return new WaitForSecondsRealtime(3f);
        RestartLevel();
    }
    
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    
    public bool AreAllFragmentsCollected()
    {
        return allFragmentsCollected;
    }
    
    public int GetCollectedFragments()
    {
        return collectedFragments;
    }
    
    public int GetTotalFragments()
    {
        return totalFragments;
    }
    
    // Additional methods for Level 3 debugging
    public void DebugVirusInfo()
    {
        if (virusSpawner != null)
        {
            int virusCount = virusSpawner.GetActiveVirusCount();
            Vector3[] virusPositions = virusSpawner.GetVirusPositions();
            
            Debug.Log($"Active viruses: {virusCount}");
            for (int i = 0; i < virusPositions.Length; i++)
            {
                Debug.Log($"Virus {i + 1} position: {virusPositions[i]}");
            }
        }
    }
}