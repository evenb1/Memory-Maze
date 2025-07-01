using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Pause Menu UI")]
    public GameObject pauseMenuPanel;
    public Button resumeButton;
    public Button restartButton;
    public Button quitButton;
    public TextMeshProUGUI pauseTitle;
    
    [Header("Settings")]
    public KeyCode pauseKey = KeyCode.Escape;
    public bool canPause = true;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip buttonClickSound;
    public AudioClip pauseSound;
    public AudioClip resumeSound;
    
    private bool isPaused = false;
    private GameManager gameManager;
    
    void Start()
    {
        // Find the GameManager
        gameManager = FindFirstObjectByType<GameManager>();
        
        // Setup audio source
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.volume = 0.7f;
        
        // Setup the pause menu
        SetupPauseMenu();
        
        // Hide pause menu at start
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        
        Debug.Log("Pause Menu Manager initialized");
    }
    
    void SetupPauseMenu()
    {
        // Setup Resume button
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(() => {
                PlayButtonSound();
                ResumeGame();
            });
        }
        
        // Setup Restart button
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(() => {
                PlayButtonSound();
                RestartLevel();
            });
        }
        
        // Setup Quit button (goes to main menu)
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(() => {
                PlayButtonSound();
                GoToMainMenu();
            });
        }
        
        // Setup title text
        if (pauseTitle != null)
        {
            pauseTitle.text = "GAME PAUSED";
        }
    }
    
    void Update()
    {
        // Check for pause key input
        if (Input.GetKeyDown(pauseKey) && canPause)
        {
            if (!isPaused)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }
    }
    
    public void PauseGame()
    {
        if (!canPause || isPaused) return;
        
        isPaused = true;
        Time.timeScale = 0f;
        
        // Show pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
        
        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Play pause sound
        if (audioSource != null && pauseSound != null)
        {
            audioSource.PlayOneShot(pauseSound);
        }
        
        Debug.Log("Game paused");
        
        // Update status message
        if (gameManager != null)
        {
            gameManager.UpdateStatusMessage("Game Paused - Press ESC to resume");
        }
    }
    
    public void ResumeGame()
    {
        if (!isPaused) return;
            Debug.Log("RESUME BUTTON CLICKED!"); // Add this line

        isPaused = false;
        Time.timeScale = 1f;
        
        // Hide pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        
        // Lock cursor back
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Play resume sound
        if (audioSource != null && resumeSound != null)
        {
            audioSource.PlayOneShot(resumeSound);
        }
        
        Debug.Log("Game resumed");
        
        // Update status message
        if (gameManager != null)
        {
            gameManager.UpdateStatusMessage("Game resumed!");
        }
    }
    
    public void RestartLevel()
    {
        Debug.Log("Restarting level...");
        
        // Reset time scale
        Time.timeScale = 1f;
        
        // Reload current scene
        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }
    
    public void GoToMainMenu()
    {
        Debug.Log("Returning to main menu...");
        
        // Reset time scale
        Time.timeScale = 1f;
        
        // Load main menu scene
        SceneManager.LoadScene("MainMenu");
    }
    
    void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
    
    // Public getters for other scripts
    public bool IsPaused()
    {
        return isPaused;
    }
    
    public void DisablePausing()
    {
        canPause = false;
    }
    
    public void EnablePausing()
    {
        canPause = true;
    }
}