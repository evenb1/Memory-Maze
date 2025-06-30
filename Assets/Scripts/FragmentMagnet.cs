using UnityEngine;

public class FragmentMagnet : MonoBehaviour
{
    [Header("Magnet Settings")]
    public float magnetDuration = 10f;
    public float rotationSpeed = 180f;
    public float bobSpeed = 3f;
    public float bobAmount = 0.2f;
    
    [Header("Visual Effects")]
    public Color magnetColor = Color.magenta;
    public float glowIntensity = 2f;
    
    [Header("Audio")]
    public AudioClip pickupSound;
    
    private Renderer magnetRenderer;
    private Material magnetMaterial;
    private Light magnetLight;
    private float spawnTime;
    
    void Start()
    {
        spawnTime = Time.time;
        SetupMagnetVisuals();
        SetupCollider();
        
        Debug.Log($"Fragment Magnet spawned - attracts fragments for {magnetDuration}s!");
    }
    
    void SetupMagnetVisuals()
    {
        magnetRenderer = GetComponent<Renderer>();
        if (magnetRenderer != null)
        {
            magnetMaterial = new Material(magnetRenderer.sharedMaterial);
            magnetMaterial.EnableKeyword("_EMISSION");
            magnetMaterial.color = magnetColor;
            magnetMaterial.SetColor("_EmissionColor", magnetColor * glowIntensity);
            magnetRenderer.material = magnetMaterial;
        }
        
        magnetLight = gameObject.AddComponent<Light>();
        magnetLight.type = LightType.Point;
        magnetLight.color = magnetColor;
        magnetLight.intensity = 2f;
        magnetLight.range = 6f;
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
        
        if (magnetLight != null)
        {
            float pulse = (Mathf.Sin(Time.time * 4f) + 1f) * 0.5f;
            magnetLight.intensity = 1.5f + (pulse * 1.5f);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("🧲 FRAGMENT MAGNET COLLECTED!");
            
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                ApplyMagnetToPlayer(playerController);
            }
            
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, 0.7f);
            }
            
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                gameManager.UpdateStatusMessage($"FRAGMENT MAGNET! Attracts fragments for {magnetDuration}s!");
            }
            
            Destroy(gameObject);
        }
    }
    
    void ApplyMagnetToPlayer(PlayerController playerController)
    {
        PlayerFragmentMagnet playerMagnet = playerController.GetComponent<PlayerFragmentMagnet>();
        if (playerMagnet == null)
        {
            playerMagnet = playerController.gameObject.AddComponent<PlayerFragmentMagnet>();
        }
        
        playerMagnet.ActivateMagnet(magnetDuration);
        Debug.Log($"Fragment magnet applied to player for {magnetDuration} seconds!");
    }
}