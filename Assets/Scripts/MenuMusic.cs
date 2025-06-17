using UnityEngine;

public class MenuMusic : MonoBehaviour
{
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.volume = 0.2f;
        CreateMenuMusic();
        audioSource.Play();
    }
    
    void CreateMenuMusic()
    {
        int sampleRate = 22050;
        float duration = 8f;
        int samples = Mathf.FloorToInt(sampleRate * duration);
        
        AudioClip musicClip = AudioClip.Create("MenuMusic", samples, 1, sampleRate, false);
        float[] data = new float[samples];
        
        for(int i = 0; i < samples; i++)
        {
            float time = (float)i / sampleRate;
            
            // Ambient cyber sounds
            float bass = Mathf.Sin(2 * Mathf.PI * 80f * time) * 0.1f;
            float pad = Mathf.Sin(2 * Mathf.PI * 160f * time) * 0.05f;
            float ambient = Mathf.Sin(2 * Mathf.PI * 240f * time) * 0.03f;
            
            data[i] = bass + pad + ambient;
        }
        
        musicClip.SetData(data, 0);
        audioSource.clip = musicClip;
    }
}