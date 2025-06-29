using UnityEngine;

public class PortalBeacon : MonoBehaviour
{
    [Header("Beacon Settings")]
    public float beamHeight = 20f;
    public float beamRadius = 1f; // Changed from width to radius
    public Color beamColor = Color.cyan;
    public float pulseSpeed = 2f;
    public bool isActive = false;
    
    [Header("Audio")]
    public AudioClip activationSound;
    public AudioClip ambientHum;
    
    private GameObject beamCylinder; // 3D beam instead of LineRenderer
    private AudioSource audioSource;
    private Light beaconLight;
    private ParticleSystem particles;
    
    void Start()
    {
        SetupBeacon();
        SetBeaconState(false);
    }
    
    void SetupBeacon()
    {
        // Create 3D cylindrical beam
        Create3DBeam();
        
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
    
    void Create3DBeam()
    {
        // Create cylinder for 3D beam
        beamCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
    beamCylinder.name = "PortalBeam";
    beamCylinder.transform.SetParent(transform);
    
    // Smaller beam radius
    float smallerRadius = beamRadius * 0.6f; // 40% smaller
    beamCylinder.transform.localPosition = new Vector3(0, beamHeight / 2f, 0);
    beamCylinder.transform.localScale = new Vector3(smallerRadius * 2f, beamHeight / 2f, smallerRadius * 2f);
    
    Destroy(beamCylinder.GetComponent<Collider>());
    
    // Force cyan color
    Renderer beamRenderer = beamCylinder.GetComponent<Renderer>();
    Material beamMat = new Material(Shader.Find("Standard"));
    beamMat.color = Color.cyan;
    beamMat.EnableKeyword("_EMISSION");
    beamMat.SetColor("_EmissionColor", Color.cyan * 2f);
        beamMat.SetFloat("_Metallic", 0f);
        beamMat.SetFloat("_Smoothness", 1f);
        
        // Make it transparent
        beamMat.SetFloat("_Mode", 3f); // Transparent mode
        beamMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        beamMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        beamMat.SetInt("_ZWrite", 0);
        beamMat.DisableKeyword("_ALPHATEST_ON");
        beamMat.EnableKeyword("_ALPHABLEND_ON");
        beamMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        beamMat.renderQueue = 3000;
        
        Color finalColor = beamColor;
        finalColor.a = 0.4f; // Semi-transparent
        beamMat.color = finalColor;
        
        beamRenderer.material = beamMat;
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
        shape.radius = beamRadius;
        
        var velocityOverLifetime = particles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.y = 10f;
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
        if (beamCylinder != null)
        {
            // Pulse the beam intensity
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            Renderer beamRenderer = beamCylinder.GetComponent<Renderer>();
            
            Color pulseColor = beamColor;
            pulseColor.a = 0.2f + (pulse * 0.3f);
            beamRenderer.material.color = pulseColor;
            beamRenderer.material.SetColor("_EmissionColor", beamColor * (1f + pulse));
        }
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
        
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }
        
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
        if (beamCylinder != null)
            beamCylinder.SetActive(active);
            
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
    
    public void OnAllFragmentsCollected()
    {
        ActivateBeacon();
    }
}