using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Level4DramaticController : MonoBehaviour
{
    [Header("Required UI Elements")]
    public TextMeshProUGUI coreSystemText;
    public Image screenOverlay;
    
    [Header("Lighting")]
    public Light[] emergencyLights;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip earthquakeSound;
    public AudioClip alarmSound;
    
    [Header("Earthquake Settings")]
    public float earthquakeDuration = 3f;
    public float cameraShakeIntensity = 0.5f;
    
    // Internal variables
    private int currentEscalationLevel = 0;
    private bool isEarthquakeActive = false;
    private PlayerController playerController;
    private Camera mainCamera;
    private Vector3 originalCameraPosition;
    private GameManager gameManager;
    
    void Start()
    {
        // Find required components
        playerController = FindFirstObjectByType<PlayerController>();
        mainCamera = Camera.main;
        gameManager = FindFirstObjectByType<GameManager>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Store original camera position
        if (mainCamera != null)
        {
            originalCameraPosition = mainCamera.transform.localPosition;
        }
        
        // Auto-find lights if not assigned
        if (emergencyLights == null || emergencyLights.Length == 0)
        {
            emergencyLights = FindObjectsOfType<Light>();
        }
        
        // Hide core system text initially
        if (coreSystemText != null)
        {
            coreSystemText.gameObject.SetActive(false);
        }
        
        Debug.Log("Level 4 Dramatic Controller ready for earthquakes!");
    }
    
    public void OnFragmentCollected(int fragmentNumber)
    {
        Debug.Log($"🌋 Level 4: Fragment {fragmentNumber} collected - checking for earthquake...");
        
        // Determine what should happen
        if (fragmentNumber == 1)
        {
            ShowCoreMessage("Core system online... scanning for intrusions...", Color.yellow);
        }
        else if (fragmentNumber == 2)
        {
            ShowCoreMessage("WARNING: Unauthorized access detected!", Color.orange);
        }
        else if (fragmentNumber == 3)
        {
            ShowCoreMessage("ALERT: Maze reconfiguration initiated!", Color.red);
            StartEarthquake(2f, 0.3f); // 2 second earthquake, light intensity
        }
        else if (fragmentNumber == 4)
        {
            ShowCoreMessage("Activating adaptive security protocols...", Color.red);
            StartEarthquake(3f, 0.5f); // 3 second earthquake, medium intensity
        }
        else if (fragmentNumber == 5)
        {
            ShowCoreMessage("DANGER: Core system defensive mode engaged!", Color.red);
            StartEarthquake(4f, 0.7f); // 4 second earthquake, high intensity
            SwitchToEmergencyLighting();
        }
        else if (fragmentNumber == 6)
        {
            ShowCoreMessage("CRITICAL: Emergency containment protocols!", Color.red);
            StartEarthquake(4f, 0.8f);
        }
        else if (fragmentNumber == 7)
        {
            ShowCoreMessage("SYSTEM PANIC: All defenses activated!", Color.red);
            StartEarthquake(5f, 1f); // Maximum earthquake chaos
            PlayAlarm();
        }
        else if (fragmentNumber == 8)
        {
            ShowCoreMessage("MEMORY RESTORATION COMPLETE! Core system shutting down!", Color.cyan);
            StopAlarm();
            RestoreNormalLighting();
        }
    }
    
    void StartEarthquake(float duration, float intensity)
    {
        if (isEarthquakeActive) return;
        
        Debug.Log($"🌋 EARTHQUAKE STARTING! Duration: {duration}s, Intensity: {intensity}");
        StartCoroutine(EarthquakeSequence(duration, intensity));
    }
    
    IEnumerator EarthquakeSequence(float duration, float intensity)
    {
        isEarthquakeActive = true;
        
        // DON'T FREEZE THE PLAYER - Let them keep moving during earthquake!
        // The earthquake should be chaotic but still allow gameplay
        
        // Show status message
        if (gameManager != null)
        {
            gameManager.UpdateStatusMessage("⚠️ SYSTEM RECONFIGURING - WALLS MOVING!");
        }
        
        // Start camera shake
        StartCoroutine(CameraShake(duration, intensity));
        
        // Play earthquake sound
        if (audioSource != null && earthquakeSound != null)
        {
            audioSource.clip = earthquakeSound;
            audioSource.loop = true;
            audioSource.volume = intensity * 0.7f;
            audioSource.Play();
        }
        
        // Screen flash effects
        StartCoroutine(ScreenFlash(duration, intensity));
        
        // Flicker lights
        StartCoroutine(FlickerLights(duration));
        
        // Wait for earthquake duration
        yield return new WaitForSeconds(duration);
        
        // Stop earthquake sound
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
        
        isEarthquakeActive = false;
        
        Debug.Log("🌋 Earthquake sequence complete - player can continue moving!");
    }
    
    IEnumerator CameraShake(float duration, float intensity)
    {
        float elapsed = 0f;
        
        while (elapsed < duration && mainCamera != null)
        {
            // Generate random shake
            float shakeX = Random.Range(-1f, 1f) * cameraShakeIntensity * intensity;
            float shakeY = Random.Range(-1f, 1f) * cameraShakeIntensity * intensity;
            float shakeZ = Random.Range(-1f, 1f) * cameraShakeIntensity * intensity * 0.5f;
            
            Vector3 shakeOffset = new Vector3(shakeX, shakeY, shakeZ);
            
            // Apply shake to camera's WORLD position (not local)
            mainCamera.transform.position = mainCamera.transform.position + shakeOffset;
            
            elapsed += Time.deltaTime;
            yield return new WaitForSeconds(0.02f); // Consistent shake timing
        }
        
        Debug.Log("Camera shake complete!");
    }
    
    IEnumerator ScreenFlash(float duration, float intensity)
    {
        if (screenOverlay == null) yield break;
        
        float elapsed = 0f;
        Color originalColor = screenOverlay.color;
        
        while (elapsed < duration)
        {
            // Random red flashing
            float alpha = Random.Range(0f, intensity * 0.4f);
            screenOverlay.color = new Color(1f, 0.2f, 0.2f, alpha);
            
            elapsed += Time.deltaTime;
            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
        }
        
        // Restore original
        screenOverlay.color = originalColor;
    }
    
    IEnumerator FlickerLights(float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            foreach (Light light in emergencyLights)
            {
                if (light != null)
                {
                    light.intensity = light.intensity * Random.Range(0.3f, 1f);
                }
            }
            
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
            
            // Restore
            foreach (Light light in emergencyLights)
            {
                if (light != null)
                {
                    light.intensity = Mathf.Clamp(light.intensity / Random.Range(0.3f, 1f), 0.5f, 2f);
                }
            }
            
            elapsed += Time.deltaTime;
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
        }
    }
    
    void SwitchToEmergencyLighting()
    {
        foreach (Light light in emergencyLights)
        {
            if (light != null)
            {
                light.color = Color.red;
                light.intensity = light.intensity * 1.2f;
            }
        }
        
        Debug.Log("🚨 Emergency lighting activated!");
    }
    
    void RestoreNormalLighting()
    {
        foreach (Light light in emergencyLights)
        {
            if (light != null)
            {
                light.color = Color.white;
                light.intensity = light.intensity / 1.2f;
            }
        }
        
        Debug.Log("💡 Normal lighting restored!");
    }
    
    void PlayAlarm()
    {
        if (audioSource != null && alarmSound != null)
        {
            audioSource.clip = alarmSound;
            audioSource.loop = true;
            audioSource.volume = 0.6f;
            audioSource.Play();
        }
        
        Debug.Log("🚨 ALARM ACTIVATED!");
    }
    
    void StopAlarm()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
        
        Debug.Log("🔇 Alarm stopped.");
    }
    
    void ShowCoreMessage(string message, Color color)
    {
        if (coreSystemText != null)
        {
            StopCoroutine(nameof(DisplayMessage));
            StartCoroutine(DisplayMessage(message, color));
        }
        
        Debug.Log($"💻 CORE SYSTEM: {message}");
    }
    
    IEnumerator DisplayMessage(string message, Color color)
    {
        coreSystemText.gameObject.SetActive(true);
        coreSystemText.text = message;
        coreSystemText.color = color;
        
        yield return new WaitForSeconds(3f);
        
        coreSystemText.gameObject.SetActive(false);
    }
    
    // Public methods for other systems
    public bool IsEarthquakeActive()
    {
        return isEarthquakeActive;
    }
    
    public void ForceStopEarthquake()
    {
        StopAllCoroutines();
        isEarthquakeActive = false;
        
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        if (mainCamera != null)
        {
            mainCamera.transform.localPosition = originalCameraPosition;
        }
        
        Debug.Log("⏹️ Earthquake forcibly stopped!");
    }
}