using System.Collections;
using UnityEngine;

public class TeleportInteractable : MonoBehaviour, IInteractable
{
    [Header("Teleport Destination")]
    [Tooltip("The target position to teleport to. If empty, will use the target object's position")]
    public Transform targetLocation;
    
    [Header("Teleport Options")]
    [Tooltip("If true, teleports to a specific GameObject's position. If false, uses exact coordinates")]
    public bool teleportToObject = true;
    
    [Tooltip("Exact position to teleport to (only used if teleportToObject is false)")]
    public Vector3 exactPosition;
    
    [Header("Teleport Effects")]
    public bool useTeleportEffect = true;
    public float teleportDelay = 0.5f;
    
    [Header("One-time Use")]
    public bool oneTimeUse = false;
    public bool destroyAfterUse = false;
    
    private bool hasBeenUsed = false;
    private GameObject player;
    private InteractionDetector interactionDetector;
    private ScientistController playerController;
    private ScientistAnimator playerAnimator;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        
        
        // If teleporting to object but no target specified, use self
        if (teleportToObject && targetLocation == null)
        {
            targetLocation = transform;
        }
    }
    
    public bool CanInteract()
    {
        // If one-time use and already used, can't interact again
        if (oneTimeUse && hasBeenUsed)
        {
            return false;
        }
        return true;
    }
    
    public void Interact()
    {
        
        if (oneTimeUse && hasBeenUsed)
        {
            return;
        }
        
        
        StartCoroutine(TeleportPlayer());
    }

    private IEnumerator TeleportPlayer()
    {
        
        // Mark as used if one-time
        if (oneTimeUse)
        {
            hasBeenUsed = true;
        }
        
        // Disable player movement during teleport
        FindAndDisablePlayerMovement();
        
        // Optional teleport effects/delay
        if (useTeleportEffect && teleportDelay > 0)
        {
            yield return new WaitForSeconds(teleportDelay);
        }
        
        // Calculate target position
        Vector3 targetPosition = teleportToObject ? targetLocation.position : exactPosition;
        
        // Perform teleport
        player.transform.position = targetPosition;
        
        // Small delay before re-enabling to ensure teleport completes
        yield return null;
        
        // Re-enable player movement after teleport
        EnablePlayerMovement();
        
        // Destroy if configured to do so
        if (destroyAfterUse)
        {
            Destroy(gameObject);
        }
    }
    
    // Same player movement control as NPC
    private void FindAndDisablePlayerMovement()
    {
        // Find the player controller component
        playerController = FindObjectOfType<ScientistController>();
        if (playerController != null)
        {
            // Disable the moveAction to prevent movement input
            playerController.moveAction.Disable();
        }

        // Find the player animator to potentially freeze animations
        playerAnimator = FindObjectOfType<ScientistAnimator>();
        if (playerAnimator != null && playerAnimator.animator != null)
        {
            // Set to idle animation during teleport
            playerAnimator.animator.SetBool("isIdle", true);
            playerAnimator.animator.SetBool("isRunning", false);
        }

        // Find and disable the interaction detector to prevent multiple interactions
        interactionDetector = FindObjectOfType<InteractionDetector>();
        if (interactionDetector != null)
        {
            interactionDetector.enabled = false;
            // Also hide the interaction icon if it exists
            if (interactionDetector.interactionIcon != null)
                interactionDetector.interactionIcon.SetActive(false);
        }
    }

    private void EnablePlayerMovement()
    {
        // Re-enable player movement
        if (playerController != null)
        {
            playerController.moveAction.Enable();
        }

        // Re-enable interaction detector
        if (interactionDetector != null)
        {
            interactionDetector.enabled = true;
        }
    }
    
    // Optional: Visual feedback in editor
    void OnDrawGizmosSelected()
    {
        if (teleportToObject && targetLocation != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetLocation.position);
            Gizmos.DrawWireCube(targetLocation.position, Vector3.one * 0.5f);
        }
        else if (!teleportToObject)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(exactPosition, Vector3.one * 0.5f);
            Gizmos.DrawLine(transform.position, exactPosition);
        }
    }
}