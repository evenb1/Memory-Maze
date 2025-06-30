using UnityEngine;

public class GroundPatrolVirus : MonoBehaviour
{
    [Header("AI Settings")]
    public float moveSpeed = 2.5f;
    public float detectionRange = 12f;
    public float huntSpeed = 3.5f;
    public float patrolSpeed = 1.5f;
    
    [Header("Robot Configuration")]
    public Transform robotModel; // Drag the main robot body here
    public Color virusColor = Color.red;
    public float glowIntensity = 1.5f;
    
    [Header("Movement Settings")]
    public bool useSimpleMovement = true; // Start with simple movement
    public float patrolRadius = 8f;
    public LayerMask wallLayerMask = 1; // Layer mask for walls
    public float wallCheckDistance = 1f; // Distance to check for walls ahead
    
    private Transform player;
    private GameManager gameManager;
    private CharacterController characterController; // Add character controller for proper movement
    private bool isHunting = false;
    private bool hasCaughtPlayer = false;
    private Vector3 basePosition;
    private Vector3 currentTarget;
    private float changeTargetTimer = 0f;
    private float changeTargetInterval = 3f;
    
    // Visual components
    private Renderer[] robotRenderers;
    private Material[] virusMaterials;
    
    void Start()
    {
        // Find player
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log($"Ground Patrol Virus found player: {playerObj.name}");
        }
        else
        {
            Debug.LogError("Ground Patrol Virus cannot find player! Make sure player is tagged 'Player'");
        }
        
        gameManager = FindFirstObjectByType<GameManager>();
        basePosition = transform.position;
        currentTarget = basePosition;
        
        SetupRobot();
        SetupMovement();
        SetupCollider();
        
        Debug.Log("Ground Patrol Virus initialized with simple movement!");
    }
    
    void SetupRobot()
    {
        // Auto-find robot model if not assigned
        if (robotModel == null)
        {
            robotModel = transform.GetChild(0);
        }
        
        // Apply virus colors to all robot parts
        robotRenderers = robotModel.GetComponentsInChildren<Renderer>();
        virusMaterials = new Material[robotRenderers.Length];
        
        for (int i = 0; i < robotRenderers.Length; i++)
        {
            if (robotRenderers[i] != null)
            {
                // Create new material with virus corruption
                virusMaterials[i] = new Material(Shader.Find("Standard"));
                virusMaterials[i].color = virusColor;
                virusMaterials[i].EnableKeyword("_EMISSION");
                virusMaterials[i].SetColor("_EmissionColor", virusColor * glowIntensity);
                virusMaterials[i].SetFloat("_Metallic", 0.3f);
                virusMaterials[i].SetFloat("_Smoothness", 0.7f);
                
                // Apply to renderer
                robotRenderers[i].material = virusMaterials[i];
                
                Debug.Log($"Applied virus material to: {robotRenderers[i].name}");
            }
        }
        
        Debug.Log($"Applied virus corruption to {robotRenderers.Length} robot parts");
    }
    
    void SetupMovement()
    {
        // Add CharacterController for proper ground-based movement
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
        }
        
        // Configure CharacterController
        characterController.radius = 0.5f;
        characterController.height = 2f;
        characterController.center = new Vector3(0, 1f, 0);
        characterController.slopeLimit = 45f;
        characterController.stepOffset = 0.3f;
        
        Debug.Log("CharacterController setup for ground movement");
    }
    
    void SetupCollider()
    {
        // We'll use the CharacterController for movement, but add a trigger for player detection
        SphereCollider triggerCollider = gameObject.AddComponent<SphereCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = 1.2f;
        triggerCollider.center = new Vector3(0, 1f, 0);
    }
    
    void Update()
    {
        if (player != null && !hasCaughtPlayer)
        {
            HandleSimpleAI();
            UpdateVisuals();
        }
    }
    
    void HandleSimpleAI()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= detectionRange)
        {
            // Player detected - hunt mode
            if (!isHunting)
            {
                isHunting = true;
                Debug.Log("Ground Patrol detected player - HUNTING!");
            }
            
            HuntPlayer();
        }
        else
        {
            // Patrol mode
            if (isHunting)
            {
                isHunting = false;
                SetNewPatrolTarget();
                Debug.Log("Ground Patrol lost player - patrolling");
            }
            
            PatrolArea();
        }
    }
    
    void HuntPlayer()
    {
        // Calculate direction to player
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Keep on ground
        
        // Check for walls ahead before moving
        if (CanMoveInDirection(direction))
        {
            // Use CharacterController for proper movement
            Vector3 moveVector = direction * huntSpeed * Time.deltaTime;
            characterController.Move(moveVector);
        }
        else
        {
            // If blocked, try to find alternate path
            Vector3 alternateDirection = FindAlternatePath(direction);
            if (alternateDirection != Vector3.zero)
            {
                Vector3 moveVector = alternateDirection * huntSpeed * 0.5f * Time.deltaTime;
                characterController.Move(moveVector);
            }
        }
        
        // Rotate to face movement direction
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }
        
        Debug.Log($"Hunting player! Distance: {Vector3.Distance(transform.position, player.position):F1}");
    }
    
    void PatrolArea()
    {
        // Calculate direction to patrol target
        Vector3 direction = (currentTarget - transform.position).normalized;
        direction.y = 0;
        
        // Check for walls before moving
        if (CanMoveInDirection(direction))
        {
            // Use CharacterController for movement
            Vector3 moveVector = direction * patrolSpeed * Time.deltaTime;
            characterController.Move(moveVector);
        }
        else
        {
            // If blocked, pick new target
            SetNewPatrolTarget();
        }
        
        // Rotate toward target
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 3f * Time.deltaTime);
        }
        
        // Check if reached target or time to change
        changeTargetTimer += Time.deltaTime;
        if (Vector3.Distance(transform.position, currentTarget) < 2f || changeTargetTimer >= changeTargetInterval)
        {
            SetNewPatrolTarget();
            changeTargetTimer = 0f;
        }
    }
    
    void SetNewPatrolTarget()
    {
        // Generate random point around base position
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
        currentTarget = basePosition + new Vector3(randomCircle.x, 0, randomCircle.y);
        
        Debug.Log($"New patrol target: {currentTarget}");
    }
    
    bool CanMoveInDirection(Vector3 direction)
    {
        // Cast a ray ahead to check for walls
        Vector3 rayStart = transform.position + Vector3.up * 0.5f; // Start ray from center
        float rayDistance = wallCheckDistance;
        
        // Check if there's a wall in the movement direction
        if (Physics.Raycast(rayStart, direction, rayDistance, wallLayerMask))
        {
            return false; // Wall detected
        }
        
        // Also check with a sphere cast for more reliable collision detection
        if (Physics.SphereCast(rayStart, 0.4f, direction, out RaycastHit hit, rayDistance))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                return false; // Wall detected
            }
        }
        
        return true; // Path is clear
    }
    
    Vector3 FindAlternatePath(Vector3 blockedDirection)
    {
        // Try directions at 45-degree angles to find alternate path
        Vector3[] alternateDirections = {
            Quaternion.Euler(0, 45f, 0) * blockedDirection,   // 45 degrees right
            Quaternion.Euler(0, -45f, 0) * blockedDirection,  // 45 degrees left
            Quaternion.Euler(0, 90f, 0) * blockedDirection,   // 90 degrees right
            Quaternion.Euler(0, -90f, 0) * blockedDirection   // 90 degrees left
        };
        
        foreach (Vector3 direction in alternateDirections)
        {
            if (CanMoveInDirection(direction))
            {
                return direction;
            }
        }
        
        return Vector3.zero; // No alternate path found
    }
    
    void UpdateVisuals()
    {
        if (virusMaterials != null && virusMaterials.Length > 0)
        {
            float intensity = isHunting ? glowIntensity * 2f : glowIntensity;
            
            // Pulsing effect when hunting
            if (isHunting)
            {
                float pulse = (Mathf.Sin(Time.time * 8f) + 1f) * 0.5f;
                intensity = glowIntensity * (1.5f + pulse * 0.5f);
            }
            
            // Apply to all materials
            foreach (Material mat in virusMaterials)
            {
                if (mat != null)
                {
                    mat.SetColor("_EmissionColor", virusColor * intensity);
                }
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasCaughtPlayer)
        {
            hasCaughtPlayer = true;
            Debug.Log("GAME OVER! Ground Patrol Virus caught player!");
            
            StartCoroutine(GameOverSequence());
        }
    }
    
    System.Collections.IEnumerator GameOverSequence()
    {
        Debug.Log("Starting ground patrol capture sequence...");
        
        // Stop movement
        this.enabled = false;
        
        // Stop player
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Show message
        if (gameManager != null)
        {
            gameManager.UpdateStatusMessage("SECURITY BREACH! Ground patrol captured BIT-27!");
        }
        
        // Intense glow effect
        if (virusMaterials != null)
        {
            foreach (Material mat in virusMaterials)
            {
                if (mat != null)
                {
                    mat.SetColor("_EmissionColor", virusColor * 5f);
                }
            }
        }
        
        yield return new WaitForSeconds(2f);
        
        if (gameManager != null)
        {
            gameManager.GameOver();
        }
    }
    
    public void IncreaseSpeed(float speedBoost)
    {
        moveSpeed += speedBoost;
        huntSpeed += speedBoost;
        patrolSpeed += speedBoost;
        
        Debug.Log($"Ground Patrol speed increased! Hunt speed: {huntSpeed}");
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        // Show detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Show patrol radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(basePosition, patrolRadius);
        
        // Show current target
        if (currentTarget != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(currentTarget, 1f);
            Gizmos.DrawLine(transform.position, currentTarget);
        }
    }
}