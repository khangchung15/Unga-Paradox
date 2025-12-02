using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScientistController : MonoBehaviour
{
    [Header("Game Object and Component References")]
    [Tooltip("The sprite renderer that represents the player.")]
    public SpriteRenderer spriteRenderer = null;
    [Tooltip("The health component attached to the player.")]
    public Health playerHealth;
    [Tooltip("The HealthFuture component attached to the player.")]
    public HealthFuture playerHealthFuture;
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
                if (transform.rotation.eulerAngles.y == 180f)
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
        Vector2 direction = facing == PlayerDirection.Right ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1.5f);
    }

    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerHealthFuture == null)
        {
            playerHealthFuture = GetComponent<HealthFuture>();
        }

        if (playerHealth == null)
        {
            playerHealth = GetComponent<Health>();
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
        if (state != PlayerState.Dead)
        {
            transform.position += (Vector3)currentVelocity * Time.deltaTime;
        }
    }

    private void UpdateCameraPosition()
    {
        if (playerCamera != null)
        {
            Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, playerCamera.transform.position.z);
            playerCamera.transform.position = targetPosition;
        }
    }

    private void HandleSpriteDirection()
    {
        if (state == PlayerState.Dead) return;

        if (currentVelocity.x > 0.1f)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else if (currentVelocity.x < -0.1f)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }

    private void DetermineState()
    {
        bool isDead = false;

        if (playerHealthFuture != null)
        {
            isDead = playerHealthFuture.currentHealth <= 0;
        }
        else if (playerHealth != null)
        {
            isDead = playerHealth.currentHealth <= 0;
        }

        if (isDead)
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
