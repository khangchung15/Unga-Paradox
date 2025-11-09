using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerCutsceneController : MonoBehaviour
{
    [Header("Player Components")]
    public MonoBehaviour playerMovementScript; // Reference to your movement script
    public Rigidbody2D playerRigidbody;
    
    [Header("Timeline References")]
    public PlayableDirector timelineDirector;
    
    [Header("Post-Cutscene Settings")]
    public Transform targetPosition; // Where to move player after cutscene
    public bool disableMovementDuringCutscene = true;
    
    [Header("Player Control")]
    public bool hidePlayerDuringCutscene = false;
    
    [Header("Objects to Destroy")]
    public GameObject[] objectsToDestroy; // Multiple objects that will be destroyed when cutscene finishes
    public bool destroyImmediately = true;
    
    private Vector3 originalPosition;
    private bool wasKinematic;
    private ScientistController scientistController;
    private ScientistAnimator scientistAnimator;
    private InteractionDetector interactionDetector;
    private GameObject playerObject;

    void Start()
    {
        playerObject = gameObject;
        
        // Find player components
        scientistController = GetComponent<ScientistController>();
        scientistAnimator = GetComponent<ScientistAnimator>();
        interactionDetector = FindObjectOfType<InteractionDetector>();
        
        // Store original state
        if (playerRigidbody != null)
        {
            wasKinematic = playerRigidbody.isKinematic;
        }
        
        // Subscribe to timeline events
        if (timelineDirector != null)
        {
            timelineDirector.played += OnTimelineStarted;
            timelineDirector.stopped += OnTimelineFinished;
        }
    }
    
    void OnTimelineStarted(PlayableDirector director)
    {
        StartCutscene();
    }
    
    void OnTimelineFinished(PlayableDirector director)
    {
        EndCutscene();
    }
    
    public void StartCutscene()
    {
        // Store original position
        originalPosition = transform.position;
        
        // Disable player movement using the same method as CutsceneTrigger
        DisablePlayerMovement();
        
        // Stop player physics
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.isKinematic = true;
        }
        
        // Hide player if configured
        if (hidePlayerDuringCutscene && playerObject != null)
        {
            playerObject.SetActive(false);
        }
        
        Debug.Log("Cutscene started - Player movement disabled");
    }
    
    public void EndCutscene()
    {
        // Move player to target position
        if (targetPosition != null)
        {
            transform.position = targetPosition.position;
        }
        
        // Re-enable player movement using the same method as CutsceneTrigger
        EnablePlayerMovement();
        
        // Restore physics
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = wasKinematic;
        }
        
        // Show player if it was hidden
        if (hidePlayerDuringCutscene && playerObject != null)
        {
            playerObject.SetActive(true);
        }
        
        // Destroy all objects in the array
        DestroyObjects();
        
        Debug.Log("Cutscene finished - Player movement enabled");
    }
    
    private void DestroyObjects()
    {
        if (objectsToDestroy != null && objectsToDestroy.Length > 0)
        {
            foreach (GameObject obj in objectsToDestroy)
            {
                if (obj != null)
                {
                    if (destroyImmediately)
                    {
                        Destroy(obj);
                    }
                    else
                    {
                        Destroy(obj, 0.1f); // Small delay if needed
                    }
                    Debug.Log("Destroyed object: " + obj.name);
                }
            }
        }
    }
    
    private void DisablePlayerMovement()
    {
        // Disable movement input - same as CutsceneTrigger
        if (scientistController != null)
        {
            scientistController.moveAction.Disable();
        }
        
        // Set animation to idle - same as CutsceneTrigger
        if (scientistAnimator != null && scientistAnimator.animator != null)
        {
            scientistAnimator.animator.SetBool("isIdle", true);
            scientistAnimator.animator.SetBool("isRunning", false);
        }
        
        // Disable interaction detector - same as CutsceneTrigger
        if (interactionDetector != null)
        {
            interactionDetector.enabled = false;
            if (interactionDetector.interactionIcon != null)
                interactionDetector.interactionIcon.SetActive(false);
        }
        
        // Also disable the generic movement script if it exists
        if (playerMovementScript != null && disableMovementDuringCutscene)
        {
            playerMovementScript.enabled = false;
        }
    }
    
    private void EnablePlayerMovement()
    {
        // Re-enable movement input - same as CutsceneTrigger
        if (scientistController != null)
        {
            scientistController.moveAction.Enable();
        }
        
        // Re-enable interaction detector - same as CutsceneTrigger
        if (interactionDetector != null)
        {
            interactionDetector.enabled = true;
        }
        
        // Re-enable the generic movement script if it exists
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }
    }
    
    // Call this method from Timeline using Signal Receiver
    public void OnCutsceneSignal()
    {
        StartCutscene();
    }
    
    // Call this method from Timeline using Signal Receiver
    public void OnCutsceneEndSignal()
    {
        EndCutscene();
    }
    
    // Public method to add a single object to destroy
    public void AddObjectToDestroy(GameObject obj)
    {
        if (obj != null)
        {
            List<GameObject> objectList = new List<GameObject>();
            if (objectsToDestroy != null)
            {
                objectList.AddRange(objectsToDestroy);
            }
            
            if (!objectList.Contains(obj))
            {
                objectList.Add(obj);
                objectsToDestroy = objectList.ToArray();
            }
        }
    }
    
    // Public method to add multiple objects to destroy
    public void AddObjectsToDestroy(GameObject[] objs)
    {
        if (objs != null && objs.Length > 0)
        {
            List<GameObject> objectList = new List<GameObject>();
            if (objectsToDestroy != null)
            {
                objectList.AddRange(objectsToDestroy);
            }
            
            foreach (GameObject obj in objs)
            {
                if (obj != null && !objectList.Contains(obj))
                {
                    objectList.Add(obj);
                }
            }
            
            objectsToDestroy = objectList.ToArray();
        }
    }
    
    // Public method to set all objects to destroy (replaces existing array)
    public void SetObjectsToDestroy(GameObject[] objs)
    {
        objectsToDestroy = objs;
    }
    
    // Public method to clear all objects to destroy
    public void ClearObjectsToDestroy()
    {
        objectsToDestroy = new GameObject[0];
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (timelineDirector != null)
        {
            timelineDirector.played -= OnTimelineStarted;
            timelineDirector.stopped -= OnTimelineFinished;
        }
    }
}