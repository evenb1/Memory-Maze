using UnityEngine;

public class VirusAI : MonoBehaviour
{
    [Header("AI Settings")]
    public float moveSpeed = 3f;
    public float detectionRange = 15f;
    public float huntSpeed = 4f;
    public float patrolSpeed = 2f;
    
    [Header("Drone Effects")]
    public Transform droneModel;
    public float hoverHeight = 1.5f;
    public float bobSpeed = 2f;
    public float bobAmount = 0.3f;
    
    [Header("Light Beam")]
    public bool enableLightBeam = true;
    public float beamRange = 10f;
    
    [Header("Game Over Effects")]
    public float gameOverEffectTime = 2f;
    
    private Transform player;
    private GameManager gameManager;
    private bool isHunting = false;
    private bool hasCaughtPlayer = false;
    private Vector3 basePosition;
    private Renderer droneRenderer;
    private Material droneMaterial;
    private Light droneLight;
    
    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if(playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log($"Virus found player: {playerObj.name}");
        }
        else
        {
            Debug.LogError("Virus cannot find player! Make sure player is tagged 'Player'");
        }
        
        gameManager = FindFirstObjectByType<GameManager>();
        SetupDrone();
        basePosition = transform.position;
        
        SetupCollider();
        
        Debug.Log("Virus drone initialized - hunting for player!");
    }
    
    void SetupCollider()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.isTrigger = true;
            sphereCol.radius = 1f;
            Debug.Log("Added trigger collider to virus");
        }
        else
        {
            col.isTrigger = true;
            Debug.Log("Set existing collider as trigger");
        }
    }
    
    void SetupDrone()
    {
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
        
        droneRenderer = droneModel.GetComponent<Renderer>();
        if (droneRenderer == null)
        {
            droneRenderer = droneModel.GetComponentInChildren<Renderer>();
        }
        
        Vector3 pos = transform.position;
        pos.y = hoverHeight;
        transform.position = pos;
        
        if (enableLightBeam)
        {
            CreateLightBeam();
        }
        
        MakeVirusGlow();
    }
    
    void CreateLightBeam()
    {
        GameObject lightObj = new GameObject("DroneLight");
        lightObj.transform.SetParent(droneModel != null ? droneModel : transform);
        lightObj.transform.localPosition = Vector3.forward * 0.5f;
        
        droneLight = lightObj.AddComponent<Light>();
        droneLight.type = LightType.Spot;
        droneLight.color = Color.red;
        droneLight.intensity = 3f;
        droneLight.range = beamRange;
        droneLight.spotAngle = 30f;
        droneLight.innerSpotAngle = 15f;
    }
    
    void Update()
    {
        if(player != null && !hasCaughtPlayer)
        {
            HuntPlayer();
        }
        
        if (!hasCaughtPlayer)
        {
            AddHoverAnimation();
            UpdateDroneVisuals();
        }
    }
    
    void HuntPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if(distanceToPlayer <= detectionRange)
        {
            isHunting = true;
            
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            
            Vector3 newPosition = transform.position + direction * huntSpeed * Time.deltaTime;
            newPosition.y = hoverHeight;
            
            transform.position = newPosition;
            
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
            float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            Vector3 localPos = droneModel.localPosition;
            localPos.y = bobOffset;
            droneModel.localPosition = localPos;
        }
    }
    
    void UpdateDroneVisuals()
    {
        if (droneMaterial != null)
        {
            if (isHunting)
            {
                float intensity = (Mathf.Sin(Time.time * 8f) + 1f) * 0.5f + 1f;
                droneMaterial.SetColor("_EmissionColor", Color.red * intensity);
                
                if (droneLight != null) droneLight.intensity = 4f;
            }
            else
            {
                droneMaterial.SetColor("_EmissionColor", Color.red * 0.5f);
                
                if (droneLight != null) droneLight.intensity = 1f;
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && !hasCaughtPlayer)
        {
            hasCaughtPlayer = true;
            Debug.Log("GAME OVER! Virus drone caught player!");
            
            // Start dramatic game over sequence
            StartCoroutine(GameOverSequence(other.transform));
        }
    }
    
    System.Collections.IEnumerator GameOverSequence(Transform caughtPlayer)
    {
        Debug.Log("Starting virus capture sequence...");
        
        // Stop player movement immediately
        PlayerController playerController = caughtPlayer.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Stop virus movement
        this.enabled = false;
        
        // Show capture message
        if (gameManager != null)
        {
            gameManager.UpdateStatusMessage("SYSTEM BREACH! Virus has compromised BIT-27!");
        }
        
        // Create capture effects
        GameObject captureEffect = CreateCaptureEffect(caughtPlayer);
        
        // Make screen flash red
        StartCoroutine(RedFlashEffect());
        
        // Make player and virus glow red
        StartCoroutine(MakeCaptureGlow(caughtPlayer));
        
        // Wait for effects
        yield return new WaitForSeconds(gameOverEffectTime);
        
        // Clean up effects
        if (captureEffect != null)
        {
            Destroy(captureEffect);
        }
        
        // Final game over message
        if (gameManager != null)
        {
            gameManager.UpdateStatusMessage("DATA CORRUPTED! Memory recovery failed!");
        }
        
        // Trigger GameManager game over with delay
        yield return new WaitForSeconds(0.5f);
        
        if(gameManager != null)
        {
            gameManager.GameOver();
        }
    }
    
    GameObject CreateCaptureEffect(Transform player)
    {
        GameObject effect = new GameObject("CaptureEffect");
        effect.transform.position = player.position;
        
        // Create red corruption particles
        ParticleSystem particles = effect.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startColor = Color.red;
        main.startSpeed = 8f;
        main.startSize = 0.3f;
        main.maxParticles = 100;
        main.startLifetime = 2f;
        
        var emission = particles.emission;
        emission.rateOverTime = 50f;
        
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 1f;
        
        // Add menacing red light
        Light captureLight = effect.AddComponent<Light>();
        captureLight.type = LightType.Point;
        captureLight.color = Color.red;
        captureLight.intensity = 8f;
        captureLight.range = 10f;
        
        // Pulsing light effect
        StartCoroutine(PulseCaptureLight(captureLight));
        
        return effect;
    }
    
    System.Collections.IEnumerator PulseCaptureLight(Light light)
    {
        while (light != null)
        {
            float pulse = (Mathf.Sin(Time.time * 10f) + 1f) * 0.5f;
            light.intensity = 5f + (pulse * 8f);
            yield return null;
        }
    }
    
    System.Collections.IEnumerator RedFlashEffect()
    {
        // Create red screen overlay
        GameObject flashPanel = new GameObject("RedFlash");
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            flashPanel.transform.SetParent(canvas.transform);
        }
        
        UnityEngine.UI.Image flashImage = flashPanel.AddComponent<UnityEngine.UI.Image>();
        flashImage.color = new Color(1, 0, 0, 0); // Start transparent red
        
        // Set to full screen
        RectTransform rect = flashPanel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // Flash red multiple times
        for (int i = 0; i < 3; i++)
        {
            // Flash to red
            float timer = 0f;
            while (timer < 0.2f)
            {
                timer += Time.deltaTime;
                float alpha = timer / 0.2f;
                flashImage.color = new Color(1, 0, 0, alpha * 0.6f);
                yield return null;
            }
            
            // Fade out
            timer = 0f;
            while (timer < 0.3f)
            {
                timer += Time.deltaTime;
                float alpha = 1f - (timer / 0.3f);
                flashImage.color = new Color(1, 0, 0, alpha * 0.6f);
                yield return null;
            }
        }
        
        Destroy(flashPanel);
    }
    
    System.Collections.IEnumerator MakeCaptureGlow(Transform player)
    {
        // Make player glow red
        Renderer playerRenderer = player.GetComponentInChildren<Renderer>();
        Material originalPlayerMaterial = null;
        
        if (playerRenderer != null)
        {
            originalPlayerMaterial = playerRenderer.material;
            Material playerGlowMaterial = new Material(originalPlayerMaterial);
            playerGlowMaterial.EnableKeyword("_EMISSION");
            playerGlowMaterial.SetColor("_EmissionColor", Color.red * 2f);
            playerRenderer.material = playerGlowMaterial;
        }
        
        // Make virus glow intensely red
        if (droneMaterial != null)
        {
            droneMaterial.SetColor("_EmissionColor", Color.red * 3f);
        }
        
        if (droneLight != null)
        {
            droneLight.intensity = 10f;
            droneLight.color = Color.red;
        }
        
        yield return new WaitForSeconds(gameOverEffectTime);
        
        // Restore original materials
        if (playerRenderer != null && originalPlayerMaterial != null)
        {
            playerRenderer.material = originalPlayerMaterial;
        }
    }
    
    void MakeVirusGlow()
    {
        if (droneRenderer != null)
        {
            droneMaterial = new Material(droneRenderer.sharedMaterial);
            droneMaterial.EnableKeyword("_EMISSION");
            droneMaterial.SetColor("_EmissionColor", Color.red * 2f);
            droneRenderer.material = droneMaterial;
        }
        else
        {
            Debug.LogWarning("No renderer found for virus drone!");
        }
    }
    
    public void IncreaseSpeed(float speedBoost)
    {
        moveSpeed += speedBoost;
        huntSpeed += speedBoost;
        Debug.Log($"Virus drone speed increased! New speed: {huntSpeed}");
    }
}