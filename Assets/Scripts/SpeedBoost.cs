using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    [Header("Asset Configuration")]
    public GameObject speedBoostAsset; // Drag your SpeedC... (lightning) prefab here
    public float assetScale = 1f;      // Scale adjustment for your asset
    public Vector3 assetRotationOffset = Vector3.zero; // Rotation adjustment if needed
    
    [Header("Speed Boost Settings")]
    public float speedMultiplier = 1.5f;     // 50% speed increase
    public float boostDuration = 8f;         // How long the boost lasts
    public float rotationSpeed = 90f;        // Visual rotation speed
    public float bobSpeed = 2f;              // Up/down floating animation
    public float bobAmount = 0.3f;           // How much it bobs
    
    [Header("Visual Effects")]
    public Color boostColor = Color.yellow;
    public float glowIntensity = 2f;
    public bool enableParticles = true;
    public bool enableGlow = true;
    
    [Header("Audio")]
    public AudioClip collectSound;
    
    private Vector3 startPosition;
    private GameObject assetInstance;
    private Renderer[] assetRenderers;
    private Material[] originalMaterials;
    private Material[] glowMaterials;
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
        
        Debug.Log($"Speed boost spawned using asset: {(speedBoostAsset != null ? speedBoostAsset.name : "None")}");
    }
    
    void SetupAsset()
    {
        if (speedBoostAsset != null)
        {
            // Instantiate the asset as a child
            assetInstance = Instantiate(speedBoostAsset, transform);
            assetInstance.name = "SpeedBoost_Asset";
            assetInstance.transform.localPosition = Vector3.zero;
            assetInstance.transform.localRotation = Quaternion.Euler(assetRotationOffset);
            assetInstance.transform.localScale = Vector3.one * assetScale;
            
            // Remove any colliders from the asset (we'll use our own)
            Collider[] assetColliders = assetInstance.GetComponentsInChildren<Collider>();
            foreach (Collider col in assetColliders)
            {
                Destroy(col);
            }
            
            // Get all renderers for glow effects
            assetRenderers = assetInstance.GetComponentsInChildren<Renderer>();
            SetupAssetMaterials();
        }
        else
        {
            Debug.LogWarning("No speed boost asset assigned! Using fallback.");
            CreateFallbackVisual();
        }
    }
    
    void SetupAssetMaterials()
    {
        if (assetRenderers != null && assetRenderers.Length > 0)
        {
            originalMaterials = new Material[assetRenderers.Length];
            glowMaterials = new Material[assetRenderers.Length];
            
            for (int i = 0; i < assetRenderers.Length; i++)
            {
                if (assetRenderers[i] != null)
                {
                    // Store original material
                    originalMaterials[i] = assetRenderers[i].material;
                    
                    // Create glow version if glow is enabled
                    if (enableGlow)
                    {
                        glowMaterials[i] = new Material(originalMaterials[i]);
                        glowMaterials[i].EnableKeyword("_EMISSION");
                        glowMaterials[i].SetColor("_EmissionColor", boostColor * glowIntensity);
                        
                        // Apply glow material
                        assetRenderers[i].material = glowMaterials[i];
                    }
                }
            }
        }
    }
    
    void CreateFallbackVisual()
    {
        // Fallback if no asset is assigned
        GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        fallback.name = "SpeedBoost_Fallback";
        fallback.transform.SetParent(transform);
        fallback.transform.localPosition = Vector3.zero;
        fallback.transform.localScale = new Vector3(0.8f, 0.3f, 0.8f);
        
        // Remove collider
        Collider fallbackCollider = fallback.GetComponent<Collider>();
        if (fallbackCollider != null) Destroy(fallbackCollider);
        
        // Apply material
        Renderer fallbackRenderer = fallback.GetComponent<Renderer>();
        Material fallbackMaterial = new Material(Shader.Find("Standard"));
        fallbackMaterial.color = boostColor;
        fallbackMaterial.EnableKeyword("_EMISSION");
        fallbackMaterial.SetColor("_EmissionColor", boostColor * glowIntensity);
        fallbackRenderer.material = fallbackMaterial;
        
        assetInstance = fallback;
        assetRenderers = new Renderer[] { fallbackRenderer };
    }
    
    void SetupVisualEffects()
    {
        if (enableParticles)
        {
            SetupParticleEffect();
        }
    }
    
    void SetupParticleEffect()
    {
        particles = gameObject.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startColor = boostColor;
        main.startSpeed = 2f;
        main.startSize = 0.15f;
        main.maxParticles = 25;
        main.startLifetime = 2f;
        
        var emission = particles.emission;
        emission.rateOverTime = 12f;
        
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.8f;
        
        var velocityOverLifetime = particles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.y = 4f;
        
        // Sparkling effect for speed boost
        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.5f);
        sizeCurve.AddKey(0.5f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Fade out over lifetime
        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(boostColor, 0.0f), new GradientColorKey(boostColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = gradient;
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
            AnimateBoost();
        }
    }
    
    void AnimateBoost()
    {
        // Rotation animation
        if (assetInstance != null)
        {
            assetInstance.transform.Rotate(assetRotationOffset.x * Time.deltaTime, 
                                         rotationSpeed * Time.deltaTime, 
                                         assetRotationOffset.z * Time.deltaTime);
        }
        
        // Bob up and down animation
        float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = startPosition + new Vector3(0, bobOffset, 0);
        
        // Pulsing glow effect
        if (enableGlow && glowMaterials != null)
        {
            float pulse = (Mathf.Sin(Time.time * 3f) + 1f) * 0.5f;
            float currentIntensity = glowIntensity * (0.7f + pulse * 0.6f);
            
            for (int i = 0; i < glowMaterials.Length; i++)
            {
                if (glowMaterials[i] != null)
                {
                    glowMaterials[i].SetColor("_EmissionColor", boostColor * currentIntensity);
                }
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            CollectSpeedBoost(other.gameObject);
        }
    }
    
    void CollectSpeedBoost(GameObject player)
    {
        isCollected = true;
        
        Debug.Log("Speed boost collected by player!");
        
        // Play collection sound
        if (audioSource != null && collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
        }
        
        // Apply speed boost to player
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.ApplySpeedBoost(speedMultiplier, boostDuration);
        }
        else
        {
            Debug.LogWarning("Player doesn't have PlayerController component!");
        }
        
        // Update UI
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            int percentIncrease = Mathf.RoundToInt((speedMultiplier - 1) * 100);
            gameManager.UpdateStatusMessage($"LIGHTNING SPEED! +{percentIncrease}% speed for {boostDuration}s");
        }
        
        // Collection effect
        StartCoroutine(CollectionEffect());
    }
    
    System.Collections.IEnumerator CollectionEffect()
    {
        // Stop particles from emitting new ones
        if (particles != null)
        {
            var emission = particles.emission;
            emission.enabled = false;
        }
        
        // Dramatic collection effect
        Vector3 originalScale = transform.localScale;
        float timer = 0f;
        float effectDuration = 0.6f;
        
        while (timer < effectDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / effectDuration;
            
            // Scale up and spin faster
            transform.localScale = originalScale * (1f + progress * 1.5f);
            
            if (assetInstance != null)
            {
                assetInstance.transform.Rotate(0, rotationSpeed * 3f * Time.deltaTime, 0);
            }
            
            // Fade out materials
            if (glowMaterials != null)
            {
                for (int i = 0; i < glowMaterials.Length; i++)
                {
                    if (glowMaterials[i] != null)
                    {
                        Color currentColor = glowMaterials[i].color;
                        currentColor.a = 1f - progress;
                        glowMaterials[i].color = currentColor;
                        
                        // Intensify glow as it fades
                        float finalGlow = glowIntensity * (2f + progress * 3f);
                        glowMaterials[i].SetColor("_EmissionColor", boostColor * finalGlow);
                    }
                }
            }
            
            yield return null;
        }
        
        // Wait a bit for sound to finish
        yield return new WaitForSeconds(0.3f);
        
        // Destroy the speed boost
        Destroy(gameObject);
    }
    
    void OnDestroy()
    {
        // Clean up materials
        if (glowMaterials != null)
        {
            for (int i = 0; i < glowMaterials.Length; i++)
            {
                if (glowMaterials[i] != null)
                {
                    Destroy(glowMaterials[i]);
                }
            }
        }
    }
}