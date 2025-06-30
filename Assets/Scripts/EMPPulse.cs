using UnityEngine;

public class EMPPulse : MonoBehaviour
{
    [Header("EMP Settings")]
    public float empDuration = 6f;         // How long viruses are disabled
    public float empRadius = 15f;          // Range of EMP effect
    public float rotationSpeed = 120f;     // Visual rotation speed
    public float bobSpeed = 3f;            // Up/down floating animation
    public float bobAmount = 0.4f;         // How much it bobs
    
    [Header("Visual Effects")]
    public Color empColor = Color.cyan;    // Electric blue
    public float glowIntensity = 3f;
    public bool enableParticles = true;
    
    [Header("Audio")]
    public AudioClip collectSound;
    public AudioClip empActivationSound;
    
    private Vector3 startPosition;
    private GameObject assetInstance;
    private Renderer[] assetRenderers;
    private ParticleSystem particles;
    private AudioSource audioSource;
    private bool isCollected = false;
    
    void Start()
    {
        startPosition = transform.position;
        SetupAsset();
        SetupAudio();
        SetupCollider();
        SetupVisualEffects();
        
        Debug.Log("EMP Pulse power-up spawned and ready!");
    }
    
    void SetupAsset()
    {
        // Look for Thunder asset or create fallback
        if (transform.childCount > 0)
        {
            assetInstance = transform.GetChild(0).gameObject;
        }
        else
        {
            // Create fallback visual if no asset assigned
            assetInstance = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            assetInstance.name = "EMP_Fallback";
            assetInstance.transform.SetParent(transform);
            assetInstance.transform.localPosition = Vector3.zero;
            assetInstance.transform.localScale = new Vector3(0.8f, 0.4f, 0.8f);
            
            // Remove collider from visual
            Collider visualCollider = assetInstance.GetComponent<Collider>();
            if (visualCollider != null) Destroy(visualCollider);
        }
        
        SetupAssetMaterials();
    }
    
    void SetupAssetMaterials()
    {
        assetRenderers = assetInstance.GetComponentsInChildren<Renderer>();
        
        for (int i = 0; i < assetRenderers.Length; i++)
        {
            if (assetRenderers[i] != null)
            {
                Material empMaterial = new Material(Shader.Find("Standard"));
                empMaterial.color = empColor;
                empMaterial.EnableKeyword("_EMISSION");
                empMaterial.SetColor("_EmissionColor", empColor * glowIntensity);
                empMaterial.SetFloat("_Metallic", 0.1f);
                empMaterial.SetFloat("_Smoothness", 0.9f);
                
                assetRenderers[i].material = empMaterial;
            }
        }
    }
    
    void SetupVisualEffects()
    {
        if (enableParticles)
        {
            particles = gameObject.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = empColor;
            main.startSpeed = 3f;
            main.startSize = 0.1f;
            main.maxParticles = 30;
            main.startLifetime = 1.5f;
            
            var emission = particles.emission;
            emission.rateOverTime = 20f;
            
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.5f;
            
            // Electric sparks effect
            var velocityOverLifetime = particles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            velocityOverLifetime.radial = 2f; // Sparks fly outward
            
            var sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 0.2f);
            sizeCurve.AddKey(0.5f, 1f);
            sizeCurve.AddKey(1f, 0f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        }
    }
    
    void SetupAudio()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.volume = 0.7f;
        audioSource.spatialBlend = 1f; // 3D audio
    }
    
    void SetupCollider()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.isTrigger = true;
            sphereCol.radius = 1.5f; // Generous collection area
        }
        else
        {
            col.isTrigger = true;
        }
    }
    
    void Update()
    {
        if (!isCollected)
        {
            AnimateEMP();
        }
    }
    
    void AnimateEMP()
    {
        // Rotation animation - faster than speed boost for electric effect
        if (assetInstance != null)
        {
            assetInstance.transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
        
        // Bob up and down animation
        float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = startPosition + new Vector3(0, bobOffset, 0);
        
        // Electric pulsing glow effect
        if (assetRenderers != null)
        {
            float pulse = (Mathf.Sin(Time.time * 6f) + 1f) * 0.5f; // Faster pulse for electric effect
            float currentIntensity = glowIntensity * (0.8f + pulse * 0.4f);
            
            foreach (Renderer renderer in assetRenderers)
            {
                if (renderer != null && renderer.material != null)
                {
                    renderer.material.SetColor("_EmissionColor", empColor * currentIntensity);
                }
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            CollectEMPPulse(other.gameObject);
        }
    }
    
    void CollectEMPPulse(GameObject player)
    {
        isCollected = true;
        
        Debug.Log("EMP Pulse collected by player!");
        
        // Play collection sound
        if (audioSource != null && collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
        }
        
        // Activate EMP effect
        ActivateEMPEffect();
        
        // Update UI
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.UpdateStatusMessage($"EMP PULSE ACTIVATED! Viruses disabled for {empDuration}s!");
        }
        
        // Collection effect
        StartCoroutine(CollectionEffect());
    }
    
    void ActivateEMPEffect()
    {
        Debug.Log($"EMP Pulse activated! Disabling all viruses within {empRadius} units for {empDuration} seconds");
        
        // Play EMP activation sound
        if (audioSource != null && empActivationSound != null)
        {
            audioSource.PlayOneShot(empActivationSound);
        }
        
        // Find all viruses in range and disable them
        VirusAI[] flyingViruses = FindObjectsOfType<VirusAI>();
        GroundPatrolVirus[] groundViruses = FindObjectsOfType<GroundPatrolVirus>();
        
        int disabledCount = 0;
        
        // Disable flying viruses
        foreach (VirusAI virus in flyingViruses)
        {
            if (virus != null)
            {
                float distance = Vector3.Distance(transform.position, virus.transform.position);
                if (distance <= empRadius)
                {
                    StartCoroutine(DisableVirusTemporarily(virus.gameObject, empDuration));
                    disabledCount++;
                }
            }
        }
        
        // Disable ground viruses
        foreach (GroundPatrolVirus virus in groundViruses)
        {
            if (virus != null)
            {
                float distance = Vector3.Distance(transform.position, virus.transform.position);
                if (distance <= empRadius)
                {
                    StartCoroutine(DisableVirusTemporarily(virus.gameObject, empDuration));
                    disabledCount++;
                }
            }
        }
        
        Debug.Log($"EMP disabled {disabledCount} viruses!");
        
        // Create EMP blast effect
        CreateEMPBlastEffect();
    }
    
    System.Collections.IEnumerator DisableVirusTemporarily(GameObject virus, float duration)
    {
        // Disable virus AI
        VirusAI flyingAI = virus.GetComponent<VirusAI>();
        GroundPatrolVirus groundAI = virus.GetComponent<GroundPatrolVirus>();
        
        bool wasEnabled = false;
        
        if (flyingAI != null)
        {
            wasEnabled = flyingAI.enabled;
            flyingAI.enabled = false;
        }
        
        if (groundAI != null)
        {
            wasEnabled = groundAI.enabled;
            groundAI.enabled = false;
        }
        
        // Add visual effect to show virus is disabled
        StartCoroutine(EMPDisabledVisualEffect(virus, duration));
        
        Debug.Log($"{virus.name} disabled by EMP for {duration} seconds");
        
        // Wait for duration
        yield return new WaitForSeconds(duration);
        
        // Re-enable virus if it still exists and was originally enabled
        if (virus != null && wasEnabled)
        {
            if (flyingAI != null)
            {
                flyingAI.enabled = true;
            }
            
            if (groundAI != null)
            {
                groundAI.enabled = true;
            }
            
            Debug.Log($"{virus.name} recovered from EMP");
        }
    }
    
    System.Collections.IEnumerator EMPDisabledVisualEffect(GameObject virus, float duration)
    {
        // Add blue sparks and flickering to disabled virus
        GameObject empEffect = new GameObject("EMP_DisabledEffect");
        empEffect.transform.SetParent(virus.transform);
        empEffect.transform.localPosition = Vector3.zero;
        
        // Add sparking particle effect
        ParticleSystem empParticles = empEffect.AddComponent<ParticleSystem>();
        var main = empParticles.main;
        main.startColor = Color.cyan;
        main.startSpeed = 1f;
        main.startSize = 0.05f;
        main.maxParticles = 15;
        main.startLifetime = 0.5f;
        
        var emission = empParticles.emission;
        emission.rateOverTime = 30f;
        
        // Make virus flicker
        Renderer[] virusRenderers = virus.GetComponentsInChildren<Renderer>();
        Material[] originalMaterials = new Material[virusRenderers.Length];
        
        for (int i = 0; i < virusRenderers.Length; i++)
        {
            if (virusRenderers[i] != null)
            {
                originalMaterials[i] = virusRenderers[i].material;
            }
        }
        
        float flickerTimer = 0f;
        while (flickerTimer < duration)
        {
            flickerTimer += Time.deltaTime;
            
            // Flicker effect
            bool showOriginal = (Mathf.Sin(flickerTimer * 20f) > 0f);
            
            for (int i = 0; i < virusRenderers.Length; i++)
            {
                if (virusRenderers[i] != null)
                {
                    if (showOriginal)
                    {
                        virusRenderers[i].material = originalMaterials[i];
                    }
                    else
                    {
                        // Create disabled material (darker/blue tinted)
                        Material disabledMat = new Material(originalMaterials[i]);
                        disabledMat.color = Color.cyan * 0.5f;
                        virusRenderers[i].material = disabledMat;
                    }
                }
            }
            
            yield return null;
        }
        
        // Restore original materials
        for (int i = 0; i < virusRenderers.Length; i++)
        {
            if (virusRenderers[i] != null)
            {
                virusRenderers[i].material = originalMaterials[i];
            }
        }
        
        // Clean up effect
        Destroy(empEffect);
    }
    
    void CreateEMPBlastEffect()
    {
        GameObject blastEffect = new GameObject("EMP_BlastEffect");
        blastEffect.transform.position = transform.position;
        
        // Create expanding ring effect
        ParticleSystem blastParticles = blastEffect.AddComponent<ParticleSystem>();
        var main = blastParticles.main;
        main.startColor = empColor;
        main.startSpeed = 10f;
        main.startSize = 0.2f;
        main.maxParticles = 100;
        main.startLifetime = 2f;
        
        var emission = blastParticles.emission;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 100)
        });
        
        var shape = blastParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;
        
        var velocityOverLifetime = blastParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = 15f;
        
        // Bright EMP light
        Light empLight = blastEffect.AddComponent<Light>();
        empLight.type = LightType.Point;
        empLight.color = empColor;
        empLight.intensity = 10f;
        empLight.range = empRadius;
        
        // Clean up after effect
        Destroy(blastEffect, 3f);
    }
    
    System.Collections.IEnumerator CollectionEffect()
    {
        // Stop particles from emitting new ones
        if (particles != null)
        {
            var emission = particles.emission;
            emission.enabled = false;
        }
        
        // Scaling up effect with electric crackle
        Vector3 originalScale = transform.localScale;
        float timer = 0f;
        float effectDuration = 0.8f;
        
        while (timer < effectDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / effectDuration;
            
            // Scale up and fade out
            transform.localScale = originalScale * (1f + progress * 2f);
            
            if (assetRenderers != null)
            {
                foreach (Renderer renderer in assetRenderers)
                {
                    if (renderer != null && renderer.material != null)
                    {
                        Color currentColor = renderer.material.color;
                        currentColor.a = 1f - progress;
                        renderer.material.color = currentColor;
                        
                        // Intensify glow as it disappears
                        float finalGlow = glowIntensity * (2f + progress * 3f);
                        renderer.material.SetColor("_EmissionColor", empColor * finalGlow);
                    }
                }
            }
            
            yield return null;
        }
        
        // Wait a bit for sound to finish
        yield return new WaitForSeconds(0.3f);
        
        // Destroy the EMP pulse
        Destroy(gameObject);
    }
}