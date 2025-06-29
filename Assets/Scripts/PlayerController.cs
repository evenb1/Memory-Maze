using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    
    [Header("Simple Camera")]
    public float mouseSensitivity = 2f;
    
    private CharacterController controller;
    private Animator animator;
    private Camera playerCamera;
    private float cameraRotationY = 0f;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerCamera = Camera.main;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
    }
    
    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
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
            // Move character
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
            
            // Rotate character to face movement direction
            transform.rotation = Quaternion.Slerp(transform.rotation, 
                Quaternion.LookRotation(moveDirection), rotationSpeed * Time.deltaTime);
            
            if (animator != null)
            {
                animator.SetFloat("Blend", 0.5f); // Walking animation
            }
        }
        else
        {
            if (animator != null)
            {
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
}