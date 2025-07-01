using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float sprintSpeed = 5f;      // Sprint speed when holding Shift
    public float rotationSpeed = 5f;
    
    [Header("Simple Camera")]
    public float mouseSensitivity = 2f;
    
    [Header("Speed Boost System")]
    public float baseMovementSpeed = 3f;        // Will be set to moveSpeed in Start()
    public bool hasSpeedBoost = false;
    public float currentSpeedMultiplier = 1f;
    public float speedBoostTimeRemaining = 0f;
    
    [Header("Speed Boost UI (Optional)")]
    public UnityEngine.UI.Image speedBoostIndicator; // Drag UI element here if you have one
    public Color boostActiveColor = Color.yellow;
    public Color boostInactiveColor = Color.gray;
    
    [Header("Speed Boost Effects")]
    public bool showSpeedBoostGlow = true;
    public Color speedBoostGlowColor = Color.yellow;
    
    private CharacterController controller;
    private Animator animator;
    private Camera playerCamera;
    private float cameraRotationY = 0f;
    private Coroutine speedBoostCoroutine;
    private Renderer playerRenderer;
    private Material originalMaterial;
    private Material speedBoostMaterial;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerCamera = Camera.main;
        
        // Initialize speed boost system
        baseMovementSpeed = moveSpeed; // Store original speed
        
        // Get renderer for glow effects
        playerRenderer = GetComponentInChildren<Renderer>();
        if (playerRenderer != null)
        {
            originalMaterial = playerRenderer.material;
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log($"PlayerController initialized. Base speed: {baseMovementSpeed}, Sprint speed: {sprintSpeed}");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? 
                CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
        }
        
        HandleMovement();
        HandleSimpleCamera();
        
        // Update speed boost system
        UpdateSpeedBoostTimer();
        UpdateSpeedBoostUI();
    }
    
    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // Check if sprinting (Shift key held)
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float currentSpeed = GetCurrentMoveSpeed(isSprinting);
        
        // Movement relative to camera direction
        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;
        forward.y = 0; // Keep movement horizontal
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        
        Vector3 moveDirection = (forward * vertical + right * horizontal).normalized;
        
        if (moveDirection.magnitude > 0.1f)
        {
            // Move character using current speed (includes speed boost and sprint)
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);
            
            // Rotate character to face movement direction
            transform.rotation = Quaternion.Slerp(transform.rotation, 
                Quaternion.LookRotation(moveDirection), rotationSpeed * Time.deltaTime);
            
            if (animator != null)
            {
                // Adjust animation speed based on current speed
                float animSpeedMultiplier = currentSpeed / baseMovementSpeed;
                animator.speed = animSpeedMultiplier;
                animator.SetFloat("Blend", 0.5f); // Walking animation
            }
        }
        else
        {
            if (animator != null)
            {
                animator.speed = 1f; // Normal speed when idle
                animator.SetFloat("Blend", 0f); // Idle animation
            }
        }
    }
    
    void HandleSimpleCamera()
    {
        if (playerCamera == null) return;
        
        // Mouse rotation around Y axis only
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            cameraRotationY += Input.GetAxis("Mouse X") * mouseSensitivity;
        }
        
        // Fixed camera position (no smoothing)
        Vector3 offset = new Vector3(0, 1.2f, -1.8f);
        Vector3 rotatedOffset = Quaternion.Euler(0, cameraRotationY, 0) * offset;
        
        // Direct positioning (no lerp)
        playerCamera.transform.position = transform.position + rotatedOffset;
        playerCamera.transform.LookAt(transform.position + Vector3.up * 1f);
    }
    
    void UpdateSpeedBoostTimer()
    {
        if (hasSpeedBoost && speedBoostTimeRemaining > 0)
        {
            speedBoostTimeRemaining -= Time.deltaTime;
            
            if (speedBoostTimeRemaining <= 0)
            {
                RemoveSpeedBoost();
            }
        }
    }
    
    void UpdateSpeedBoostUI()
    {
        if (speedBoostIndicator != null)
        {
            if (hasSpeedBoost)
            {
                speedBoostIndicator.color = boostActiveColor;
                speedBoostIndicator.fillAmount = speedBoostTimeRemaining / 8f; // Assuming 8s duration
            }
            else
            {
                speedBoostIndicator.color = boostInactiveColor;
                speedBoostIndicator.fillAmount = 0f;
            }
        }
    }
    
    public void ApplySpeedBoost(float multiplier, float duration)
    {
        // Stop any existing speed boost coroutine
        if (speedBoostCoroutine != null)
        {
            StopCoroutine(speedBoostCoroutine);
        }
        
        // Apply the speed boost
        hasSpeedBoost = true;
        currentSpeedMultiplier = multiplier;
        speedBoostTimeRemaining = duration;
        
        Debug.Log($"Speed boost applied! Multiplier: {multiplier}x for {duration}s");
        Debug.Log($"New speed: {GetCurrentMoveSpeed(false):F1} (was {baseMovementSpeed})");
        
        // Start visual effects
        if (showSpeedBoostGlow)
        {
            ApplySpeedBoostGlow();
        }
        
        // Start the boost coroutine for effects
        speedBoostCoroutine = StartCoroutine(SpeedBoostEffect(duration));
        
        // Update game manager status
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            int percentIncrease = Mathf.RoundToInt((multiplier - 1) * 100);
            gameManager.UpdateStatusMessage($"SPEED BOOST! +{percentIncrease}% speed for {duration:F0}s");
        }
    }
    
    void RemoveSpeedBoost()
    {
        hasSpeedBoost = false;
        currentSpeedMultiplier = 1f;
        speedBoostTimeRemaining = 0f;
        
        Debug.Log("Speed boost expired - returning to normal speed");
        
        // Remove visual effects
        RemoveSpeedBoostGlow();
        
        // Reset animation speed
        if (animator != null)
        {
            animator.speed = 1f;
        }
        
        // Update status
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.UpdateStatusMessage("Speed boost expired");
        }
    }
    
    void ApplySpeedBoostGlow()
    {
        if (playerRenderer != null && originalMaterial != null)
        {
            // Create glowing material
            speedBoostMaterial = new Material(originalMaterial);
            speedBoostMaterial.EnableKeyword("_EMISSION");
            speedBoostMaterial.SetColor("_EmissionColor", speedBoostGlowColor * 1.5f);
            playerRenderer.material = speedBoostMaterial;
            
            Debug.Log("Speed boost glow effect applied");
        }
    }
    
    void RemoveSpeedBoostGlow()
    {
        if (playerRenderer != null && originalMaterial != null)
        {
            playerRenderer.material = originalMaterial;
            
            if (speedBoostMaterial != null)
            {
                Destroy(speedBoostMaterial);
                speedBoostMaterial = null;
            }
            
            Debug.Log("Speed boost glow effect removed");
        }
    }
    
    System.Collections.IEnumerator SpeedBoostEffect(float duration)
    {
        float timer = 0f;
        
        while (timer < duration && hasSpeedBoost)
        {
            timer += Time.deltaTime;
            
            // Optional: Add pulsing glow effect
            if (speedBoostMaterial != null && hasSpeedBoost)
            {
                float pulse = (Mathf.Sin(Time.time * 4f) + 1f) * 0.5f;
                float glowIntensity = 1f + (pulse * 0.5f);
                speedBoostMaterial.SetColor("_EmissionColor", speedBoostGlowColor * glowIntensity);
            }
            
            yield return null;
        }
        
        // Effect ended naturally or was interrupted
        if (hasSpeedBoost)
        {
            RemoveSpeedBoost();
        }
    }
    
    // Get current movement speed (base speed * multiplier if boosted, or sprint if sprinting)
    float GetCurrentMoveSpeed(bool isSprinting = false)
    {
        float baseSpeed = hasSpeedBoost ? baseMovementSpeed * currentSpeedMultiplier : baseMovementSpeed;
        
        // If sprinting, use sprint speed (but still apply speed boost multiplier if active)
        if (isSprinting)
        {
            return hasSpeedBoost ? sprintSpeed * currentSpeedMultiplier : sprintSpeed;
        }
        
        return baseSpeed;
    }
    
    // Public getters for other systems
    public bool HasSpeedBoost()
    {
        return hasSpeedBoost;
    }
    
    public float GetSpeedBoostTimeRemaining()
    {
        return speedBoostTimeRemaining;
    }
    
    public float GetCurrentSpeedMultiplier()
    {
        return currentSpeedMultiplier;
    }
    
    public float GetCurrentSpeed()
    {
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        return GetCurrentMoveSpeed(isSprinting);
    }
    
    public bool IsSprinting()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }
    
    // Debug method - remove in final build
    void OnGUI()
    {
        bool isSprinting = IsSprinting();
        
        if (hasSpeedBoost || isSprinting)
        {
            int yOffset = 10;
            
            if (hasSpeedBoost)
            {
                GUI.color = Color.yellow;
                GUI.Label(new Rect(10, yOffset, 250, 20), $"SPEED BOOST: {speedBoostTimeRemaining:F1}s");
                yOffset += 20;
                GUI.Label(new Rect(10, yOffset, 250, 20), $"Boost Speed: {GetCurrentMoveSpeed(false):F1}");
                yOffset += 20;
            }
            
            if (isSprinting)
            {
                GUI.color = Color.cyan;
                GUI.Label(new Rect(10, yOffset, 250, 20), $"SPRINTING: {GetCurrentMoveSpeed(true):F1}");
                yOffset += 20;
                GUI.Label(new Rect(10, yOffset, 250, 20), "Hold SHIFT to sprint");
                yOffset += 20;
            }
            
            // Show total current speed
            GUI.color = Color.white;
            GUI.Label(new Rect(10, yOffset, 250, 20), $"Current Speed: {GetCurrentSpeed():F1}");
            
            GUI.color = Color.white;
        }
    }
}