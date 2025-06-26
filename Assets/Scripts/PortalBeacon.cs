using UnityEngine;

public class PortalBeacon : MonoBehaviour
{
    [Header("Beacon Settings")]
    public float beamHeight = 20f;
    public float beamWidth = 2f;
    public Color beamColor = Color.cyan;
    public float pulseSpeed = 2f;
    public bool isActive = false;
    
    [Header("Audio")]
    public AudioClip activationSound;
    public AudioClip ambientHum;
    
    private LineRenderer beamRenderer;
    private AudioSource audioSource;
    private Light beaconLight;
    private ParticleSystem particles;
    
    void Start()
    {
        SetupBeacon();
        SetBeaconState(false); // Start inactive
    }
    
    void SetupBeacon()
    {
        // Create line renderer for beam
        beamRenderer = gameObject.AddComponent<LineRenderer>();
        beamRenderer.material = CreateBeamMaterial();
        beamRenderer.startWidth = beamWidth;
        beamRenderer.endWidth = beamWidth * 0.5f;
        beamRenderer.positionCount = 2;
        beamRenderer.useWorldSpace = true;
        
        // Setup light component
        beaconLight = gameObject.AddComponent<Light>();
        beaconLight.type = LightType.Point;
        beaconLight.color = beamColor;
        beaconLight.intensity = 3f;
        beaconLight.range = 15f;
        
        // Setup audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = 0.3f;
        audioSource.loop = true;
        
        // Setup particles
        SetupParticles();
    }
    
    void SetupParticles()
    {
        particles = gameObject.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startColor = beamColor;
        main.startSpeed = 5f;
        main.startSize = 0.1f;
        main.maxParticles = 50;
        
        var emission = particles.emission;
        emission.rateOverTime = 20f;
        
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 1f;
        
        var velocityOverLifetime = particles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.y = 10f; // Upward movement
    }
    
    Material CreateBeamMaterial()
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = beamColor;
        mat.SetInt("_ZWrite", 0);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        return mat;
    }
    
    void Update()
    {
        if (isActive)
        {
            UpdateBeamEffect();
            UpdateLightPulse();
        }
    }
    
    void UpdateBeamEffect()
    {
        // Set beam positions
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * beamHeight;
        
        beamRenderer.SetPosition(0, startPos);
        beamRenderer.SetPosition(1, endPos);
        
        // Pulse effect
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        Color currentColor = beamColor;
        currentColor.a = 0.3f + (pulse * 0.4f);
        beamRenderer.material.color = currentColor;
    }
    
    void UpdateLightPulse()
    {
        float pulse = (Mathf.Sin(Time.time * pulseSpeed * 1.5f) + 1f) * 0.5f;
        beaconLight.intensity = 2f + (pulse * 2f);
    }
    
    public void ActivateBeacon()
    {
        if (isActive) return;
        
        isActive = true;
        SetBeaconState(true);
        
        // Play activation sound
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }
        
        // Start ambient hum
        if (ambientHum != null)
        {
            audioSource.clip = ambientHum;
            audioSource.Play();
        }
        
        Debug.Log("Portal beacon activated!");
    }
    
    public void DeactivateBeacon()
    {
        isActive = false;
        SetBeaconState(false);
        
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
    void SetBeaconState(bool active)
    {
        if (beamRenderer != null)
            beamRenderer.enabled = active;
            
        if (beaconLight != null)
            beaconLight.enabled = active;
            
        if (particles != null)
        {
            if (active)
                particles.Play();
            else
                particles.Stop();
        }
    }
    
    // Call this from GameManager when all fragments collected
    public void OnAllFragmentsCollected()
    {
        ActivateBeacon();
    }
}