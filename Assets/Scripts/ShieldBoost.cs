using UnityEngine;

public class ShieldBoost : MonoBehaviour
{
    [Header("Shield Settings")]
    public float shieldDuration = 8f;
    public float rotationSpeed = 90f;
    public float bobSpeed = 2f;
    public float bobAmount = 0.3f;
    
    [Header("Visual Effects")]
    public Color shieldColor = Color.cyan;
    public float glowIntensity = 2f;
    
    [Header("Audio")]
    public AudioClip pickupSound;
    
    private Renderer shieldRenderer;
    private Material shieldMaterial;
    private Light shieldLight;
    private float spawnTime;
    
    void Start()
    {
        spawnTime = Time.time;
        SetupShieldVisuals();
        SetupCollider();
        
        Debug.Log($"Shield power-up spawned - provides {shieldDuration}s of invincibility!");
    }
    
    void SetupShieldVisuals()
    {
        shieldRenderer = GetComponent<Renderer>();
        if (shieldRenderer != null)
        {
            shieldMaterial = new Material(shieldRenderer.sharedMaterial);
            shieldMaterial.EnableKeyword("_EMISSION");
            shieldMaterial.color = shieldColor;
            shieldMaterial.SetColor("_EmissionColor", shieldColor * glowIntensity);
            shieldRenderer.material = shieldMaterial;
        }
        
        shieldLight = gameObject.AddComponent<Light>();
        shieldLight.type = LightType.Point;
        shieldLight.color = shieldColor;
        shieldLight.intensity = 2f;
        shieldLight.range = 5f;
    }
    
    void SetupCollider()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.isTrigger = true;
            sphereCol.radius = 1f;
        }
        else
        {
            col.isTrigger = true;
        }
    }
    
    void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        
        float bobOffset = Mathf.Sin((Time.time - spawnTime) * bobSpeed) * bobAmount;
        Vector3 pos = transform.position;
        pos.y = transform.position.y + bobOffset * Time.deltaTime;
        
        if (shieldLight != null)
        {
            float pulse = (Mathf.Sin(Time.time * 3f) + 1f) * 0.5f;
            shieldLight.intensity = 1.5f + (pulse * 1f);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("🛡️ SHIELD POWER-UP COLLECTED!");
            
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                ApplyShieldToPlayer(playerController);
            }
            
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, 0.7f);
            }
            
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                gameManager.UpdateStatusMessage($"SHIELD ACTIVATED! Invulnerable for {shieldDuration}s!");
            }
            
            Destroy(gameObject);
        }
    }
    
    void ApplyShieldToPlayer(PlayerController playerController)
    {
        PlayerShield playerShield = playerController.GetComponent<PlayerShield>();
        if (playerShield == null)
        {
            playerShield = playerController.gameObject.AddComponent<PlayerShield>();
        }
        
        playerShield.ActivateShield(shieldDuration);
        Debug.Log($"Shield applied to player for {shieldDuration} seconds!");
    }
}