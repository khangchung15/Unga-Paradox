using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Class which handles player movement (no gravity version)
/// </summary>
public class PlayerController : MonoBehaviour
{
    public SpriteRenderer spriteRenderer = null;
    public Camera playerCamera;
    public float movementSpeed = 4.0f;
    public InputAction moveAction;

    // Current movement velocity
    private Vector2 currentVelocity = Vector2.zero;

    public int damage;
    public float health;

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
    }

    void OnDisable()
    {
        moveAction.Disable();
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
        if (health != null && health <= 0)
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

    // Detect if player is colliding with enemy
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy")
        {
            collision.GetComponent<Enemy>().TakeDamage(damage);
        }
    }
    
    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}