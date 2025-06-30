using UnityEngine;

public class PlayerShield : MonoBehaviour
{
    [Header("Shield Settings")]
    public bool isShielded = false;
    public float shieldTimeRemaining = 0f;
    
    [Header("Visual Effects")]
    public Color shieldColor = Color.cyan;
    public float shieldIntensity = 1.5f;
    
    private Renderer playerRenderer;
    private Material originalMaterial;
    private Material shieldMaterial;
    private GameObject shieldEffect;
    private Light shieldLight;
    private PlayerController playerController;
    
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        playerRenderer = GetComponentInChildren<Renderer>();
        
        if (playerRenderer != null)
        {
            originalMaterial = playerRenderer.material;
        }
        
        Debug.Log("Player Shield system initialized");
    }
    
    void Update()
    {
        if (isShielded && shieldTimeRemaining > 0)
        {
            shieldTimeRemaining -= Time.deltaTime;
            
            // Update shield visual effects
            UpdateShieldEffects();
            
            if (shieldTimeRemaining <= 0)
            {
                DeactivateShield();
            }
        }
    }
    
    public void ActivateShield(float duration)
    {
        isShielded = true;
        shieldTimeRemaining = duration;
        
        Debug.Log($"🛡️ Shield activated for {duration} seconds!");
        
        CreateShieldVisuals();
        
        // Make player immune to virus collisions
        SetVirusImmunity(true);
        
        // Update status
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.UpdateStatusMessage($"SHIELD ACTIVE! {duration:F0}s remaining");
        }
    }
    
    void CreateShieldVisuals()
    {
        // Create glowing player material
        if (playerRenderer != null && originalMaterial != null)
        {
            shieldMaterial = new Material(originalMaterial);
            shieldMaterial.EnableKeyword("_EMISSION");
            shieldMaterial.SetColor("_EmissionColor", shieldColor * shieldIntensity);
            playerRenderer.material = shieldMaterial;
        }
        
        // Create shield effect sphere around player
        shieldEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shieldEffect.name = "ShieldEffect";
        shieldEffect.transform.SetParent(transform);
        shieldEffect.transform.localPosition = Vector3.zero;
        shieldEffect.transform.localScale = Vector3.one * 2.5f;
        
        // Make shield effect transparent and glowing
        Renderer effectRenderer = shieldEffect.GetComponent<Renderer>();
        Material effectMaterial = new Material(Shader.Find("Standard"));
        effectMaterial.SetFloat("_Mode", 3); // Transparent mode
        effectMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        effectMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        effectMaterial.SetInt("_ZWrite", 0);
        effectMaterial.DisableKeyword("_ALPHATEST_ON");
        effectMaterial.EnableKeyword("_ALPHABLEND_ON");
        effectMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        effectMaterial.renderQueue = 3000;
        
        effectMaterial.color = new Color(shieldColor.r, shieldColor.g, shieldColor.b, 0.3f);
        effectMaterial.EnableKeyword("_EMISSION");
        effectMaterial.SetColor("_EmissionColor", shieldColor * 0.8f);
        effectRenderer.material = effectMaterial;
        
        // Remove collider from effect
        Collider effectCollider = shieldEffect.GetComponent<Collider>();
        if (effectCollider != null)
        {
            Destroy(effectCollider);
        }
        
        // Add shield light
        shieldLight = shieldEffect.AddComponent<Light>();
        shieldLight.type = LightType.Point;
        shieldLight.color = shieldColor;
        shieldLight.intensity = 3f;
        shieldLight.range = 6f;
        
        Debug.Log("Shield visual effects created");
    }
    
    void UpdateShieldEffects()
    {
        if (shieldEffect != null)
        {
            // Pulse shield effect
            float pulse = (Mathf.Sin(Time.time * 5f) + 1f) * 0.5f;
            shieldEffect.transform.localScale = Vector3.one * (2.3f + pulse * 0.4f);
            
            // Update light intensity
            if (shieldLight != null)
            {
                shieldLight.intensity = 2f + pulse * 2f;
            }
            
            // Warning flash when shield is about to expire
            if (shieldTimeRemaining <= 2f)
            {
                float warningFlash = Mathf.Sin(Time.time * 8f);
                if (shieldMaterial != null)
                {
                    Color warningColor = Color.Lerp(shieldColor, Color.red, warningFlash * 0.5f + 0.5f);
                    shieldMaterial.SetColor("_EmissionColor", warningColor * shieldIntensity);
                }
            }
        }
    }
    
    void DeactivateShield()
    {
        isShielded = false;
        shieldTimeRemaining = 0f;
        
        Debug.Log("🛡️ Shield deactivated!");
        
        // Restore original player material
        if (playerRenderer != null && originalMaterial != null)
        {
            playerRenderer.material = originalMaterial;
        }
        
        // Remove shield effects
        if (shieldEffect != null)
        {
            Destroy(shieldEffect);
        }
        
        if (shieldMaterial != null)
        {
            Destroy(shieldMaterial);
            shieldMaterial = null;
        }
        
        // Remove virus immunity
        SetVirusImmunity(false);
        
        // Update status
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.UpdateStatusMessage("Shield expired!");
        }
    }
    
    void SetVirusImmunity(bool immune)
    {
        // Find all viruses and set their ability to catch this player
        VirusAI[] viruses = FindObjectsOfType<VirusAI>();
        foreach (VirusAI virus in viruses)
        {
            // We'll add a method to VirusAI to check if player is shielded
            // For now, we can use tags or layers
        }
        
        // Alternative: Change player's layer temporarily
        if (immune)
        {
            gameObject.layer = LayerMask.NameToLayer("Default"); // Or create "ShieldedPlayer" layer
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
        }
        
        Debug.Log($"Player virus immunity: {immune}");
    }
    
    public bool IsShielded()
    {
        return isShielded && shieldTimeRemaining > 0;
    }
    
    public float GetShieldTimeRemaining()
    {
        return shieldTimeRemaining;
    }
    
    // Called by viruses to check if they can catch the player
    public bool CanBeCaughtByVirus()
    {
        return !IsShielded();
    }
    
    void OnGUI()
    {
        if (isShielded && shieldTimeRemaining > 0)
        {
            GUI.color = shieldColor;
            GUI.Label(new Rect(10, 50, 200, 20), $"🛡️ SHIELD: {shieldTimeRemaining:F1}s");
            GUI.color = Color.white;
        }
    }
}