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
        SetupAudio();
        SetupMenu();
        StartCoroutine(TypewriterEffect());
        StartCoroutine(TitleGlowEffect());
        SetupButtons();
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
            playButton.onClick.AddListener(PlayGame);
            playButton.onClick.AddListener(PlayClickSound);
        }
            
        if(quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
            quitButton.onClick.AddListener(PlayClickSound);
        }
    }
    
    public void PlayClickSound()
    {
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
        SceneManager.LoadScene("Level1");
    }
    
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit!");
    }
}