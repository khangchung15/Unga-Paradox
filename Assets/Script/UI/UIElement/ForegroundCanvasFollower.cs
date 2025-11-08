using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ForegroundCanvasFollower : MonoBehaviour
{
    [Header("Exit Movement Settings")]
    public float exitMoveDuration = 2f;
    public float exitMoveDistance = 15f;
    public AnimationCurve exitCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Trigger Settings")]
    public bool moveOnStart = false;
    public Key debugKey = Key.Space;
    
    private Vector3 originalPosition;
    private bool hasExited = false;
    private bool isMoving = false;
    private Keyboard keyboard;

    void Start()
    {
        // Store original position
        originalPosition = transform.position;
        
        // Get keyboard reference
        keyboard = Keyboard.current;
        
        // Auto-start if configured
        if (moveOnStart)
        {
            StartExitMovement();
        }
    }

    void Update()
    {
        // Debug trigger using new Input System
        if (keyboard != null && keyboard[debugKey].wasPressedThisFrame && !hasExited && !isMoving)
        {
            StartExitMovement();
        }
    }

    /// <summary>
    /// Call this when the cutscene ends and player control begins
    /// </summary>
    public void StartExitMovement()
    {
        if (!hasExited && !isMoving)
        {
            StartCoroutine(ExitMovementRoutine());
        }
    }

    IEnumerator ExitMovementRoutine()
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + Vector3.right * exitMoveDistance;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < exitMoveDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / exitMoveDuration;
            float curvedProgress = exitCurve.Evaluate(progress);
            
            transform.position = Vector3.Lerp(startPosition, targetPosition, curvedProgress);
            
            yield return null;
        }
        
        // Ensure exact final position
        transform.position = targetPosition;
        isMoving = false;
        hasExited = true;
        
    }

    /// <summary>
    /// Reset the canvas to its original position
    /// </summary>
    public void ResetPosition()
    {
        StopAllCoroutines();
        transform.position = originalPosition;
        hasExited = false;
        isMoving = false;
    }

    /// <summary>
    /// Instantly move the canvas out of view
    /// </summary>
    public void InstantExit()
    {
        StopAllCoroutines();
        transform.position = originalPosition + Vector3.right * exitMoveDistance;
        hasExited = true;
        isMoving = false;
    }

    // Public properties to check state
    public bool HasExited { get { return hasExited; } }
    public bool IsMoving { get { return isMoving; } }
}