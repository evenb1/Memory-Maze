using UnityEngine;

public class GroundPatrolVirus : MonoBehaviour
{
    [Header("AI Settings")]
    public float moveSpeed = 2.5f;
    public float detectionRange = 12f;
    public float huntSpeed = 3.5f;
    public float patrolSpeed = 1.5f;
    public float rotationSpeed = 5f;
    
    [Header("Robot Configuration")]
    public Transform robotModel; // Drag the main robot body here
    public Color virusColor = Color.red;
    public float glowIntensity = 1.5f;
    
    [Header("Smart AI Settings")]
    public float avoidanceRadius = 3f; // Distance to avoid other viruses
    public float wallCheckDistance = 1.5f;
    public float patrolRadius = 8f;
    
    private Transform player;
    private GameManager gameManager;
    private bool isHunting = false;
    private bool hasCaughtPlayer = false;
    private Vector3 basePosition;
    private Vector3 currentTarget;
    private float changeTargetTimer = 0f;
    private float changeTargetInterval = 3f;
    
    // Smart AI variables
    private static System.Collections.Generic.List<GroundPatrolVirus> allGroundViruses = new System.Collections.Generic.List<GroundPatrolVirus>();
    private Vector3 lastKnownPlayerPosition;
    private float lostPlayerTimer = 0f;
    
    // Visual components
    private Renderer[] robotRenderers;
    private Material[] virusMaterials;
    
    void Start()
    {
        // Add this virus to the static list for collision avoidance
        allGroundViruses.Add(this);
        
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
        lastKnownPlayerPosition = basePosition;
        
        SetupRobot();
        SetupSimpleCollider();
        
        Debug.Log($"Simple Ground Patrol Virus initialized at position: {transform.position}");
    }
    
    void OnDestroy()
    {
        // Remove from static list when destroyed
        allGroundViruses.Remove(this);
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
    
    void SetupSimpleCollider()
    {
        // Remove any existing physics components that might cause flying
        Rigidbody existingRb = GetComponent<Rigidbody>();
        if (existingRb != null)
        {
            Destroy(existingRb);
            Debug.Log("Removed Rigidbody to prevent flying away");
        }
        
        CharacterController existingCC = GetComponent<CharacterController>();
        if (existingCC != null)
        {
            Destroy(existingCC);
            Debug.Log("Removed CharacterController to prevent conflicts");
        }
        
        // Simple trigger collider for player detection
        SphereCollider triggerCollider = GetComponent<SphereCollider>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<SphereCollider>();
        }
        triggerCollider.isTrigger = true;
        triggerCollider.radius = 1.2f;
        triggerCollider.center = new Vector3(0, 1f, 0);
        
        Debug.Log("Setup simple trigger collider - no more flying!");
    }
    
    void Update()
    {
        if (player != null && !hasCaughtPlayer)
        {
            // Keep grounded
            KeepGrounded();
            
            HandleSmartAI();
            UpdateVisuals();
        }
    }
    
    void KeepGrounded()
    {
        // Force Y position to stay at ground level
        Vector3 pos = transform.position;
        pos.y = 1f; // Keep at ground level
        transform.position = pos;
    }
    
    void HandleSmartAI()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= detectionRange)
        {
            // Player detected - hunt mode
            if (!isHunting)
            {
                isHunting = true;
                Debug.Log($"{gameObject.name} detected player - HUNTING!");
            }
            
            lastKnownPlayerPosition = player.position;
            lostPlayerTimer = 0f;
            HuntPlayerSmart();
        }
        else if (isHunting && lostPlayerTimer < 5f)
        {
            // Lost player but remember last position
            lostPlayerTimer += Time.deltaTime;
            SearchLastKnownPosition();
        }
        else
        {
            // Patrol mode
            if (isHunting)
            {
                isHunting = false;
                SetNewPatrolTarget();
                Debug.Log($"{gameObject.name} lost player - returning to patrol");
            }
            
            PatrolAreaSmart();
        }
    }
    
    void HuntPlayerSmart()
    {
        Vector3 targetPosition = player.position;
        
        // Add smart spacing - avoid other viruses
        Vector3 avoidanceVector = CalculateVirusAvoidance();
        targetPosition += avoidanceVector;
        
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Keep on ground
        
        // Check for walls and navigate around them
        if (!CanMoveInDirection(direction))
        {
            direction = FindSmartAlternatePath(direction, targetPosition);
        }
        
        if (direction != Vector3.zero)
        {
            // Simple transform movement - no physics!
            Vector3 newPosition = transform.position + direction * huntSpeed * Time.deltaTime;
            newPosition.y = 1f; // Force ground level
            transform.position = newPosition;
            
            // Face forward in movement direction
            FaceDirection(direction);
        }
        
        Debug.Log($"{gameObject.name} hunting! Distance: {Vector3.Distance(transform.position, player.position):F1}");
    }
    
    void SearchLastKnownPosition()
    {
        Vector3 direction = (lastKnownPlayerPosition - transform.position).normalized;
        direction.y = 0;
        
        if (Vector3.Distance(transform.position, lastKnownPlayerPosition) > 2f)
        {
            if (CanMoveInDirection(direction))
            {
                Vector3 newPosition = transform.position + direction * huntSpeed * 0.7f * Time.deltaTime;
                newPosition.y = 1f; // Force ground level
                transform.position = newPosition;
                FaceDirection(direction);
            }
        }
        else
        {
            // Reached last known position - give up hunt
            lostPlayerTimer = 10f;
        }
    }
    
    void PatrolAreaSmart()
    {
        Vector3 direction = (currentTarget - transform.position).normalized;
        direction.y = 0;
        
        // Add virus avoidance during patrol too
        Vector3 avoidanceVector = CalculateVirusAvoidance();
        direction += avoidanceVector;
        direction.Normalize();
        
        if (CanMoveInDirection(direction))
        {
            Vector3 newPosition = transform.position + direction * patrolSpeed * Time.deltaTime;
            newPosition.y = 1f; // Force ground level
            transform.position = newPosition;
            FaceDirection(direction);
        }
        else
        {
            // Pick new target if blocked
            SetNewPatrolTarget();
        }
        
        // Check if reached target or time to change
        changeTargetTimer += Time.deltaTime;
        if (Vector3.Distance(transform.position, currentTarget) < 2f || changeTargetTimer >= changeTargetInterval)
        {
            SetNewPatrolTarget();
            changeTargetTimer = 0f;
        }
    }
    
    Vector3 CalculateVirusAvoidance()
    {
        Vector3 avoidanceVector = Vector3.zero;
        
        foreach (GroundPatrolVirus otherVirus in allGroundViruses)
        {
            if (otherVirus != this && otherVirus != null)
            {
                float distance = Vector3.Distance(transform.position, otherVirus.transform.position);
                
                if (distance < avoidanceRadius && distance > 0.1f)
                {
                    // Calculate repulsion force
                    Vector3 repulsion = (transform.position - otherVirus.transform.position).normalized;
                    float strength = (avoidanceRadius - distance) / avoidanceRadius;
                    avoidanceVector += repulsion * strength * 2f;
                }
            }
        }
        
        return avoidanceVector;
    }
    
    void FaceDirection(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    bool CanMoveInDirection(Vector3 direction)
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        
        // Check for walls with raycast
        if (Physics.Raycast(rayStart, direction, wallCheckDistance))
        {
            RaycastHit hit;
            if (Physics.Raycast(rayStart, direction, out hit, wallCheckDistance))
            {
                if (hit.collider.CompareTag("Wall"))
                {
                    return false;
                }
            }
        }
        
        return true;
    }
    
    Vector3 FindSmartAlternatePath(Vector3 blockedDirection, Vector3 finalTarget)
    {
        // Try multiple angles to find the best path toward target
        float[] angles = { 30f, -30f, 60f, -60f, 90f, -90f };
        
        Vector3 bestDirection = Vector3.zero;
        float bestScore = -1f;
        
        foreach (float angle in angles)
        {
            Vector3 testDirection = Quaternion.Euler(0, angle, 0) * blockedDirection;
            
            if (CanMoveInDirection(testDirection))
            {
                // Score based on how close this direction gets us to the target
                Vector3 testPosition = transform.position + testDirection;
                float distanceToTarget = Vector3.Distance(testPosition, finalTarget);
                float score = 1f / (distanceToTarget + 0.1f);
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestDirection = testDirection;
                }
            }
        }
        
        return bestDirection;
    }
    
    void SetNewPatrolTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
        currentTarget = basePosition + new Vector3(randomCircle.x, 0, randomCircle.y);
        currentTarget.y = 1f; // Keep target at ground level
        
        Debug.Log($"{gameObject.name} new patrol target: {currentTarget}");
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
            Debug.Log($"GAME OVER! {gameObject.name} caught player!");
            
            StartCoroutine(GameOverSequence());
        }
    }
    
    System.Collections.IEnumerator GameOverSequence()
    {
        Debug.Log($"{gameObject.name} starting capture sequence...");
        
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
            gameManager.UpdateStatusMessage($"SECURITY BREACH! {gameObject.name} captured BIT-27!");
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
        
        Debug.Log($"{gameObject.name} speed increased! Hunt speed: {huntSpeed}");
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        // Show detection range
        Gizmos.color = isHunting ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Show avoidance radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, avoidanceRadius);
        
        // Show current target
        if (currentTarget != Vector3.zero)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(currentTarget, 1f);
            Gizmos.DrawLine(transform.position, currentTarget);
        }
    }
}