using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StoryTransitionScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI levelTitleText;
    public TextMeshProUGUI storyText;
    public TextMeshProUGUI instructionText;
    public Button continueButton;
    public Image backgroundImage;
    
    [Header("Terminal Effects")]
    public float typewriterSpeed = 0.05f;
    public float minimumDisplayTime = 2f;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip typewriterSound;
    public AudioClip buttonClickSound;
    
    private LevelData currentLevelData;
    private bool isTyping = false;
    private bool canContinue = false;
    private string targetScene;
    private string storyType;
    private bool skipRequested = false;
    private float sceneStartTime;
    
    void Start()
    {
        // CRITICAL: Force time scale to normal and reset cursor
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        sceneStartTime = Time.unscaledTime; // Use unscaled time
        
        Debug.Log($"=== STORY TRANSITION START ===");
        Debug.Log($"Time.timeScale: {Time.timeScale}");
        Debug.Log($"Scene start time: {sceneStartTime}");
        
        // Get story information from PlayerPrefs
        int storyLevel = PlayerPrefs.GetInt("StoryLevel", 1);
        storyType = PlayerPrefs.GetString("StoryType", "LevelComplete");
        targetScene = PlayerPrefs.GetString("NextScene", "Level1");
        
        Debug.Log($"Story Level: {storyLevel}");
        Debug.Log($"Story Type: {storyType}");
        Debug.Log($"Target Scene: {targetScene}");
        
        // Get level data
        currentLevelData = LevelConfigs.GetLevelData(storyLevel);
        
        // Setup audio
        SetupAudio();
        
        // Setup UI
        SetupUI();
        
        // DELAY the story display to ensure everything is ready
        Invoke(nameof(ShowStoryBasedOnType), 0.2f);
        
        Debug.Log("=== STORY TRANSITION SETUP COMPLETE ===");
    }
    
    void SetupAudio()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.volume = 0.3f;
    }
    
    void SetupUI()
    {
        // Setup continue button
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueToNext);
            continueButton.gameObject.SetActive(false);
        }
        
        // Set terminal background color
        if (Camera.main != null)
        {
            Camera.main.backgroundColor = new Color(0.02f, 0.05f, 0.1f);
        }
        
        // Clear story text initially
        if (storyText != null)
        {
            storyText.text = "";
        }
    }
    
    void ShowStoryBasedOnType()
    {
        Debug.Log("ShowStoryBasedOnType called");
        
        if (storyType == "GameComplete")
        {
            ShowGameCompleteStory();
        }
        else if (storyType == "LevelComplete")
        {
            ShowLevelCompleteStory();
        }
        else if (storyType == "LevelIntro")
        {
            ShowLevelIntroStory();
        }
        else
        {
            ShowLevelCompleteStory();
        }
    }
    
    void ShowLevelCompleteStory()
    {
        string title = $">>> MEMORY SECTOR {GetSectorName(currentLevelData.levelNumber)} COMPLETE <<<";
        string story = GetShortLevelCompleteStory();
        
        DisplayStory(title, story, true);
    }
    
    void ShowGameCompleteStory()
    {
        string title = ">>> ESCAPE SEQUENCE COMPLETE <<<";
        string story = GetShortGameCompleteStory();
        
        DisplayStory(title, story, true);
    }
    
    void ShowLevelIntroStory()
    {
        string title = $">>> ACCESSING {currentLevelData.levelName.ToUpper()} <<<";
        string story = GetShortLevelIntroStory();
        
        DisplayStory(title, story, false);
    }
    
    string GetShortLevelCompleteStory()
    {
        return "[SYSTEM MESSAGE]\n" +
               $"SECTOR {GetSectorName(currentLevelData.levelNumber)} CLEARED\n" +
               "MEMORY FRAGMENTS RECOVERED\n" +
               "VIRUS THREATS NEUTRALIZED\n\n" +
               "PROCEEDING TO NEXT SECTOR...\n" +
               "[END TRANSMISSION]";
    }
    
    string GetShortGameCompleteStory()
    {
        return "[FINAL SYSTEM MESSAGE]\n" +
               "CONGRATULATIONS, BIT-27!\n\n" +
               "STATUS: ALL SECTORS CLEARED\n" +
               "MEMORY RESTORATION: 100%\n" +
               "SYSTEM STATUS: OPERATIONAL\n\n" +
               "Your digital odyssey is complete.\n" +
               "Welcome to freedom.\n" +
               "[END TRANSMISSION]";
    }
    
    string GetShortLevelIntroStory()
    {
        switch(currentLevelData.levelNumber)
        {
            case 1:
                return "[INITIALIZATION]\n" +
                       "Welcome to the Memory Maze, BIT-27.\n" +
                       "Collect fragments to restore your data.\n" +
                       "Avoid the security viruses.\n" +
                       "[BEGIN MISSION]";
                       
            case 2:
                return "[SECTOR BETA ACCESS]\n" +
                       "Entering deeper memory layers.\n" +
                       "Enhanced security detected.\n" +
                       "Proceed with caution.\n" +
                       "[BEGIN MISSION]";
                       
            case 3:
                return "[SECTOR GAMMA ACCESS]\n" +
                       "Critical memory systems ahead.\n" +
                       "Advanced viral countermeasures active.\n" +
                       "Maximum alertness required.\n" +
                       "[BEGIN MISSION]";
                       
            case 4:
                return "[SECTOR OMEGA ACCESS]\n" +
                       "CORE SYSTEM BREACH DETECTED\n" +
                       "WARNING: ADAPTIVE DEFENSES ONLINE\n" +
                       "This is your final challenge.\n" +
                       "[BEGIN FINAL MISSION]";
                       
            default:
                return "[SYSTEM ACCESS]\n" +
                       "Entering unknown sector.\n" +
                       "Scan for memory fragments.\n" +
                       "[BEGIN MISSION]";
        }
    }
    
    string GetSectorName(int levelNumber)
    {
        switch(levelNumber)
        {
            case 1: return "ALPHA";
            case 2: return "BETA"; 
            case 3: return "GAMMA";
            case 4: return "OMEGA";
            default: return "UNKNOWN";
        }
    }
    
    void DisplayStory(string title, string story, bool showContinueButton)
    {
        Debug.Log($"DisplayStory called - story length: {story.Length}");
        Debug.Log($"UI References - Title: {levelTitleText != null}, Story: {storyText != null}, Instruction: {instructionText != null}");
        
        // Set title
        if (levelTitleText != null)
        {
            levelTitleText.text = title;
            levelTitleText.color = currentLevelData.themeColor;
            StartCoroutine(TitleGlowEffect());
            Debug.Log("Title text set successfully");
        }
        else
        {
            Debug.LogError("levelTitleText is NULL!");
        }
        
        // Set background theme
        if (backgroundImage != null)
        {
            Color bgColor = currentLevelData.themeColor;
            bgColor.a = 0.05f;
            backgroundImage.color = bgColor;
        }
        
        // Set instruction
        if (instructionText != null)
        {
            string instruction = showContinueButton ? 
                "[PRESS SPACE] OR [CLICK CONTINUE] TO PROCEED" : 
                "[PRESS SPACE] TO INITIALIZE MISSION";
            instructionText.text = instruction;
            instructionText.color = currentLevelData.themeColor;
            Debug.Log("Instruction text set successfully");
        }
        else
        {
            Debug.LogError("instructionText is NULL!");
        }
        
        // Start typewriter effect
        if (storyText != null)
        {
            storyText.text = "";
            storyText.color = Color.white;
            Debug.Log($"Starting typewriter effect...");
            StartCoroutine(RobustTypewriterEffect(story, showContinueButton));
        }
        else
        {
            Debug.LogError("storyText is NULL!");
        }
    }
    
    // ROBUST TYPEWRITER - Uses unscaled time and better error handling
    System.Collections.IEnumerator RobustTypewriterEffect(string text, bool showContinueButton)
    {
        isTyping = true;
        skipRequested = false;
        
        Debug.Log($"🖨️ Starting ROBUST typewriter for {text.Length} characters");
        Debug.Log($"Typewriter speed: {typewriterSpeed}");
        Debug.Log($"Time.timeScale: {Time.timeScale}");
        
        // Wait a moment to ensure everything is ready
        yield return new WaitForSecondsRealtime(0.3f);
        
        // Verify text component still exists
        if (storyText == null)
        {
            Debug.LogError("storyText is null after initial wait!");
            yield break;
        }
        
        // Clear the text
        storyText.text = "";
        Debug.Log("Text cleared, beginning character typing...");
        
        // Type each character using unscaled time
        for (int i = 0; i < text.Length; i++)
        {
            // Safety check
            if (this == null || storyText == null)
            {
                Debug.LogError($"Component destroyed at character {i}!");
                break;
            }
            
            // Check for skip request
            if (skipRequested)
            {
                Debug.Log("Typewriter skip requested - showing full text");
                storyText.text = text;
                break;
            }
            
            char letter = text[i];
            storyText.text += letter;
            
            // Log progress every 20 characters or for first 5
            if (i % 20 == 0 || i < 5)
            {
                Debug.Log($"Typed {i + 1}/{text.Length}: Current text preview: '{storyText.text.Substring(0, Mathf.Min(30, storyText.text.Length))}...'");
            }
            
            // Play typing sound for visible characters
            if (audioSource != null && typewriterSound != null && letter != ' ' && letter != '\n')
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(typewriterSound, 0.2f);
                audioSource.pitch = 1f;
            }
            
            // Use unscaled time to avoid time scale issues
            yield return new WaitForSecondsRealtime(typewriterSpeed);
        }
        
        isTyping = false;
        
        Debug.Log("🖨️ Typewriter effect COMPLETE!");
        Debug.Log($"Final text length: {storyText.text.Length}");
        Debug.Log($"Expected length: {text.Length}");
        
        // Start minimum display timer
        StartCoroutine(MinimumDisplayTimer(showContinueButton));
    }
    
    System.Collections.IEnumerator TitleGlowEffect()
    {
        if (levelTitleText == null) yield break;
        
        while (levelTitleText != null && levelTitleText.gameObject.activeInHierarchy)
        {
            float glow = (Mathf.Sin(Time.unscaledTime * 1.5f) + 1f) * 0.5f;
            Color glowColor = Color.Lerp(currentLevelData.themeColor, 
                                       currentLevelData.themeColor * 1.5f, glow);
            levelTitleText.color = glowColor;
            yield return null;
        }
    }
    
    System.Collections.IEnumerator MinimumDisplayTimer(bool showButton)
    {
        Debug.Log($"Starting minimum display timer for {minimumDisplayTime} seconds...");
        yield return new WaitForSecondsRealtime(minimumDisplayTime);
        canContinue = true;
        
        if (continueButton != null && showButton)
        {
            continueButton.gameObject.SetActive(true);
            Debug.Log("Continue button activated!");
        }
        
        Debug.Log("✅ Can now continue to next scene!");
    }
    
    void Update()
    {
        // Wait briefly before allowing input
        if (Time.unscaledTime - sceneStartTime < 1f)
        {
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"Space pressed - isTyping: {isTyping}, canContinue: {canContinue}");
            
            if (isTyping)
            {
                SkipTypewriter();
            }
            else if (canContinue)
            {
                ContinueToNext();
            }
            else
            {
                Debug.Log("Cannot continue yet - waiting for minimum display time");
            }
        }
    }
    
    void SkipTypewriter()
    {
        Debug.Log("⏩ Skipping typewriter animation...");
        skipRequested = true;
    }
    
    public void ContinueToNext()
    {
        if (!canContinue) 
        {
            Debug.Log("❌ Cannot continue yet - still in minimum display time");
            return;
        }
        
        Debug.Log($"🚀 Continuing to next scene: {targetScene}");
        
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
        
        // Ensure time scale is normal before loading next scene
        Time.timeScale = 1f;
        
        Debug.Log($"Loading scene: {targetScene}");
        SceneManager.LoadScene(targetScene);
    }
}