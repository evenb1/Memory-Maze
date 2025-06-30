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
    
    void Start()
    {
        // Get story information from PlayerPrefs
        int storyLevel = PlayerPrefs.GetInt("StoryLevel", 1);
        storyType = PlayerPrefs.GetString("StoryType", "LevelComplete");
        targetScene = PlayerPrefs.GetString("NextScene", "Level1");
        
        // Get level data
        currentLevelData = LevelConfigs.GetLevelData(storyLevel);
        
        // Setup audio
        SetupAudio();
        
        // Setup UI
        SetupUI();
        
        // Show appropriate story
        ShowStoryBasedOnType();
        
        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log($"Story Transition: Level {storyLevel}, Type: {storyType}, Target: {targetScene}");
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
        Camera.main.backgroundColor = new Color(0.02f, 0.05f, 0.1f);
    }
    
    void ShowStoryBasedOnType()
    {
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
        // Set title
        if (levelTitleText != null)
        {
            levelTitleText.text = title;
            levelTitleText.color = currentLevelData.themeColor;
            StartCoroutine(TitleGlowEffect());
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
        }
        
        // Start typewriter effect
        if (storyText != null)
        {
            storyText.text = "";
            storyText.color = Color.white;
            StartCoroutine(TerminalTypewriterEffect(story, showContinueButton));
        }
    }
    
    System.Collections.IEnumerator TerminalTypewriterEffect(string text, bool showContinueButton)
    {
        isTyping = true;
        skipRequested = false;
        
        Debug.Log("🖨️ Starting typewriter effect...");
        
        for (int i = 0; i < text.Length; i++)
        {
            // Check if skip was requested
            if (skipRequested)
            {
                // Instantly show remaining text
                storyText.text = text;
                break;
            }
            
            char letter = text[i];
            storyText.text += letter;
            
            // Play typing sound for visible characters
            if (audioSource != null && typewriterSound != null && letter != ' ' && letter != '\n')
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(typewriterSound, 0.2f);
                audioSource.pitch = 1f;
            }
            
            // Wait before next character
            yield return new WaitForSeconds(typewriterSpeed);
        }
        
        isTyping = false;
        
        Debug.Log("🖨️ Typewriter effect complete!");
        
        // Start minimum display timer
        StartCoroutine(MinimumDisplayTimer(showContinueButton));
    }
    
    System.Collections.IEnumerator TitleGlowEffect()
    {
        if (levelTitleText == null) yield break;
        
        while (levelTitleText != null && levelTitleText.gameObject.activeInHierarchy)
        {
            float glow = (Mathf.Sin(Time.time * 1.5f) + 1f) * 0.5f;
            Color glowColor = Color.Lerp(currentLevelData.themeColor, 
                                       currentLevelData.themeColor * 1.5f, glow);
            levelTitleText.color = glowColor;
            yield return null;
        }
    }
    
    System.Collections.IEnumerator MinimumDisplayTimer(bool showButton)
    {
        yield return new WaitForSeconds(minimumDisplayTime);
        canContinue = true;
        
        if (continueButton != null && showButton)
        {
            continueButton.gameObject.SetActive(true);
        }
        
        Debug.Log("✅ Can now continue!");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                SkipTypewriter();
            }
            else if (canContinue)
            {
                ContinueToNext();
            }
        }
    }
    
    void SkipTypewriter()
    {
        Debug.Log("⏩ Skipping typewriter...");
        skipRequested = true;
    }
    
    public void ContinueToNext()
    {
        if (!canContinue) 
        {
            Debug.Log("❌ Cannot continue yet - still in minimum display time");
            return;
        }
        
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
        
        Debug.Log($"🚀 Loading next scene: {targetScene}");
        SceneManager.LoadScene(targetScene);
    }
}