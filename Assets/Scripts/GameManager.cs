using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int totalFragments = 5;
    public int collectedFragments = 0;
    
    [Header("UI Elements")]
    public Text fragmentCounterText;
    public Text winText;
    public Text statusText; // For showing messages to player
    
    [Header("Exit Platform")]
    public GameObject exitPlatform; // The exit portal object
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip fragmentCollectedSound;
    public AudioClip allFragmentsSound;
    public AudioClip winSound;
    public AudioClip gameOverSound;
    
    private bool allFragmentsCollected = false;
    private bool gameEnded = false;
    private PortalBeacon portalBeacon; // Reference to the beacon
    
    // Singleton pattern for easy access
    public static GameManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton setup
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
        // Initialize audio if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Reset game state
        collectedFragments = 0;
        allFragmentsCollected = false;
        gameEnded = false;
        
        // Find the portal beacon
        portalBeacon = FindObjectOfType<PortalBeacon>();
        
        // Initialize UI
        UpdateUI();
        UpdateStatusMessage("Collect all memory fragments to activate the exit portal!");
        
        // Make sure exit portal starts inactive
        if(exitPlatform != null)
        {
            SetExitPlatformState(false);
        }
        
        // Enable cursor lock for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log($"GameManager initialized - Need to collect {totalFragments} fragments");
        Debug.Log($"Portal beacon found: {(portalBeacon != null ? "YES" : "NO")}");
    }
    
public void CollectFragment()
{
    if (gameEnded) return;
    
    collectedFragments++;
    Debug.Log($"Fragment collected! Progress: {collectedFragments}/{totalFragments}");
    Debug.Log($"UPDATING UI - Fragment counter should show: Memory Fragments: {collectedFragments}/{totalFragments}"); // ADD THIS LINE
    
    // Play collection sound
    if (audioSource != null && fragmentCollectedSound != null)
    {
        audioSource.PlayOneShot(fragmentCollectedSound);
    }
    
    // Update UI
    UpdateUI();
        
        // Check if all fragments collected
        if(collectedFragments >= totalFragments)
        {
            AllFragmentsCollected();
        }
        else
        {
            // Show progress message
            int remaining = totalFragments - collectedFragments;
            UpdateStatusMessage($"Fragment acquired! {remaining} fragments remaining...");
            
            // Make virus faster (if you want progressive difficulty)
            VirusAI virus = FindObjectOfType<VirusAI>();
            if (virus != null)
            {
                virus.moveSpeed += 0.3f; // Increase virus speed slightly
                Debug.Log($"Virus speed increased to {virus.moveSpeed}");
            }
        }
    }
    
    void AllFragmentsCollected()
    {
        allFragmentsCollected = true;
        Debug.Log("All fragments collected! Exit portal activated!");
        
        // Play special sound
        if (audioSource != null && allFragmentsSound != null)
        {
            audioSource.PlayOneShot(allFragmentsSound);
        }
        
        // ACTIVATE THE PORTAL BEACON!
        if (portalBeacon != null)
        {
            portalBeacon.OnAllFragmentsCollected();
        }
        else
        {
            Debug.LogWarning("Portal beacon not found! Searching again...");
            portalBeacon = FindObjectOfType<PortalBeacon>();
            if (portalBeacon != null)
            {
                portalBeacon.OnAllFragmentsCollected();
            }
        }
        
        // Activate exit platform
        if(exitPlatform != null)
        {
            SetExitPlatformState(true);
        }
        
        // Update UI messages
        UpdateStatusMessage("ALL FRAGMENTS COLLECTED! Proceed to the glowing EXIT PORTAL!");
        
        if(fragmentCounterText != null)
        {
            fragmentCounterText.text = "All fragments collected! Find the glowing portal!";
            fragmentCounterText.color = Color.green; // Change to green
        }
        
        // Flash the status message
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
        
        statusText.color = Color.green; // Leave it green
    }
    
    void SetExitPlatformState(bool active)
    {
        if (exitPlatform != null)
        {
            // Don't use SetActive anymore since portal is always visible
            // Just make it glow when activated
            if (active)
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
    
    void UpdateUI()
    {
        if(fragmentCounterText != null)
        {
            fragmentCounterText.text = $"Memory Fragments: {collectedFragments}/{totalFragments}";
            
            // Change color based on progress
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
    }
    
    void UpdateStatusMessage(string message)
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
        Debug.Log("YOU WIN! Bit-27 has escaped the corrupted memory!");
        
        // Play win sound
        if (audioSource != null && winSound != null)
        {
            audioSource.PlayOneShot(winSound);
        }
        
        // Show win message
        if(winText != null)
        {
            winText.text = "SUCCESS! Bit-27 has escaped the corrupted memory system!";
            winText.gameObject.SetActive(true);
        }
        
        UpdateStatusMessage("ESCAPE SUCCESSFUL! Data recovery complete!");
        
        // Pause game
        Time.timeScale = 0;
        
        // Show cursor for any UI interactions
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Auto restart after 5 seconds (optional)
        StartCoroutine(AutoRestart());
    }
    
    System.Collections.IEnumerator AutoRestart()
    {
        yield return new WaitForSecondsRealtime(5f); // Wait 5 seconds in real time
        RestartLevel();
    }
    
    public void GameOver()
    {
        if (gameEnded) return;
        
        gameEnded = true;
        Debug.Log("GAME OVER! Bit-27 was caught by the virus!");
        
        // Play game over sound
        if (audioSource != null && gameOverSound != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }
        
        if(winText != null)
        {
            winText.text = "GAME OVER! Virus caught Bit-27!";
            winText.color = Color.red;
            winText.gameObject.SetActive(true);
        }
        
        UpdateStatusMessage("SYSTEM COMPROMISED! Virus corrupted the data!");
        
        // Pause game
        Time.timeScale = 0;
        
        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Auto restart after 3 seconds
        StartCoroutine(AutoRestartGameOver());
    }
    
    System.Collections.IEnumerator AutoRestartGameOver()
    {
        yield return new WaitForSecondsRealtime(3f); // Wait 3 seconds
        RestartLevel();
    }
    
    // Public methods for UI buttons or other scripts
    public void RestartLevel()
    {
        Time.timeScale = 1f; // Resume time
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Resume time
        SceneManager.LoadScene("MainMenu");
    }
    
    // Getters for other scripts
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
}
