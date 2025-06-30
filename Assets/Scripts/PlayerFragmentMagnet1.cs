using UnityEngine;

public class PlayerFragmentMagnet : MonoBehaviour
{
    [Header("Magnet Settings")]
    public bool isMagnetActive = false;
    public float magnetTimeRemaining = 0f;
    public float magnetRange = 8f;
    public float attractionSpeed = 5f;
    
    [Header("Visual Effects")]
    public Color magnetColor = Color.magenta;
    public float effectIntensity = 1f;
    
    private Renderer playerRenderer;
    private Material originalMaterial;
    private Material magnetMaterial;
    private GameObject magnetEffect;
    private Light magnetLight;
    private PlayerController playerController;
    
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        playerRenderer = GetComponentInChildren<Renderer>();
        
        if (playerRenderer != null)
        {
            originalMaterial = playerRenderer.material;
        }
        
        Debug.Log("Player Fragment Magnet system initialized");
    }
    
    void Update()
    {
        if (isMagnetActive && magnetTimeRemaining > 0)
        {
            magnetTimeRemaining -= Time.deltaTime;
            
            // Attract nearby fragments
            AttractNearbyFragments();
            
            // Update magnet visual effects
            UpdateMagnetEffects();
            
            if (magnetTimeRemaining <= 0)
            {
                DeactivateMagnet();
            }
        }
    }
    
    public void ActivateMagnet(float duration)
    {
        isMagnetActive = true;
        magnetTimeRemaining = duration;
        
        Debug.Log($"🧲 Fragment magnet activated for {duration} seconds!");
        
        CreateMagnetVisuals();
        
        // Update status
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.UpdateStatusMessage($"MAGNET ACTIVE! Attracting fragments for {duration:F0}s");
        }
    }
    
    void CreateMagnetVisuals()
    {
        // Create glowing player material
        if (playerRenderer != null && originalMaterial != null)
        {
            magnetMaterial = new Material(originalMaterial);
            magnetMaterial.EnableKeyword("_EMISSION");
            magnetMaterial.SetColor("_EmissionColor", magnetColor * effectIntensity);
            playerRenderer.material = magnetMaterial;
        }
        
        // Create magnet effect ring around player
        magnetEffect = new GameObject("MagnetEffect");
        magnetEffect.transform.SetParent(transform);
        magnetEffect.transform.localPosition = Vector3.zero;
        
        // Create a torus-like effect (using a scaled ring)
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.transform.SetParent(magnetEffect.transform);
        ring.transform.localPosition = Vector3.zero;
        ring.transform.localScale = new Vector3(magnetRange * 0.8f, 0.1f, magnetRange * 0.8f);
        
        // Make ring material
        Renderer ringRenderer = ring.GetComponent<Renderer>();
        Material ringMaterial = new Material(Shader.Find("Standard"));
        ringMaterial.SetFloat("_Mode", 3); // Transparent mode
        ringMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        ringMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        ringMaterial.SetInt("_ZWrite", 0);
        ringMaterial.DisableKeyword("_ALPHATEST_ON");
        ringMaterial.EnableKeyword("_ALPHABLEND_ON");
        ringMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        ringMaterial.renderQueue = 3000;
        
        ringMaterial.color = new Color(magnetColor.r, magnetColor.g, magnetColor.b, 0.4f);
        ringMaterial.EnableKeyword("_EMISSION");
        ringMaterial.SetColor("_EmissionColor", magnetColor * 1.5f);
        ringRenderer.material = ringMaterial;
        
        // Remove collider from ring
        Collider ringCollider = ring.GetComponent<Collider>();
        if (ringCollider != null)
        {
            Destroy(ringCollider);
        }
        
        // Add magnet light
        magnetLight = magnetEffect.AddComponent<Light>();
        magnetLight.type = LightType.Point;
        magnetLight.color = magnetColor;
        magnetLight.intensity = 2f;
        magnetLight.range = magnetRange;
        
        Debug.Log("Magnet visual effects created");
    }
    
    void AttractNearbyFragments()
    {
        // Find all memory fragments in range
        MemoryFragment[] allFragments = FindObjectsOfType<MemoryFragment>();
        
        foreach (MemoryFragment fragment in allFragments)
        {
            if (fragment != null)
            {
                float distance = Vector3.Distance(transform.position, fragment.transform.position);
                
                if (distance <= magnetRange && distance > 0.5f) // Don't attract if too close
                {
                    // Calculate attraction force
                    Vector3 direction = (transform.position - fragment.transform.position).normalized;
                    float attractionForce = (magnetRange - distance) / magnetRange; // Stronger when closer
                    
                    // Move fragment towards player
                    Vector3 attractionVector = direction * attractionSpeed * attractionForce * Time.deltaTime;
                    fragment.transform.position += attractionVector;
                    
                    // Add some visual effect to attracted fragments
                    AddFragmentMagnetEffect(fragment);
                    
                    Debug.Log($"🧲 Attracting fragment from {distance:F1} units away");
                }
            }
        }
    }
    
    void AddFragmentMagnetEffect(MemoryFragment fragment)
    {
        // Make fragment glow magenta briefly
        Renderer fragmentRenderer = fragment.GetComponent<Renderer>();
        if (fragmentRenderer != null)
        {
            // Add a subtle magnet glow
            Material fragmentMaterial = fragmentRenderer.material;
            if (fragmentMaterial.HasProperty("_EmissionColor"))
            {
                Color currentEmission = fragmentMaterial.GetColor("_EmissionColor");
                Color magnetGlow = Color.Lerp(currentEmission, magnetColor, 0.3f);
                fragmentMaterial.SetColor("_EmissionColor", magnetGlow);
            }
        }
    }
    
    void UpdateMagnetEffects()
    {
        if (magnetEffect != null)
        {
            // Rotate magnet ring
            magnetEffect.transform.Rotate(0, 90f * Time.deltaTime, 0);
            
            // Pulse magnet effect
            float pulse = (Mathf.Sin(Time.time * 3f) + 1f) * 0.5f;
            magnetEffect.transform.localScale = Vector3.one * (0.9f + pulse * 0.2f);
            
            // Update light intensity
            if (magnetLight != null)
            {
                magnetLight.intensity = 1.5f + pulse * 1.5f;
            }
            
            // Warning flash when magnet is about to expire
            if (magnetTimeRemaining <= 2f)
            {
                float warningFlash = Mathf.Sin(Time.time * 6f);
                if (magnetMaterial != null)
                {
                    Color warningColor = Color.Lerp(magnetColor, Color.yellow, warningFlash * 0.5f + 0.5f);
                    magnetMaterial.SetColor("_EmissionColor", warningColor * effectIntensity);
                }
            }
        }
    }
    
    void DeactivateMagnet()
    {
        isMagnetActive = false;
        magnetTimeRemaining = 0f;
        
        Debug.Log("🧲 Fragment magnet deactivated!");
        
        // Restore original player material
        if (playerRenderer != null && originalMaterial != null)
        {
            playerRenderer.material = originalMaterial;
        }
        
        // Remove magnet effects
        if (magnetEffect != null)
        {
            Destroy(magnetEffect);
        }
        
        if (magnetMaterial != null)
        {
            Destroy(magnetMaterial);
            magnetMaterial = null;
        }
        
        // Update status
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.UpdateStatusMessage("Fragment magnet expired!");
        }
    }
    
    public bool IsMagnetActive()
    {
        return isMagnetActive && magnetTimeRemaining > 0;
    }
    
    public float GetMagnetTimeRemaining()
    {
        return magnetTimeRemaining;
    }
    
    void OnGUI()
    {
        if (isMagnetActive && magnetTimeRemaining > 0)
        {
            GUI.color = magnetColor;
            GUI.Label(new Rect(10, 70, 200, 20), $"🧲 MAGNET: {magnetTimeRemaining:F1}s");
            GUI.color = Color.white;
        }
    }
    
    void OnDrawGizmos()
    {
        if (isMagnetActive)
        {
            // Show magnet range
            Gizmos.color = magnetColor;
            Gizmos.DrawWireSphere(transform.position, magnetRange);
        }
    }
}