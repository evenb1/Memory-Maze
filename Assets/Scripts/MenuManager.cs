using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI subtitleText;
    public Button playButton;
    public Button quitButton;
    
    [Header("Audio Files")]
    public AudioClip clickSound;
    public AudioClip glitchSound;
    public AudioClip typewriterSound;
    
    private string fullSubtitleText = "> SYSTEM.MEMORY.CORRUPTED\n> BIT-27.STATUS: FRAGMENTED\n> RECOVERY.PROTOCOL: ACTIVE\n> AWAITING.USER.INPUT...";
    private AudioSource typewriterAudio;
    private AudioSource effectsAudio;
    
    void Start()
    {
        // Debug info about scenes
        Debug.Log("=== MENU MANAGER DEBUG INFO ===");
        Debug.Log("Total scenes in build: " + SceneManager.sceneCountInBuildSettings);
        Debug.Log("Current scene: " + SceneManager.GetActiveScene().name);
        
        // List all scenes in build settings
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log($"Scene {i}: {sceneName} (Path: {scenePath})");
        }
        
        SetupAudio();
        SetupMenu();
        StartCoroutine(TypewriterEffect());
        StartCoroutine(TitleGlowEffect());
        SetupButtons();
        
        // Debug button setup
        DebugButtonSetup();
    }
    
    void DebugButtonSetup()
    {
        Debug.Log("=== BUTTON DEBUG INFO ===");
        
        if (playButton != null)
        {
            Debug.Log("Play button found: " + playButton.name);
            Debug.Log("Play button interactable: " + playButton.interactable);
            Debug.Log("Play button active: " + playButton.gameObject.activeInHierarchy);
            
            // Check if button has a graphic raycaster blocker
            Canvas parentCanvas = playButton.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                Debug.Log("Canvas found: " + parentCanvas.name);
                var raycaster = parentCanvas.GetComponent<GraphicRaycaster>();
                Debug.Log("GraphicRaycaster present: " + (raycaster != null));
            }
            
            // Check for EventSystem
            var eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            Debug.Log("EventSystem present: " + (eventSystem != null));
        }
        else
        {
            Debug.LogError("PLAY BUTTON IS NULL! Please assign it in the inspector.");
        }
    }
    
    void SetupAudio()
    {
        // Audio source for typewriter sounds
        typewriterAudio = gameObject.AddComponent<AudioSource>();
        typewriterAudio.volume = 0.3f;
        typewriterAudio.clip = typewriterSound;
        
        // Audio source for other effects
        effectsAudio = gameObject.AddComponent<AudioSource>();
        effectsAudio.volume = 0.4f;
    }
    
    void SetupMenu()
    {
        Camera.main.backgroundColor = new Color(0.02f, 0.05f, 0.1f);
        
        if(subtitleText != null)
        {
            subtitleText.text = "";
        }
    }
    
    void SetupButtons()
    {
        if(playButton != null)
        {
            playButton.onClick.RemoveAllListeners(); // Clear old connections
            playButton.onClick.AddListener(PlayGame);
            playButton.onClick.AddListener(PlayClickSound);
            Debug.Log("Play button listeners added successfully");
        }
        else
        {
            Debug.LogError("Cannot setup play button - it's null!");
        }
            
        if(quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners(); // Clear old connections
            quitButton.onClick.AddListener(QuitGame);
            quitButton.onClick.AddListener(PlayClickSound);
            Debug.Log("Quit button listeners added successfully");
        }
    }
    
    // Add this method for testing via keyboard
    void Update()
    {
        // Press P to test PlayGame function directly
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("P key pressed - calling PlayGame() directly");
            PlayGame();
        }
        
        // Press L to list scene info again
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("Current scene: " + SceneManager.GetActiveScene().name);
            Debug.Log("Build index: " + SceneManager.GetActiveScene().buildIndex);
        }
    }
    
    public void PlayClickSound()
    {
        Debug.Log("PlayClickSound called");
        if(clickSound != null && effectsAudio != null)
        {
            effectsAudio.PlayOneShot(clickSound);
        }
    }
    
    public void PlayGlitchSound()
    {
        if(glitchSound != null && effectsAudio != null)
        {
            effectsAudio.PlayOneShot(glitchSound);
        }
    }
    
    System.Collections.IEnumerator TypewriterEffect()
    {
       yield return new WaitForSeconds(1f);
       
       foreach(char letter in fullSubtitleText)
       {
           if(subtitleText != null)
           {
               subtitleText.text += letter;
               
               // Play typing sound for each character (except spaces and newlines)
               if(letter != ' ' && letter != '\n' && typewriterSound != null)
               {
                   typewriterAudio.Play();
               }
               
               yield return new WaitForSeconds(0.05f); // Realistic typing pace
           }
       }
    }
    
    System.Collections.IEnumerator TitleGlowEffect()
    {
        while(true)
        {
            if(titleText != null)
            {
                float glow = (Mathf.Sin(Time.time * 1.5f) + 1f) * 0.5f;
                titleText.color = Color.Lerp(
                    new Color(0, 0.8f, 0.6f), 
                    new Color(0, 1f, 1f), 
                    glow
                );
                
                // Occasionally play glitch sound with title effect
                if(Random.Range(0f, 1f) < 0.0002f) // Very rare
                {
                    PlayGlitchSound();
                }
            }
            yield return null;
        }
    }
    
    public void PlayGame()
    {
        Debug.Log("=== PLAY GAME CALLED ===");
        Debug.Log("BUTTON WAS CLICKED! PlayGame called!");
        Debug.Log("Attempting to load scene...");
        
        // Try multiple methods to be sure
        try 
        {
            // Method 1: By build index
            Debug.Log("Trying to load scene by index 1...");
            SceneManager.LoadScene(1);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to load scene by index: " + e.Message);
            
            // Method 2: By name as fallback
            try
            {
                Debug.Log("Trying to load scene by name 'Level1'...");
                SceneManager.LoadScene("Level1");
            }
            catch (System.Exception e2)
            {
                Debug.LogError("Failed to load scene by name: " + e2.Message);
            }
        }
    }
    
    public void QuitGame()
    {
        Debug.Log("QuitGame called");
        Application.Quit();
        Debug.Log("Game Quit!");
    }
}