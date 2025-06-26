using UnityEngine;

public class VirusAI : MonoBehaviour
{
    [Header("AI Settings")]
    public float moveSpeed = 3f;
    public float detectionRange = 15f;
    public float huntSpeed = 4f;
    public float patrolSpeed = 2f;
    
    [Header("Drone Effects")]
    public Transform droneModel; // Assign your bot drone model here
    public float hoverHeight = 1.5f;
    public float bobSpeed = 2f;
    public float bobAmount = 0.3f;
    
    [Header("Light Beam")]
    public bool enableLightBeam = true;
    public float beamRange = 10f;
    
    private Transform player;
    private GameManager gameManager;
    private bool isHunting = false;
    private Vector3 basePosition;
    private Renderer droneRenderer;
    private Material droneMaterial; // Store our own material instance
    private Light droneLight;
    
    void Start()
    {
        GameObject playerObj = GameObject.Find("Player");
        if(playerObj == null) playerObj = GameObject.Find("Bit27");
        
        if(playerObj != null)
        {
            player = playerObj.transform;
        }
        
        gameManager = FindObjectOfType<GameManager>();
        
        // Setup drone features
        SetupDrone();
        basePosition = transform.position;
        
        Debug.Log("Virus drone initialized - hunting for player!");
    }
    
    void SetupDrone()
    {
        // If no drone model assigned, use the first child or itself
        if (droneModel == null)
        {
            if (transform.childCount > 0)
            {
                droneModel = transform.GetChild(0);
            }
            else
            {
                droneModel = transform;
            }
        }
        
        // Get renderer for effects
        droneRenderer = droneModel.GetComponent<Renderer>();
        if (droneRenderer == null)
        {
            droneRenderer = droneModel.GetComponentInChildren<Renderer>();
        }
        
        // Position drone at hover height
        Vector3 pos = transform.position;
        pos.y = hoverHeight;
        transform.position = pos;
        
        // Add light beam
        if (enableLightBeam)
        {
            CreateLightBeam();
        }
        
        // Make virus look menacing
        MakeVirusGlow();
    }
    
    void CreateLightBeam()
    {
        // Create spotlight
        GameObject lightObj = new GameObject("DroneLight");
        lightObj.transform.SetParent(droneModel != null ? droneModel : transform);
        lightObj.transform.localPosition = Vector3.forward * 0.5f; // In front of drone
        
        droneLight = lightObj.AddComponent<Light>();
        droneLight.type = LightType.Spot;
        droneLight.color = Color.red;
        droneLight.intensity = 3f;
        droneLight.range = beamRange;
        droneLight.spotAngle = 30f; // Narrow beam
        droneLight.innerSpotAngle = 15f;
    }
    
    void Update()
    {
        if(player != null)
        {
            HuntPlayer();
        }
        
        // Add floating/hovering animation
        AddHoverAnimation();
        
        // Update visual effects
        UpdateDroneVisuals();
    }
    
    void HuntPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if(distanceToPlayer <= detectionRange)
        {
            isHunting = true;
            
            // Move toward player but keep at hover height
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0; // Don't move up/down
            
            Vector3 newPosition = transform.position + direction * huntSpeed * Time.deltaTime;
            newPosition.y = hoverHeight; // Maintain hover height
            
            transform.position = newPosition;
            
            // Look at player (smooth rotation)
            Vector3 lookDirection = player.position - transform.position;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 3f * Time.deltaTime);
            }
        }
        else
        {
            isHunting = false;
            
            // Patrol when not hunting
            float time = Time.time * patrolSpeed;
            Vector3 patrolOffset = new Vector3(
                Mathf.Sin(time) * 2f,
                0,
                Mathf.Cos(time * 0.7f) * 2f
            );
            
            Vector3 targetPosition = basePosition + patrolOffset;
            targetPosition.y = hoverHeight;
            
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, patrolSpeed * Time.deltaTime);
        }
    }
    
    void AddHoverAnimation()
    {
        if (droneModel != null)
        {
            // Bob up and down
            float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            Vector3 localPos = droneModel.localPosition;
            localPos.y = bobOffset;
            droneModel.localPosition = localPos;
            
            // Removed rotation animation to fix facing conflict
        }
    }
    
    void UpdateDroneVisuals()
    {
        // Only update if we have a valid material
        if (droneMaterial != null)
        {
            if (isHunting)
            {
                // Intense red glow when hunting
                float intensity = (Mathf.Sin(Time.time * 8f) + 1f) * 0.5f + 1f;
                droneMaterial.SetColor("_EmissionColor", Color.red * intensity);
                
                // Bright light when hunting
                if (droneLight != null) droneLight.intensity = 4f;
            }
            else
            {
                // Dim red glow when patrolling
                droneMaterial.SetColor("_EmissionColor", Color.red * 0.5f);
                
                // Dim light when patrolling
                if (droneLight != null) droneLight.intensity = 1f;
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name.Contains("Player") || other.gameObject.name.Contains("Bit27"))
        {
            Debug.Log("GAME OVER! Virus drone caught Bit-27!");
            
            if(gameManager != null)
            {
                gameManager.GameOver();
            }
        }
    }
    
    void MakeVirusGlow()
    {
        if (droneRenderer != null)
        {
            // Create our own material instance (not shared)
            droneMaterial = new Material(droneRenderer.sharedMaterial);
            droneMaterial.EnableKeyword("_EMISSION");
            droneMaterial.SetColor("_EmissionColor", Color.red * 2f);
            
            // Assign our material instance to the renderer
            droneRenderer.material = droneMaterial;
        }
        else
        {
            Debug.LogWarning("No renderer found for virus drone!");
        }
    }
    
    // Called by GameManager when fragments collected
    public void IncreaseSpeed(float speedBoost)
    {
        moveSpeed += speedBoost;
        huntSpeed += speedBoost;
        Debug.Log($"Virus drone speed increased! New speed: {huntSpeed}");
    }
}