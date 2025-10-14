using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Class which handles player movement (no gravity version)
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Game Object and Component References")]
    [Tooltip("The sprite renderer that represents the player.")]
    public SpriteRenderer spriteRenderer = null;
    [Tooltip("The health component attached to the player.")]
    public Health playerHealth;
    [Tooltip("The camera that will follow the player.")]
    public Camera playerCamera;
    [SerializeField] private TrailRenderer myTrailRenderer;


    [Header("Movement Settings")]
    [Tooltip("The speed at which to move the player")]
    [SerializeField] private float movementSpeed = 4.0f;
    [Tooltip("The speed at which to dash the player")]
    [SerializeField] private float dashSpeed = 4.0f;

    //[Header("Input Actions & Controls")]
    //[Tooltip("The input action(s) that map to player movement")]
    //public InputAction moveAction;

    private PlayerControls playerControls;

    // Current movement velocity
    private Vector2 currentVelocity = Vector2.zero;

    private bool isDashing = false;

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

    #endregion

    private void Awake()
    {
        playerControls = new PlayerControls();
    }


    public PlayerDirection facing
    {
        get
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 playerScreenPoint = playerCamera.WorldToScreenPoint(transform.position);
            if (mousePos.x > playerScreenPoint.x)
            {
                return PlayerDirection.Right;
            }
            else if (mousePos.x < playerScreenPoint.x)
            {
                return PlayerDirection.Left;
            }
            else
            {
                if (spriteRenderer != null && spriteRenderer.flipX == true)
                    return PlayerDirection.Left;
                return PlayerDirection.Right;
            }
            //if (currentVelocity.x > 0.1f)
            //{
            //    return PlayerDirection.Right;
            //}
            //else if (currentVelocity.x < -0.1f)
            //{
            //    return PlayerDirection.Left;
            //}
            //else
            //{
            //    if (spriteRenderer != null && spriteRenderer.flipX == true)
            //        return PlayerDirection.Left;
            //    return PlayerDirection.Right;
            //}
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

        playerControls.Player.Dash.performed += _ => Dash();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
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
        Vector2 input = playerControls.Player.Move.ReadValue<Vector2>();
        
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

    private void Dash() 
    {
        if (!isDashing)
        {
            isDashing = true;
            movementSpeed *= dashSpeed;
            myTrailRenderer.emitting = true;
            StartCoroutine(EndDashRoutine());
        }
    }

    private IEnumerator EndDashRoutine()
    {
        float dashTime = 0.2f;
        float dashCD = 0.25f;
        yield return new WaitForSeconds(dashTime);
        movementSpeed /= dashSpeed;
        myTrailRenderer.emitting = false;
        yield return new WaitForSeconds(dashCD);
        isDashing = false;
    }
}