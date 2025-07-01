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
    
    [Header("Smart AI Settings")]
    public float virusAvoidanceRadius = 4f;
    public float rotationSpeed = 3f;
    
    [Header("Collision Detection")]
    public float catchDistance = 0.8f; // Distance to catch player - much closer!
    
    [Header("Audio Settings")]
    public AudioClip proximitySound;          // Sound when near player
    public AudioClip huntingSound;            // Sound when hunting
    public float proximityDistance = 5f;      // Distance to play proximity sound
    public float audioVolume = 0.5f;
    
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
    
    // Audio system
    private AudioSource virusAudioSource;
    private bool isPlayingProximitySound = false;
    
    // Smart AI variables
    private static System.Collections.Generic.List<VirusAI> allFlyingViruses = new System.Collections.Generic.List<VirusAI>();
    private Vector3 lastKnownPlayerPosition;
    private float lostPlayerTimer = 0f;
    
    void Start()
    {
        // Add to static list for avoidance
        allFlyingViruses.Add(this);
        
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
        SetupAudio();
        basePosition = transform.position;
        lastKnownPlayerPosition = basePosition;
        
        SetupCollider();
        
        Debug.Log("Smart flying virus initialized - hunting for player!");
    }
    
    void SetupAudio()
    {
        // Add audio source for virus sounds
        virusAudioSource = gameObject.AddComponent<AudioSource>();
        virusAudioSource.volume = audioVolume;
        virusAudioSource.spatialBlend = 1f; // 3D sound
        virusAudioSource.rolloffMode = AudioRolloffMode.Linear;
        virusAudioSource.minDistance = 2f;
        virusAudioSource.maxDistance = proximityDistance * 2f;
        virusAudioSource.loop = false;
        
        Debug.Log("Virus audio system setup complete");
    }
    
    void OnDestroy()
    {
        // Remove from static list when destroyed
        allFlyingViruses.Remove(this);
    }
    
    void SetupCollider()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.isTrigger = true;
            sphereCol.radius = 0.6f; // Smaller collider for more precise detection
            Debug.Log($"Added trigger collider to virus with radius 0.6f");
        }
        else
        {
            col.isTrigger = true;
            if (col is SphereCollider sphereCol)
            {
                sphereCol.radius = 0.6f; // Smaller radius
            }
            Debug.Log("Set existing collider as trigger with smaller radius");
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
            HuntPlayerSmart();
            
            // ENHANCED: Check distance-based collision for better detection
            CheckProximityToPlayer();
            
            // AUDIO: Handle proximity and hunting sounds
            HandleVirusAudio();
        }
        
        if (!hasCaughtPlayer)
        {
            AddHoverAnimation();
            UpdateDroneVisuals();
        }
    }
    
    void CheckProximityToPlayer()
    {
        if (player == null || hasCaughtPlayer) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Only catch if VERY close - like actually touching
        if (distanceToPlayer <= catchDistance)
        {
            Debug.Log($"🔥 CLOSE CONTACT! {gameObject.name} got within {distanceToPlayer:F2} units of player!");
            TriggerPlayerCapture();
        }
    }
    
    void HandleVirusAudio()
    {
        if (player == null || virusAudioSource == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Play proximity sound when close to player
        if (distanceToPlayer <= proximityDistance && !isPlayingProximitySound)
        {
            if (proximitySound != null && !virusAudioSource.isPlaying)
            {
                virusAudioSource.clip = proximitySound;
                virusAudioSource.Play();
                isPlayingProximitySound = true;
                Debug.Log($"🔊 {gameObject.name} playing proximity sound - player at {distanceToPlayer:F1} units");
            }
        }
        else if (distanceToPlayer > proximityDistance && isPlayingProximitySound)
        {
            isPlayingProximitySound = false;
        }
        
        // Play hunting sound when actively hunting
        if (isHunting && huntingSound != null && !virusAudioSource.isPlaying)
        {
            virusAudioSource.clip = huntingSound;
            virusAudioSource.Play();
            Debug.Log($"🎯 {gameObject.name} playing hunting sound");
        }
    }
    
    void TriggerPlayerCapture()
    {
        if (hasCaughtPlayer) return;
        
        // Check if player has shield protection
        PlayerShield playerShield = player.GetComponent<PlayerShield>();
        if (playerShield != null && playerShield.IsShielded())
        {
            Debug.Log($"🛡️ Player is SHIELDED! {gameObject.name} cannot catch them!");
            
            // Bounce off the shield
            Vector3 bounceDirection = (transform.position - player.position).normalized;
            transform.position += bounceDirection * 2f;
            
            // Show shield deflection message
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                gameManager.UpdateStatusMessage("SHIELD DEFLECTED VIRUS ATTACK!");
            }
            
            return; // Don't catch the player
        }
        
        hasCaughtPlayer = true;
        Debug.Log($"💀 GAME OVER! {gameObject.name} caught player!");
        
        // Start dramatic game over sequence
        StartCoroutine(GameOverSequence(player));
    }
    
    void HuntPlayerSmart()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if(distanceToPlayer <= detectionRange)
        {
            // Player detected - hunt mode
            if (!isHunting)
            {
                isHunting = true;
                Debug.Log($"{gameObject.name} detected player - HUNTING!");
            }
            
            lastKnownPlayerPosition = player.position;
            lostPlayerTimer = 0f;
            
            Vector3 targetPosition = player.position;
            
            // Add virus avoidance to prevent clustering
            Vector3 avoidanceVector = CalculateFlyingVirusAvoidance();
            targetPosition += avoidanceVector;
            
            Vector3 direction = (targetPosition - transform.position).normalized;
            
            Vector3 newPosition = transform.position + direction * huntSpeed * Time.deltaTime;
            newPosition.y = hoverHeight; // Maintain flying height
            
            transform.position = newPosition;
            
            // Face forward in movement direction
            FaceDirection(direction);
        }
        else if (isHunting && lostPlayerTimer < 5f)
        {
            // Search last known position
            lostPlayerTimer += Time.deltaTime;
            SearchLastKnownPosition();
        }
        else
        {
            // Lost player or not detected - patrol mode
            if (isHunting)
            {
                isHunting = false;
                Debug.Log($"{gameObject.name} lost player - returning to patrol");
            }
            
            PatrolArea();
        }
    }
    
    void SearchLastKnownPosition()
    {
        Vector3 direction = (lastKnownPlayerPosition - transform.position).normalized;
        
        if (Vector3.Distance(transform.position, lastKnownPlayerPosition) > 2f)
        {
            Vector3 newPosition = transform.position + direction * huntSpeed * 0.7f * Time.deltaTime;
            newPosition.y = hoverHeight;
            transform.position = newPosition;
            
            FaceDirection(direction);
        }
        else
        {
            // Reached last known position - give up hunt
            lostPlayerTimer = 10f;
        }
    }
    
    void PatrolArea()
    {
        // Smart patrol behavior with virus avoidance
        float time = Time.time * patrolSpeed;
        Vector3 patrolOffset = new Vector3(
            Mathf.Sin(time) * 2f,
            0,
            Mathf.Cos(time * 0.7f) * 2f
        );
        
        // Add some randomness to avoid identical patrol patterns
        patrolOffset += new Vector3(
            Mathf.Sin(time * 1.3f + gameObject.GetInstanceID()) * 1f,
            0,
            Mathf.Cos(time * 0.9f + gameObject.GetInstanceID()) * 1f
        );
        
        Vector3 targetPosition = basePosition + patrolOffset;
        
        // Add virus avoidance during patrol
        Vector3 avoidanceVector = CalculateFlyingVirusAvoidance();
        targetPosition += avoidanceVector;
        
        targetPosition.y = hoverHeight;
        
        Vector3 patrolDirection = (targetPosition - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, patrolSpeed * Time.deltaTime);
        
        // Face patrol direction
        if (patrolDirection != Vector3.zero)
        {
            FaceDirection(patrolDirection);
        }
    }
    
    Vector3 CalculateFlyingVirusAvoidance()
    {
        Vector3 avoidanceVector = Vector3.zero;
        
        foreach (VirusAI otherVirus in allFlyingViruses)
        {
            if (otherVirus != this && otherVirus != null)
            {
                float distance = Vector3.Distance(transform.position, otherVirus.transform.position);
                
                if (distance < virusAvoidanceRadius && distance > 0.1f)
                {
                    // Calculate repulsion force
                    Vector3 repulsion = (transform.position - otherVirus.transform.position).normalized;
                    float strength = (virusAvoidanceRadius - distance) / virusAvoidanceRadius;
                    avoidanceVector += repulsion * strength * 3f;
                }
            }
        }
        
        return avoidanceVector;
    }
    
    void FaceDirection(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            // Create rotation that faces the movement direction
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
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
        Debug.Log($"🎯 Virus {gameObject.name} triggered with: {other.gameObject.name}, Tag: {other.tag}");
        
        if(other.CompareTag("Player") && !hasCaughtPlayer)
        {
            // Check if player has shield protection
            PlayerShield playerShield = other.GetComponent<PlayerShield>();
            if (playerShield != null && playerShield.IsShielded())
            {
                Debug.Log($"🛡️ Player is SHIELDED! {gameObject.name} bounced off!");
                
                // Bounce off the shield
                Vector3 bounceDirection = (transform.position - other.transform.position).normalized;
                transform.position += bounceDirection * 2f;
                
                // Show shield deflection message
                GameManager gameManager = FindFirstObjectByType<GameManager>();
                if (gameManager != null)
                {
                    gameManager.UpdateStatusMessage("SHIELD DEFLECTED VIRUS ATTACK!");
                }
                
                return; // Don't catch the player
            }
            
            Debug.Log($"💀 TRIGGER CATCH! {gameObject.name} caught player via OnTriggerEnter!");
            TriggerPlayerCapture();
        }
    }
    
    System.Collections.IEnumerator GameOverSequence(Transform caughtPlayer)
    {
        Debug.Log($"{gameObject.name} starting virus capture sequence...");
        
        // Stop player movement immediately
        PlayerController playerController = caughtPlayer.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // LOCK THE CAMERA POSITION
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // Lock camera to current position/rotation
            mainCamera.transform.SetParent(null);
            Vector3 lockedPosition = mainCamera.transform.position;
            Quaternion lockedRotation = mainCamera.transform.rotation;
            
            StartCoroutine(LockCamera(mainCamera, lockedPosition, lockedRotation));
        }
        
        // Stop virus movement
        this.enabled = false;
        
        // Show capture message
        if (gameManager != null)
        {
            gameManager.UpdateStatusMessage($"SYSTEM BREACH! {gameObject.name} has compromised BIT-27!");
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
    
    System.Collections.IEnumerator LockCamera(Camera camera, Vector3 position, Quaternion rotation)
    {
        while (camera != null)
        {
            camera.transform.position = position;
            camera.transform.rotation = rotation;
            yield return null;
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
        flashImage.color = new Color(1, 0, 0, 0);
        
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
        Debug.Log($"{gameObject.name} speed increased! New speed: {huntSpeed}");
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        // Show detection range
        Gizmos.color = isHunting ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Show catch distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchDistance);
        
        // Show proximity audio range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, proximityDistance);
        
        // Show virus avoidance radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, virusAvoidanceRadius);
        
        // Show last known player position if searching
        if (isHunting || lostPlayerTimer < 5f)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(lastKnownPlayerPosition, 0.5f);
            Gizmos.DrawLine(transform.position, lastKnownPlayerPosition);
        }
    }
}