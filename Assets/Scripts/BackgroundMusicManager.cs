using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundMusicManager : MonoBehaviour
{
    [Header("Level Music Tracks")]
    public AudioClip level1Music;        // Calm/Tutorial atmosphere
    public AudioClip level2Music;        // Building tension
    public AudioClip level3Music;        // High tension
    public AudioClip level4Music;        // Epic finale/dramatic
    public AudioClip menuMusic;          // For main menu
    public AudioClip storyMusic;         // For story transitions
    
    [Header("Audio Settings")]
    public float musicVolume = 0.6f;
    public float fadeSpeed = 1f;
    public bool playOnStart = true;
    
    [Header("Level Detection")]
    public bool autoDetectLevel = true;
    
    private AudioSource musicSource;
    private bool isPaused = false;
    private bool isFading = false;
    private float targetVolume;
    private AudioClip currentTrack;
    
    public static BackgroundMusicManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern - persist across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        SetupAudioSource();
        
        if (autoDetectLevel && playOnStart)
        {
            DetectLevelAndPlayMusic();
        }
        
        // Listen for scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        Debug.Log("BackgroundMusicManager initialized and ready!");
    }
    
    void SetupAudioSource()
    {
        musicSource = GetComponent<AudioSource>();
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        
        musicSource.volume = musicVolume;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f; // 2D audio
        
        targetVolume = musicVolume;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (autoDetectLevel)
        {
            DetectLevelAndPlayMusic();
        }
    }
    
    void DetectLevelAndPlayMusic()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        AudioClip targetClip = null;
        
        Debug.Log($"🎵 Detecting music for scene: {sceneName}");
        
        if (sceneName.Contains("Level1"))
        {
            targetClip = level1Music;
            Debug.Log("🎵 Playing Level 1 music - Tutorial/Calm");
        }
        else if (sceneName.Contains("Level2"))
        {
            targetClip = level2Music;
            Debug.Log("🎵 Playing Level 2 music - Building Tension");
        }
        else if (sceneName.Contains("Level3"))
        {
            targetClip = level3Music;
            Debug.Log("🎵 Playing Level 3 music - High Tension");
        }
        else if (sceneName.Contains("Level4"))
        {
            targetClip = level4Music;
Debug.Log("🎵 Playing Level 4 music - EPIC FINALE!");  // Capital 'L' in Log
        }
        else if (sceneName.Contains("MainMenu") || sceneName.Contains("Menu"))
        {
            targetClip = menuMusic;
            Debug.Log("🎵 Playing Menu music");
        }
        else if (sceneName.Contains("Story") || sceneName.Contains("Transition"))
        {
            targetClip = storyMusic;
            Debug.Log("🎵 Playing Story transition music");
        }
        else
        {
            Debug.Log($"⚠️ No music assigned for scene: {sceneName}");
        }
        
        if (targetClip != null)
        {
            PlayMusic(targetClip);
        }
    }
    
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("Cannot play null audio clip!");
            return;
        }
        
        if (currentTrack == clip && musicSource.isPlaying)
        {
            Debug.Log("Same track already playing - no change needed");
            return;
        }
        
        currentTrack = clip;
        
        if (musicSource.isPlaying)
        {
            // Fade out current track, then fade in new one
            StartCoroutine(CrossFadeMusic(clip));
        }
        else
        {
            // Start new track immediately
            musicSource.clip = clip;
            musicSource.Play();
            StartCoroutine(FadeIn());
        }
        
        Debug.Log($"🎵 Playing music: {clip.name}");
    }
    
    System.Collections.IEnumerator CrossFadeMusic(AudioClip newClip)
    {
        isFading = true;
        
        // Fade out current music
        yield return StartCoroutine(FadeOut());
        
        // Switch to new clip
        musicSource.clip = newClip;
        musicSource.Play();
        
        // Fade in new music
        yield return StartCoroutine(FadeIn());
        
        isFading = false;
    }
    
    System.Collections.IEnumerator FadeOut()
    {
        float startVolume = musicSource.volume;
        
        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * fadeSpeed * Time.unscaledDeltaTime;
            yield return null;
        }
        
        musicSource.volume = 0f;
        musicSource.Stop();
    }
    
    System.Collections.IEnumerator FadeIn()
    {
        musicSource.volume = 0f;
        float targetVol = isPaused ? 0f : targetVolume;
        
        while (musicSource.volume < targetVol)
        {
            musicSource.volume += targetVolume * fadeSpeed * Time.unscaledDeltaTime;
            yield return null;
        }
        
        musicSource.volume = targetVol;
    }
    
    public void PauseMusic()
    {
        if (isPaused) return;
        
        isPaused = true;
        Debug.Log("🎵 Music paused");
        
        if (musicSource.isPlaying)
        {
            StartCoroutine(FadeOutForPause());
        }
    }
    
    public void ResumeMusic()
    {
        if (!isPaused) return;
        
        isPaused = false;
        Debug.Log("🎵 Music resumed");
        
        if (musicSource.clip != null)
        {
            if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }
            StartCoroutine(FadeInFromPause());
        }
    }
    
    System.Collections.IEnumerator FadeOutForPause()
    {
        float startVolume = musicSource.volume;
        
        while (musicSource.volume > 0 && isPaused)
        {
            musicSource.volume -= startVolume * fadeSpeed * 2f * Time.unscaledDeltaTime;
            yield return null;
        }
        
        if (isPaused)
        {
            musicSource.volume = 0f;
            musicSource.Pause();
        }
    }
    
    System.Collections.IEnumerator FadeInFromPause()
    {
        if (musicSource.clip != null && !musicSource.isPlaying)
        {
            musicSource.UnPause();
        }
        
        musicSource.volume = 0f;
        
        while (musicSource.volume < targetVolume && !isPaused)
        {
            musicSource.volume += targetVolume * fadeSpeed * 2f * Time.unscaledDeltaTime;
            yield return null;
        }
        
        if (!isPaused)
        {
            musicSource.volume = targetVolume;
        }
    }
    
    public void StopMusic()
    {
        StopAllCoroutines();
        musicSource.Stop();
        currentTrack = null;
        Debug.Log("🎵 Music stopped");
    }
    
    public void SetVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        targetVolume = musicVolume;
        
        if (!isPaused)
        {
            musicSource.volume = targetVolume;
        }
        
        Debug.Log($"🎵 Music volume set to: {musicVolume:F2}");
    }
    
    public void PlaySpecificLevelMusic(int levelNumber)
    {
        AudioClip targetClip = null;
        
        switch (levelNumber)
        {
            case 1:
                targetClip = level1Music;
                break;
            case 2:
                targetClip = level2Music;
                break;
            case 3:
                targetClip = level3Music;
                break;
            case 4:
                targetClip = level4Music;
                break;
            default:
                Debug.LogWarning($"No music defined for level {levelNumber}");
                return;
        }
        
        PlayMusic(targetClip);
    }
    
    // Public getters for other systems
    public bool IsPlaying()
    {
        return musicSource != null && musicSource.isPlaying;
    }
    
    public bool IsPaused()
    {
        return isPaused;
    }
    
    public AudioClip GetCurrentTrack()
    {
        return currentTrack;
    }
    
    public float GetVolume()
    {
        return musicVolume;
    }
    
    // Manual music control methods
    public void PlayLevel1Music() => PlayMusic(level1Music);
    public void PlayLevel2Music() => PlayMusic(level2Music);
    public void PlayLevel3Music() => PlayMusic(level3Music);
    public void PlayLevel4Music() => PlayMusic(level4Music);
    public void PlayMenuMusic() => PlayMusic(menuMusic);
    public void PlayStoryMusic() => PlayMusic(storyMusic);
    
    void Update()
    {
        // Handle volume changes smoothly
        if (!isFading && !isPaused && musicSource.volume != targetVolume)
        {
            musicSource.volume = Mathf.MoveTowards(musicSource.volume, targetVolume, fadeSpeed * Time.unscaledDeltaTime);
        }
    }
    
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    // Debug information
    void OnGUI()
    {
        if (Application.isPlaying && musicSource != null)
        {
            GUI.color = Color.white;
            GUI.Label(new Rect(10, Screen.height - 60, 300, 20), $"Music: {(currentTrack ? currentTrack.name : "None")}");
            GUI.Label(new Rect(10, Screen.height - 40, 300, 20), $"Volume: {musicSource.volume:F2} | Paused: {isPaused}");
            GUI.Label(new Rect(10, Screen.height - 20, 300, 20), $"Playing: {musicSource.isPlaying}");
        }
    }
}