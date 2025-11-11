using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Class which handles player movement (no gravity version)
/// </summary>
public class PlayerController : Singleton<PlayerController>
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



    // Movement slow effect



    private int dashBlockers = 0;
    private bool canDash = true;
    private float baseMovementSpeed;
    private float currentSlowMultiplier = 1f;
    private Stack<float> slowStack = new();

    private readonly Dictionary<string, float> speedMods = new Dictionary<string, float>();

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

    protected override void Awake()
    {
        base.Awake();
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
        baseMovementSpeed = movementSpeed;

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
        if (!isDashing && canDash)
        {
            isDashing = true;
            myTrailRenderer.emitting = true;
            RecomputeMovementSpeed();
            StartCoroutine(EndDashRoutine());
        }
    }

    private IEnumerator EndDashRoutine()
    {
        float dashTime = 0.2f;
        float dashCD = 0.25f;
        GetComponent<Health>().isDashing = true;
        isDashing = false;
        yield return new WaitForSeconds(dashTime);
        myTrailRenderer.emitting = false;
        RecomputeMovementSpeed();
        yield return new WaitForSeconds(dashCD);
        canDash = (dashBlockers == 0);
    }

    public void AddOrUpdateSpeedMod(string key, float multiplier)
    {
        multiplier = Mathf.Clamp(multiplier, 0.05f, 1f);
        speedMods[key] = multiplier;
        UpdateSlowFromMods();
    }

    private void UpdateSlowFromMods()
    {
        float m = 1f;
        foreach (var v in speedMods.Values) m = Mathf.Min(m, v);
        currentSlowMultiplier = m;
        RecomputeMovementSpeed();
    }
    public void RemoveSpeedMod(string key)
    {
        if (speedMods.Remove(key))
            UpdateSlowFromMods();
    }

    private void RecomputeMovementSpeed()
    {
        float dashMul = isDashing ? dashSpeed : 1f;
        movementSpeed = baseMovementSpeed * currentSlowMultiplier * dashMul;
        Debug.Log($"[Speed] base={baseMovementSpeed}, slowMul={currentSlowMultiplier}, dash={(isDashing ? dashSpeed : 1f)}, active={movementSpeed}");
    }

    public void CancelDash()
    {
        if (!isDashing) return;
        isDashing = false;
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;
        myTrailRenderer.emitting = false;
        RecomputeMovementSpeed();
    }

    public void ApplySpeedModifier(float speedModifier)
    {
        float m = Mathf.Clamp(speedModifier, 0.05f, 5f);
        slowStack.Push(m);
        RecomputeMovementSpeed();
    }

    public void RemoveSpeedModifier()
    {
        if (slowStack.Count > 0)
            slowStack.Pop();
        RecomputeMovementSpeed();
    }

    public void AddDashBlock()
    {
        dashBlockers++;
        UpdateDashPermission();
        if (isDashing) CancelDash();
    }

    public void RemoveDashBlock()
    {
        dashBlockers = Mathf.Max(0, dashBlockers - 1);
        UpdateDashPermission();
    }

    public void UpdateDashPermission()
    {
        canDash = (dashBlockers == 0);
    }

}