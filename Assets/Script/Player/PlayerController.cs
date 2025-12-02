using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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

    [Header("Trail Cosmetic Settings")]
    [Tooltip("If true, the player's trail will use a rainbow gradient.")]
    [SerializeField] private bool useRainbowTrail = false;

    private Gradient _defaultTrailGradient;


    [Header("Movement Settings")]
    [Tooltip("The speed at which to move the player")]
    [SerializeField] private float movementSpeed = 4.0f;
    [Tooltip("The speed at which to dash the player")]
    [SerializeField] private float dashSpeed = 4.0f;

    private PlayerControls playerControls;

    // Current movement velocity
    private Vector2 currentVelocity = Vector2.zero;

    private int dashBlockers = 0;
    private bool canDash = true;
    private float baseMovementSpeed;
    private float currentSlowMultiplier = 1f;
    private Stack<float> slowStack = new();

    private readonly Dictionary<string, float> speedMods = new Dictionary<string, float>();

    private bool isDashing = false;

    [Header("Parry Settings")]
    [SerializeField] private GameObject parryVisual;
    [SerializeField] private float parryDuration = 0.2f;
    [SerializeField] private float parryCooldown = 0.5f;
    [SerializeField] private AudioSource parryAudioSource;
    [SerializeField] private AudioClip parryStartSFX;
    [SerializeField] private AudioClip parryReadySFX;
    [SerializeField] private float parryReadyVolume = 1f;
    private bool isParrying = false;
    private bool canParry = true;
    private Coroutine parryRoutine;

    // Just to remove the one component from this level since the player gets carried over into different scenes.
    [Header("Scene-Specific Spotlight")]
    [SerializeField] private string spotlightSceneName = "L1_Room02";
    [SerializeField] private GameObject playerSpotlight;

    private Rigidbody2D rb;

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
        rb = GetComponent<Rigidbody2D>();

        if (myTrailRenderer != null)
        {
            _defaultTrailGradient = myTrailRenderer.colorGradient;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    public PlayerDirection facing
    {
        get
        {
            if (playerCamera == null)
            {
                RefreshCameraReference();
                if (playerCamera == null)
                {
                    // Fallback: face based on velocity or default to right
                    if (currentVelocity.x < -0.1f) return PlayerDirection.Left;
                    return PlayerDirection.Right;
                }
            }

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
        }
    }

    private void Start()
    {
        baseMovementSpeed = movementSpeed;
        RefreshCameraReference();

        if (useRainbowTrail)
        {
            ApplyRainbowTrail();
        }

        if (parryVisual != null)
        {
            Debug.Log($"[Parry] ParryVisual assigned to: {parryVisual.name}");
            parryVisual.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[Parry] ParryVisual is NULL in PlayerController!");
        }
        

        playerControls.Player.Dash.performed += _ => Dash();
        playerControls.Player.Parry.performed += _ => TryParry();
    }

    private void OnEnable()
    {
        playerControls.Enable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (playerControls != null)
        {
            playerControls.Disable();
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        playerCamera = Camera.main;

        // For that one scene where the player has a spotlight. Hardcoded since its the only scene where a player has an added component like that
        if (playerSpotlight != null)
        {
            if (scene.name != spotlightSceneName)
            {
                Destroy(playerSpotlight);
                playerSpotlight = null;
            }
        }
    }

    private void Update()
    {
        if (playerCamera == null)
        {
            RefreshCameraReference();
        }
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
        if (rb == null)
        {
            transform.position += (Vector3)currentVelocity * Time.deltaTime;
            return;
        }

        Vector2 newPos = rb.position + currentVelocity * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
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
        if (playerCamera == null)
        {
            RefreshCameraReference();
            if (playerCamera == null)
                return;
        }

        Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, playerCamera.transform.position.z);
        playerCamera.transform.position = targetPosition;
    }

    private void RefreshCameraReference()
    {
        playerCamera = Camera.main;

        if (playerCamera == null)
        {
            Debug.LogWarning("PlayerController: No main camera found in the current scene!");
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

    private void TryParry()
    {
        if (!canParry || state == PlayerState.Dead)
            return;

        if (parryRoutine != null)
        {
            StopCoroutine(parryRoutine);
        }

        parryRoutine = StartCoroutine(ParryRoutine());

        if (parryAudioSource != null && parryStartSFX != null)
        {
            parryAudioSource.PlayOneShot(parryStartSFX);
        }
        else
        {
            Debug.LogWarning($"[Parry] Start SFX NOT played. parryAudioSource null? {parryAudioSource == null}, parryStartSFX null? {parryStartSFX == null}");
        }
    }

    private IEnumerator ParryRoutine()
    {
        canParry = false;
        isParrying = true;

        if (playerHealth != null)
        {
            playerHealth.isParrying = true;
            Debug.Log("[Parry] playerHealth.isParrying = true");
        }

        if (parryVisual != null)
        {
            parryVisual.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[Parry] parryVisual is NULL when trying to activate!");
        }

        yield return new WaitForSeconds(parryDuration);

        isParrying = false;

        if (playerHealth != null)
        {
            playerHealth.isParrying = false;
        }

        if (parryVisual != null)
        {
            parryVisual.SetActive(false);
        }

        yield return new WaitForSeconds(parryCooldown);

        canParry = true;
        parryRoutine = null;
        
        if (parryReadySFX != null)
        {
            Vector3 soundPos = playerCamera != null ? playerCamera.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(parryReadySFX, soundPos, parryReadyVolume);
        }
        else
        {
            Debug.LogWarning("[Parry] Ready SFX NOT played. parryReadySFX is NULL.");
        }

        yield break;
    }

    private IEnumerator EndDashRoutine()
    {
        float dashTime = 0.2f;
        float dashCD = 0.25f;

        GetComponent<Health>().isDashing = true;
        isDashing = true;
        RecomputeMovementSpeed();

        yield return new WaitForSeconds(dashTime);

        GetComponent<Health>().isDashing = false;
        isDashing = false;
        myTrailRenderer.emitting = false;
        RecomputeMovementSpeed();

        canDash = false;
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
    }

    public void CancelDash()
    {
        if (!isDashing) return;
        isDashing = false;
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
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

    public void SetBaseMovementSpeed(float newBaseSpeed)
    {
        if (newBaseSpeed <= 0f)
        {
            Debug.LogWarning($"PlayerController: Attempted to set non-positive baseMovementSpeed ({newBaseSpeed}) on {gameObject.name}.");
            return;
        }

        baseMovementSpeed = newBaseSpeed;
        RecomputeMovementSpeed();
    }

    public float GetBaseMovementSpeed()
    {
        return baseMovementSpeed;
    }

    public void EnableRainbowTrail()
    {
        useRainbowTrail = true;
        ApplyRainbowTrail();
    }

    public void DisableRainbowTrail()
    {
        useRainbowTrail = false;

        if (myTrailRenderer != null && _defaultTrailGradient != null)
        {
            myTrailRenderer.colorGradient = _defaultTrailGradient;
        }
    }

    private void ApplyRainbowTrail()
    {
        if (myTrailRenderer == null)
            return;

        Gradient rainbow = new Gradient();

        var colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(Color.red, 0f),
            new GradientColorKey(Color.yellow, 0.2f),
            new GradientColorKey(Color.green, 0.4f),
            new GradientColorKey(Color.cyan, 0.6f),
            new GradientColorKey(Color.blue, 0.8f),
            new GradientColorKey(Color.magenta, 1f)
        };

        var alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(0f, 1f)
        };

        rainbow.SetKeys(colorKeys, alphaKeys);
        myTrailRenderer.colorGradient = rainbow;
    }

    public void SetParryCooldown(float newCooldown)
    {
        // Prevent zero/negative cooldowns
        parryCooldown = Mathf.Max(0.05f, newCooldown);
    }

    public float GetParryCooldown()
    {
        return parryCooldown;
    }
}