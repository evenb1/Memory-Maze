using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        CreateBackgroundMusic();
        audioSource.Play();
    }
    
    void CreateBackgroundMusic()
    {
        // Create ambient electronic background music
        int sampleRate = 44100;
        float duration = 10f; // 10 second loop
        int samples = Mathf.FloorToInt(sampleRate * duration);
        
        AudioClip musicClip = AudioClip.Create("BackgroundMusic", samples, 1, sampleRate, false);
        float[] data = new float[samples];
        
        // Create layered ambient tones
        for(int i = 0; i < samples; i++)
        {
            float time = (float)i / sampleRate;
            
            // Base drone
            float drone = Mathf.Sin(2 * Mathf.PI * 60f * time) * 0.1f;
            
            // Ambient pad
            float pad = Mathf.Sin(2 * Mathf.PI * 120f * time) * 0.05f * Mathf.Sin(time * 0.5f);
            
            // Subtle pulse
            float pulse = Mathf.Sin(2 * Mathf.PI * 180f * time) * 0.03f * (Mathf.Sin(time * 2f) + 1f) * 0.5f;
            
            data[i] = drone + pad + pulse;
        }
        
        musicClip.SetData(data, 0);
        audioSource.clip = musicClip;
    }
}