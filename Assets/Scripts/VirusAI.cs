using UnityEngine;

public class VirusAI : MonoBehaviour
{
    [Header("AI Settings")]
    public float moveSpeed = 3f;
    public float detectionRange = 15f;
    public float huntSpeed = 4f;
    public float patrolSpeed = 2f;
    
    [Header("Smart Hunt Behavior")]
    public float huntDuration = 8f;           // How long to hunt before giving up
    public float giveUpDistance = 20f;        // Distance to give up hunt
    public float searchTime = 3f;             // Time spent searching last known position
    public float restTime = 5f;               // Time to rest after long hunt
    
    [Header("Wall Avoidance")]
    public float wallAvoidanceDistance = 2f;  // Distance to detect walls
    public LayerMask wallLayerMask = -1;      // What counts as walls
    public float wallAvoidanceForce = 5f;     // How strong wall avoidance is
    
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
    public float catchDistance = 0.8f;
    
    [Header("Audio Settings")]
    public AudioClip proximitySound;
    public AudioClip huntingSound;
    public float proximityDistance = 5f;
    public float audioVolume = 0.5f;
    
    [Header("Game Over Effects")]
    public float gameOverEffectTime = 2f;
    
    // AI State Machine
    private enum VirusState
    {
        Patrolling,
        Hunting,
        Searching,
        Resting,
        Investigating
    }
    
    private VirusState currentState = VirusState.Patrolling;
    private Transform player;
    private GameManager gameManager;
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
    private float stateTimer = 0f;
    private float huntStartTime = 0f;
    private bool isResting = false;
    private Vector3 investigationPoint;
    
    // Wall avoidance
    private Vector3[] rayDirections;
    
    void Start()
    {
        allFlyingViruses.Add(this);
        
        GameObject playerObj = GameObject.FindWithTag("Player");
        if(playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log($"Smart Virus found player: {playerObj.name}");
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
        SetupWallAvoidance();
        
        // Random initial state timer to spread out virus behaviors
        stateTimer = Random.Range(0f, 3f);
        
        Debug.Log($"Smart Virus {gameObject.name} initialized!");
    }
    
    void SetupWallAvoidance()
    {
        // Create rays in multiple directions for wall detection
        rayDirections = new Vector3[]
        {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right,
            new Vector3(1, 0, 1).normalized,   // Diagonal
            new Vector3(-1, 0, 1).normalized,
            new Vector3(1, 0, -1).normalized,
            new Vector3(-1, 0, -1).normalized
        };
    }
    
    void SetupAudio()
    {
        virusAudioSource = gameObject.AddComponent<AudioSource>();
        virusAudioSource.volume = audioVolume;
        virusAudioSource.spatialBlend = 1f;
        virusAudioSource.rolloffMode = AudioRolloffMode.Linear;
        virusAudioSource.minDistance = 2f;
        virusAudioSource.maxDistance = proximityDistance * 2f;
        virusAudioSource.loop = false;
    }
    
    void OnDestroy()
    {
        allFlyingViruses.Remove(this);
    }
    
    void SetupCollider()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.isTrigger = true;
            sphereCol.radius = 0.6f;
        }
        else
        {
            col.isTrigger = true;
            if (col is SphereCollider sphereCol)
            {
                sphereCol.radius = 0.6f;
            }
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
            stateTimer += Time.deltaTime;
            
            // Handle AI state machine
            HandleAIStateMachine();
            
            // Check for player capture
            CheckProximityToPlayer();
            
            // Handle audio
            HandleVirusAudio();
        }
        
        if (!hasCaughtPlayer)
        {
            AddHoverAnimation();
            UpdateDroneVisuals();
        }
    }
    
    void HandleAIStateMachine()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSeePlayer = CanSeePlayer();
        
        switch (currentState)
        {
            case VirusState.Patrolling:
                HandlePatrolling(distanceToPlayer, canSeePlayer);
                break;
                
            case VirusState.Hunting:
                HandleHunting(distanceToPlayer, canSeePlayer);
                break;
                
            case VirusState.Searching:
                HandleSearching(distanceToPlayer, canSeePlayer);
                break;
                
            case VirusState.Resting:
                HandleResting(distanceToPlayer, canSeePlayer);
                break;
                
            case VirusState.Investigating:
                HandleInvestigating(distanceToPlayer, canSeePlayer);
                break;
        }
    }
    
    void HandlePatrolling(float distanceToPlayer, bool canSeePlayer)
    {
        // Check if should start hunting
        if (distanceToPlayer <= detectionRange && canSeePlayer && !isResting)
        {
            ChangeState(VirusState.Hunting);
            lastKnownPlayerPosition = player.position;
            huntStartTime = Time.time;
            Debug.Log($"{gameObject.name} spotted player - beginning hunt!");
            return;
        }
        
        // Normal patrol behavior with wall avoidance
        PatrolWithWallAvoidance();
    }
    
    void HandleHunting(float distanceToPlayer, bool canSeePlayer)
    {
        // Update last known position if we can see player
        if (canSeePlayer && distanceToPlayer <= detectionRange * 1.5f)
        {
            lastKnownPlayerPosition = player.position;
        }
        
        // Give up hunt if conditions are met
        if (Time.time - huntStartTime > huntDuration || 
            distanceToPlayer > giveUpDistance ||
            (!canSeePlayer && Vector3.Distance(transform.position, lastKnownPlayerPosition) < 2f))
        {
            Debug.Log($"{gameObject.name} giving up hunt - switching to search or rest");
            
            if (Time.time - huntStartTime > huntDuration)
            {
                ChangeState(VirusState.Resting);
            }
            else
            {
                ChangeState(VirusState.Searching);
            }
            return;
        }
        
        // Hunt the player
        HuntPlayerSmart();
    }
    
    void HandleSearching(float distanceToPlayer, bool canSeePlayer)
    {
        // If we spot the player again, resume hunting
        if (canSeePlayer && distanceToPlayer <= detectionRange)
        {
            ChangeState(VirusState.Hunting);
            lastKnownPlayerPosition = player.position;
            huntStartTime = Time.time;
            return;
        }
        
        // Search last known position
        if (stateTimer < searchTime)
        {
            SearchLastKnownPosition();
        }
        else
        {
            // Done searching, go back to patrol or rest
            if (Random.Range(0f, 1f) < 0.3f) // 30% chance to rest
            {
                ChangeState(VirusState.Resting);
            }
            else
            {
                ChangeState(VirusState.Patrolling);
            }
        }
    }
    
    void HandleResting(float distanceToPlayer, bool canSeePlayer)
    {
        // Stay in place and rest
        if (stateTimer > restTime)
        {
            isResting = false;
            ChangeState(VirusState.Patrolling);
            Debug.Log($"{gameObject.name} finished resting - back to patrol");
        }
        
        // Only start hunting if player is very close while resting
        if (canSeePlayer && distanceToPlayer <= detectionRange * 0.5f)
        {
            isResting = false;
            ChangeState(VirusState.Hunting);
            lastKnownPlayerPosition = player.position;
            huntStartTime = Time.time;
        }
        
        // Gentle hovering while resting
        // (Just the hover animation, no movement)
    }
    
    void HandleInvestigating(float distanceToPlayer, bool canSeePlayer)
    {
        // Move to investigation point
        Vector3 direction = (investigationPoint - transform.position).normalized;
        MoveWithWallAvoidance(direction, patrolSpeed * 0.8f);
        
        // If reached investigation point or spotted player
        if (Vector3.Distance(transform.position, investigationPoint) < 2f || 
            (canSeePlayer && distanceToPlayer <= detectionRange))
        {
            if (canSeePlayer && distanceToPlayer <= detectionRange)
            {
                ChangeState(VirusState.Hunting);
                lastKnownPlayerPosition = player.position;
                huntStartTime = Time.time;
            }
            else
            {
                ChangeState(VirusState.Patrolling);
            }
        }
    }
    
    void ChangeState(VirusState newState)
    {
        currentState = newState;
        stateTimer = 0f;
        
        if (newState == VirusState.Resting)
        {
            isResting = true;
        }
    }
    
    bool CanSeePlayer()
    {
        if (player == null) return false;
        
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Raycast to check for walls blocking view
        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, distanceToPlayer, wallLayerMask))
        {
            return false; // Wall is blocking view
        }
        
        return true;
    }
    
    void PatrolWithWallAvoidance()
    {
        // Smart patrol behavior with wall avoidance
        float time = Time.time * patrolSpeed;
        Vector3 patrolOffset = new Vector3(
            Mathf.Sin(time) * 2f,
            0,
            Mathf.Cos(time * 0.7f) * 2f
        );
        
        // Add randomness
        patrolOffset += new Vector3(
            Mathf.Sin(time * 1.3f + gameObject.GetInstanceID()) * 1f,
            0,
            Mathf.Cos(time * 0.9f + gameObject.GetInstanceID()) * 1f
        );
        
        Vector3 targetPosition = basePosition + patrolOffset;
        Vector3 direction = (targetPosition - transform.position).normalized;
        
        MoveWithWallAvoidance(direction, patrolSpeed);
    }
    
    void HuntPlayerSmart()
    {
        Vector3 targetPosition = lastKnownPlayerPosition;
        
        // Add virus avoidance
        Vector3 avoidanceVector = CalculateFlyingVirusAvoidance();
        targetPosition += avoidanceVector;
        
        Vector3 direction = (targetPosition - transform.position).normalized;
        
        MoveWithWallAvoidance(direction, huntSpeed);
    }
    
    void SearchLastKnownPosition()
    {
        Vector3 direction = (lastKnownPlayerPosition - transform.position).normalized;
        
        if (Vector3.Distance(transform.position, lastKnownPlayerPosition) > 2f)
        {
            MoveWithWallAvoidance(direction, huntSpeed * 0.7f);
        }
    }
    
    void MoveWithWallAvoidance(Vector3 desiredDirection, float speed)
    {
        Vector3 wallAvoidance = CalculateWallAvoidance();
        Vector3 virusAvoidance = CalculateFlyingVirusAvoidance();
        
        // Combine all forces
        Vector3 finalDirection = (desiredDirection + wallAvoidance + virusAvoidance).normalized;
        
        Vector3 newPosition = transform.position + finalDirection * speed * Time.deltaTime;
        newPosition.y = hoverHeight; // Maintain flying height
        
        transform.position = newPosition;
        
        // Face movement direction
        if (finalDirection != Vector3.zero)
        {
            FaceDirection(finalDirection);
        }
    }
    
    Vector3 CalculateWallAvoidance()
    {
        Vector3 avoidance = Vector3.zero;
        
        foreach (Vector3 direction in rayDirections)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, wallAvoidanceDistance, wallLayerMask))
            {
                // Calculate avoidance force based on distance to wall
                float avoidanceStrength = (wallAvoidanceDistance - hit.distance) / wallAvoidanceDistance;
                avoidance -= direction * avoidanceStrength * wallAvoidanceForce;
            }
        }
        
        return avoidance;
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
                    Vector3 repulsion = (transform.position - otherVirus.transform.position).normalized;
                    float strength = (virusAvoidanceRadius - distance) / virusAvoidanceRadius;
                    avoidanceVector += repulsion * strength * 3f;
                }
            }
        }
        
        return avoidanceVector;
    }
    
    void CheckProximityToPlayer()
    {
        if (player == null || hasCaughtPlayer) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
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
            }
        }
        else if (distanceToPlayer > proximityDistance && isPlayingProximitySound)
        {
            isPlayingProximitySound = false;
        }
        
        // Play hunting sound when actively hunting
        if (currentState == VirusState.Hunting && huntingSound != null && !virusAudioSource.isPlaying)
        {
            virusAudioSource.clip = huntingSound;
            virusAudioSource.Play();
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
            
            Vector3 bounceDirection = (transform.position - player.position).normalized;
            transform.position += bounceDirection * 2f;
            
            if (gameManager != null)
            {
                gameManager.UpdateStatusMessage("SHIELD DEFLECTED VIRUS ATTACK!");
            }
            
            // Get stunned briefly after hitting shield
            ChangeState(VirusState.Resting);
            return;
        }
        
        hasCaughtPlayer = true;
        Debug.Log($"💀 GAME OVER! {gameObject.name} caught player!");
        
        StartCoroutine(GameOverSequence(player));
    }
    
    void FaceDirection(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
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
            Color emissionColor = Color.red * 0.5f;
            float intensity = 1f;
            
            switch (currentState)
            {
                case VirusState.Hunting:
                    emissionColor = Color.red * (1f + (Mathf.Sin(Time.time * 8f) + 1f) * 0.5f);
                    intensity = 4f;
                    break;
                    
                case VirusState.Searching:
                    emissionColor = Color.yellow * 0.8f;
                    intensity = 2f;
                    break;
                    
                case VirusState.Resting:
                    emissionColor = Color.red * 0.3f;
                    intensity = 0.5f;
                    break;
                    
                default:
                    emissionColor = Color.red * 0.5f;
                    intensity = 1f;
                    break;
            }
            
            droneMaterial.SetColor("_EmissionColor", emissionColor);
            
            if (droneLight != null)
            {
                droneLight.intensity = intensity;
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && !hasCaughtPlayer)
        {
            PlayerShield playerShield = other.GetComponent<PlayerShield>();
            if (playerShield != null && playerShield.IsShielded())
            {
                Vector3 bounceDirection = (transform.position - other.transform.position).normalized;
                transform.position += bounceDirection * 2f;
                
                if (gameManager != null)
                {
                    gameManager.UpdateStatusMessage("SHIELD DEFLECTED VIRUS ATTACK!");
                }
                
                ChangeState(VirusState.Resting);
                return;
            }
            
            TriggerPlayerCapture();
        }
    }
    
    // ... [Rest of the original methods like GameOverSequence, etc. remain the same] ...
    
    System.Collections.IEnumerator GameOverSequence(Transform caughtPlayer)
    {
        // [Keep existing GameOverSequence implementation]
        Debug.Log($"{gameObject.name} starting virus capture sequence...");
        
        PlayerController playerController = caughtPlayer.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.transform.SetParent(null);
            Vector3 lockedPosition = mainCamera.transform.position;
            Quaternion lockedRotation = mainCamera.transform.rotation;
            
            StartCoroutine(LockCamera(mainCamera, lockedPosition, lockedRotation));
        }
        
        this.enabled = false;
        
        if (gameManager != null)
        {
            gameManager.UpdateStatusMessage($"SYSTEM BREACH! {gameObject.name} has compromised BIT-27!");
        }
        
        yield return new WaitForSeconds(gameOverEffectTime);
        
        if (gameManager != null)
        {
            gameManager.UpdateStatusMessage("DATA CORRUPTED! Memory recovery failed!");
        }
        
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
    
    void MakeVirusGlow()
    {
        if (droneRenderer != null)
        {
            droneMaterial = new Material(droneRenderer.sharedMaterial);
            droneMaterial.EnableKeyword("_EMISSION");
            droneMaterial.SetColor("_EmissionColor", Color.red * 2f);
            droneRenderer.material = droneMaterial;
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
        Gizmos.color = currentState == VirusState.Hunting ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Show catch distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchDistance);
        
        // Show wall avoidance range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, wallAvoidanceDistance);
        
        // Show current state
        if (Application.isPlaying)
        {
            Vector3 textPos = transform.position + Vector3.up * 2f;
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(textPos, currentState.ToString());
            #endif
        }
    }
}