using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float mouseSensitivity = 2.0f;
    private CharacterController controller;
    private Camera playerCamera;
    private float verticalRotation = 0;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    void Update()
    {
        // Mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        transform.Rotate(0, mouseX, 0);
        
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90, 90);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        
        // Movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 movement = transform.TransformDirection(new Vector3(horizontal, 0, vertical));
        movement = movement.normalized * moveSpeed * Time.deltaTime;
        
        controller.Move(movement);
    }
}