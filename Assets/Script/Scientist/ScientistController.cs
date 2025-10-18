using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Class which handles player movement (no gravity version)
/// </summary>
public class ScientistController : MonoBehaviour
{
    [Header("Game Object and Component References")]
    [Tooltip("The sprite renderer that represents the player.")]
    public SpriteRenderer spriteRenderer = null;
    [Tooltip("The health component attached to the player.")]
    public Health playerHealth;
    [Tooltip("The camera that will follow the player.")]
    public Camera playerCamera;

    [Header("Movement Settings")]
    [Tooltip("The speed at which to move the player")]
    public float movementSpeed = 4.0f;

    [Header("Input Actions & Controls")]
    [Tooltip("The input action(s) that map to player movement")]
    public InputAction moveAction;
    [Tooltip("The input action for interaction")]
    public InputAction interactAction;

    // Current movement velocity
    private Vector2 currentVelocity = Vector2.zero;

    #region Player State Variables
    public enum PlayerState
    {
        Idle,
        Walk,
        Dead
    }

    public PlayerState state = PlayerState.Idle;
    #endregion

    #region Directional facing
    public enum PlayerDirection
    {
        Right,
        Left
    }

    public PlayerDirection facing
    {
        get
        {
            if (currentVelocity.x > 0.1f)
            {
                return PlayerDirection.Right;
            }
            else if (currentVelocity.x < -0.1f)
            {
                return PlayerDirection.Left;
            }
            else
            {
                if (spriteRenderer != null && spriteRenderer.flipX == true)
                    return PlayerDirection.Left;
                return PlayerDirection.Right;
            }
        }
    }
    #endregion

    void OnEnable()
    {
        moveAction.Enable();
        interactAction.Enable();
        interactAction.performed += OnInteract;
    }

    void OnDisable()
    {
        moveAction.Disable();
        interactAction.Disable();
        interactAction.performed -= OnInteract;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PerformInteraction();
        }
    }

    private void PerformInteraction()
    {
        // Your interaction logic here
        Debug.Log("Player interacted!");
        
        // Example: Check for interactable objects in front of player
        Vector2 direction = facing == PlayerDirection.Right ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1.5f);
        
        if (hit.collider != null)
        {
            Debug.Log($"Hit: {hit.collider.gameObject.name}");
            // You can add logic here to interact with specific objects
        }
    }

    private void Start()
    {
        // If no camera is assigned, try to find the main camera
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // Ensure the camera exists
        if (playerCamera == null)
        {
            Debug.LogWarning("No camera assigned to player controller and no main camera found in scene!");
        }
    }

    private void Update()
    {
        ProcessInput();
        HandleSpriteDirection();
        DetermineState();
        MovePlayer();
        UpdateCameraPosition();
    }

    private void ProcessInput()
    {
        HandleMovementInput();
    }

    private void HandleMovementInput()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        
        if (state != PlayerState.Dead)
        {
            currentVelocity = input * movementSpeed;
        }
        else
        {
            currentVelocity = Vector2.zero;
        }
    }

    private void MovePlayer()
    {
        transform.position += (Vector3)currentVelocity * Time.deltaTime;
    }

    /// <summary>
    /// Description:
    /// Updates the camera position to follow the player
    /// Inputs: 
    /// none
    /// Returns: 
    /// void (no return)
    /// </summary>
    private void UpdateCameraPosition()
    {
        if (playerCamera != null)
        {
            // Keep the camera's Z position unchanged (maintain camera distance)
            Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, playerCamera.transform.position.z);
            playerCamera.transform.position = targetPosition;
        }
    }

    private void HandleSpriteDirection()
    {
        if (spriteRenderer != null)
        {
            if (facing == PlayerDirection.Left)
            {
                spriteRenderer.flipX = true;
            }
            else
            {
                spriteRenderer.flipX = false;
            }
        }
    }

    private void DetermineState()
    {
        if (playerHealth != null && playerHealth.currentHealth <= 0)
        {
            state = PlayerState.Dead;
        }
        else if (currentVelocity.magnitude > 0.1f)
        {
            state = PlayerState.Walk;
        }
        else
        {
            state = PlayerState.Idle;
        }
    }
}