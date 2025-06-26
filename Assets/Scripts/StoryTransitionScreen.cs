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
    public float typewriterSpeed = 0.03f;
    public float minimumDisplayTime = 3f;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip typewriterSound;
    public AudioClip buttonClickSound;
    
    private LevelData currentLevelData;
    private bool isTyping = false;
    private bool canContinue = false;
    private string targetScene;
    private string storyType;
    
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
    }
    
    void SetupAudio()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.volume = 0.5f;
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
        string story = AddTerminalFormatting(currentLevelData.levelCompleteText);
        
        DisplayStory(title, story, true);
    }
    
    void ShowGameCompleteStory()
    {
        string title = ">>> ESCAPE SEQUENCE COMPLETE <<<";
        string story = AddTerminalFormatting(currentLevelData.levelCompleteText + 
                      "\n\n[SYSTEM MESSAGE]\n" +
                      "CONGRATULATIONS, BIT-27!\n\n" +
                      "STATUS: ALL MEMORY SECTORS CLEARED\n" +
                      "VIRUS THREATS: NEUTRALIZED\n" +
                      "DATA INTEGRITY: 100%\n" +
                      "SYSTEM STATUS: FULLY OPERATIONAL\n\n" +
                      "Your digital odyssey is complete.\n" +
                      "Welcome to your liberated existence.");
        
        DisplayStory(title, story, true);
    }
    
    void ShowLevelIntroStory()
    {
        string title = $">>> ACCESSING {currentLevelData.levelName.ToUpper()} <<<";
        string story = AddTerminalFormatting(currentLevelData.levelIntroText);
        
        DisplayStory(title, story, false);
    }
    
    string AddTerminalFormatting(string originalText)
    {
        return "[SYSTEM BOOT]\n" +
               "Initializing memory recovery protocol...\n" +
               "Loading sector data...\n" +
               "READY\n\n" +
               "================================\n\n" +
               originalText +
               "\n\n================================\n" +
               "[END TRANSMISSION]";
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
        
        // Start typewriter
        if (storyText != null)
        {
            storyText.text = "";
            storyText.color = Color.white;
            StartCoroutine(TerminalTypewriterEffect(story));
        }
        
        // Start timer
        StartCoroutine(MinimumDisplayTimer(showContinueButton));
    }
    
    System.Collections.IEnumerator TerminalTypewriterEffect(string text)
    {
        isTyping = true;
        
        foreach (char letter in text)
        {
            if (storyText != null)
            {
                storyText.text += letter;
                
                // Play typing sound
                if (audioSource != null && typewriterSound != null && letter != ' ')
                {
                    audioSource.pitch = Random.Range(0.9f, 1.1f);
                    audioSource.PlayOneShot(typewriterSound, 0.2f);
                    audioSource.pitch = 1f;
                }
                
                yield return new WaitForSeconds(typewriterSpeed);
            }
        }
        
        isTyping = false;
    }
    
    System.Collections.IEnumerator TitleGlowEffect()
    {
        if (levelTitleText == null) yield break;
        
        while (true)
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
        StopAllCoroutines();
        isTyping = false;
        canContinue = true;
        
        if (storyText != null && currentLevelData != null)
        {
            if (storyType == "GameComplete")
            {
                storyText.text = AddTerminalFormatting(currentLevelData.levelCompleteText + 
                               "\n\n[SYSTEM MESSAGE]\nCONGRATULATIONS, BIT-27!");
            }
            else if (storyType == "LevelComplete")
            {
                storyText.text = AddTerminalFormatting(currentLevelData.levelCompleteText);
            }
            else
            {
                storyText.text = AddTerminalFormatting(currentLevelData.levelIntroText);
            }
        }
        
        if (continueButton != null && (storyType == "LevelComplete" || storyType == "GameComplete"))
        {
            continueButton.gameObject.SetActive(true);
        }
        
        StartCoroutine(TitleGlowEffect());
    }
    
    public void ContinueToNext()
    {
        if (!canContinue) return;
        
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
        
        Debug.Log($"Loading next scene: {targetScene}");
        SceneManager.LoadScene(targetScene);
    }
}