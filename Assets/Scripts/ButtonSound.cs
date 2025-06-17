using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = 0.3f;
        CreateBetterButtonSound();
    }
    
    void CreateBetterButtonSound()
    {
        // Better click sound - short and crisp
        int sampleRate = 22050;
        int samples = 2200; // 0.1 seconds
        
        AudioClip clip = AudioClip.Create("ButtonClick", samples, 1, sampleRate, false);
        float[] data = new float[samples];
        
        for(int i = 0; i < samples; i++)
        {
            float time = (float)i / sampleRate;
            
            // Quick frequency sweep for "click" effect
            float frequency = 1200f - (time * 800f); // Starts high, goes lower
            float envelope = Mathf.Exp(-time * 30f); // Quick fade out
            
            data[i] = Mathf.Sin(2 * Mathf.PI * frequency * time) * envelope * 0.4f;
        }
        
        clip.SetData(data, 0);
        audioSource.clip = clip;
    }
    
    public void PlayClickSound()
    {
        audioSource.Play();
    }
}