using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelIntroOverlay : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject introPanel;
    public TextMeshProUGUI levelTitleText;
    public TextMeshProUGUI storyText;
    public TextMeshProUGUI instructionText;
    public Image backgroundOverlay;
    
    [Header("Settings")]
    public float typewriterSpeed = 0.03f;
    public bool showOnStart = true;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip typewriterSound;
    
    private LevelData currentLevelData;
    private bool isShowing = false;
    private bool isTyping = false;
    
    void Start()
    {
        if (showOnStart)
        {
            ShowLevelIntro();
        }
    }
    
    public void ShowLevelIntro()
    {
        // Get current level data
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        int levelNumber = 1;
        
        if (sceneName.Contains("Level"))
        {
            string levelNum = sceneName.Replace("Level", "");
            if (int.TryParse(levelNum, out int level))
            {
                levelNumber = level;
            }
        }
        
        currentLevelData = LevelConfigs.GetLevelData(levelNumber);
        
        // Setup UI
        if (introPanel != null)
        {
            introPanel.SetActive(true);
        }
        
        if (levelTitleText != null)
        {
            levelTitleText.text = currentLevelData.levelName.ToUpper();
            levelTitleText.color = currentLevelData.themeColor;
        }
        
        if (backgroundOverlay != null)
        {
            Color bgColor = currentLevelData.themeColor;
            bgColor.a = 0.8f; // Semi-transparent overlay
            backgroundOverlay.color = bgColor;
        }
        
        if (instructionText != null)
        {
            instructionText.text = "Press SPACE to begin mission";
            instructionText.color = currentLevelData.themeColor;
        }
        
        // Setup audio
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.volume = 0.3f;
        
        // Start story text
        if (storyText != null)
        {
            storyText.text = "";
            storyText.color = Color.white;
            StartCoroutine(TypewriterEffect(currentLevelData.levelIntroText));
        }
        
        // Pause game and show cursor
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        isShowing = true;
    }
    
    System.Collections.IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;
        
        foreach (char letter in text)
        {
            if (storyText != null)
            {
                storyText.text += letter;
                
                // Play typing sound
                if (letter != ' ' && letter != '\n' && typewriterSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(typewriterSound, 0.2f);
                }
                
                yield return new WaitForSecondsRealtime(typewriterSpeed);
            }
        }
        
        isTyping = false;
    }
    
    void Update()
    {
        if (!isShowing) return;
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                // Skip typewriter effect
                StopAllCoroutines();
                if (storyText != null && currentLevelData != null)
                {
                    storyText.text = currentLevelData.levelIntroText;
                }
                isTyping = false;
            }
            else
            {
                // Close intro and start level
                CloseIntro();
            }
        }
    }
    
    void CloseIntro()
    {
        if (introPanel != null)
        {
            introPanel.SetActive(false);
        }
        
        // Resume game
        Time.timeScale = 1f;
        
        // Lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        isShowing = false;
        
        Debug.Log($"Level {currentLevelData.levelNumber} intro completed - game started!");
    }
    
    // Public method to show intro manually
    public void DisplayIntro()
    {
        ShowLevelIntro();
    }
}