using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CutsceneTrigger : MonoBehaviour, IInteractable
{
    [Header("Cutscene References")]
    [SerializeField] private PlayableDirector timeline;
    [SerializeField] private string playerTag = "Player";
    
    [Header("Activation Settings")]
    [SerializeField] private bool requireInteraction = false;
    [SerializeField] private bool playOnce = true;
    [SerializeField] private bool destroyAfterUse = false;
    
    [Header("Interaction Visuals")]
    [SerializeField] private GameObject interactionIcon;
    
    [Header("Player Control")]
    [SerializeField] private bool hidePlayerDuringCutscene = true;
    [SerializeField] private bool lockPlayerPosition = true;
    
    [Header("Music Settings")]
    [SerializeField] private bool playMusicDuringCutscene = false;
    [SerializeField] private AudioClip cutsceneMusic;
    [SerializeField] private bool loopMusic = true;
    [SerializeField] private float musicFadeInTime = 1f;
    [SerializeField] private float musicFadeOutTime = 1f;
    [SerializeField] private bool stopCurrentMusic = true;
    [SerializeField] private bool resumePreviousMusic = true;
    
    [Header("Scene Transition")]
    [SerializeField] private bool transitionToNewScene = false;
    [SerializeField] private string targetSceneName = "";
    [SerializeField] private float sceneTransitionDelay = 0f;
    
    [Header("Objects to Destroy After Cutscene")]
    [Tooltip("GameObjects that will be destroyed when cutscene ends")]
    public GameObject[] objectsToDestroy;
    
    [Header("NPC Dialogue After Cutscene")]
    [SerializeField] private bool triggerNPCDialogueAfterCutscene = false;
    [SerializeField] private NPC npcToTrigger;
    [SerializeField] private float dialogueStartDelay = 0.5f;
    
    private bool hasBeenTriggered = false;
    private bool hasBeenUsed = false;
    private GameObject player;
    private InteractionDetector interactionDetector;
    private ScientistController playerController;
    private ScientistAnimator playerAnimator;
    private Collider2D triggerCollider;
    private AudioSource musicAudioSource;
    private AudioClip previousMusic;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag(playerTag);
        triggerCollider = GetComponent<Collider2D>();
        
        // Get or create AudioSource for music
        if (playMusicDuringCutscene)
        {
            musicAudioSource = GetComponent<AudioSource>();
            if (musicAudioSource == null)
            {
                musicAudioSource = gameObject.AddComponent<AudioSource>();
                musicAudioSource.playOnAwake = false;
                musicAudioSource.loop = loopMusic; // Set loop based on option
            }
        }
        
        if (interactionIcon != null)
            interactionIcon.SetActive(false);
            
        Debug.Log($"CutsceneTrigger initialized on {gameObject.name}. RequireInteraction: {requireInteraction}");
    }

    // Automatic trigger when entering region (if interaction not required)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log($"Player entered cutscene trigger: {gameObject.name}");
            
            if (!requireInteraction)
            {
                if (!(playOnce && hasBeenTriggered))
                {
                    TriggerCutscene();
                }
            }
            else
            {
                // For interaction mode, just update the interaction icon
                UpdateInteractionIcon();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (requireInteraction && other.CompareTag(playerTag))
        {
            Debug.Log($"Player exited cutscene trigger: {gameObject.name}");
            UpdateInteractionIcon();
        }
    }

    private void UpdateInteractionIcon()
    {
        if (interactionIcon != null)
        {
            bool shouldShow = requireInteraction && CanInteract();
            interactionIcon.SetActive(shouldShow);
            Debug.Log($"Interaction icon: {shouldShow}");
        }
    }

    // IInteractable implementation - SIMPLIFIED like TeleportInteractable
    public bool CanInteract()
    {
        // If one-time use and already used, can't interact again
        if (playOnce && hasBeenUsed)
        {
            return false;
        }
        
        // Always return true if not requiring interaction (for detection purposes)
        // The actual trigger logic is handled in OnTriggerEnter2D for auto-trigger
        return requireInteraction;
    }
    
    public void Interact()
    {
        if (player == null)
        {
            Debug.LogError("Player not found for cutscene!");
            return;
        }
        
        if (playOnce && hasBeenUsed)
        {
            Debug.Log("Cutscene already used and is one-time only");
            return;
        }
        
        Debug.Log("CutsceneTrigger.Interact() called!");
        TriggerCutscene();
    }

    private void TriggerCutscene()
    {
        if (playOnce && hasBeenUsed)
            return;

        Debug.Log($"Starting cutscene: {gameObject.name}");
        
        // Mark as used if one-time
        if (playOnce)
        {
            hasBeenUsed = true;
            hasBeenTriggered = true;
        }
        
        // Start music before disabling player (in case music affects player state)
        if (playMusicDuringCutscene)
        {
            StartCutsceneMusic();
        }
        
        // Disable player during cutscene (using same method as NPC)
        FindAndDisablePlayerMovement();
        
        // Play the timeline
        timeline.Play();
        
        // Hide interaction icon
        if (interactionIcon != null)
            interactionIcon.SetActive(false);
        
        // Disable trigger if play once
        if (playOnce && triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }
        
        // Listen for when the cutscene ends
        StartCoroutine(WaitForCutsceneToEnd());
    }

    private IEnumerator WaitForCutsceneToEnd()
    {
        Debug.Log($"=== WAIT FOR CUTSCENE TO END STARTED ===");
        
        // Wait for the timeline to finish playing
        Debug.Log($"Waiting for timeline to finish... Current state: {timeline.state}");
        yield return new WaitUntil(() => timeline.state != PlayState.Playing);
        
        Debug.Log($"Cutscene finished. Timeline state: {timeline.state}");
        
        // Stop music after cutscene
        if (playMusicDuringCutscene)
        {
            Debug.Log($"Stopping cutscene music");
            StopCutsceneMusic();
        }
        
        // Trigger NPC dialogue if configured
        if (triggerNPCDialogueAfterCutscene && npcToTrigger != null)
        {
            Debug.Log($"NPC dialogue configured - starting TriggerNPCDialogue coroutine");
            yield return StartCoroutine(TriggerNPCDialogue());
        }
        else
        {
            Debug.Log($"NPC dialogue NOT triggered. triggerNPCDialogueAfterCutscene: {triggerNPCDialogueAfterCutscene}, npcToTrigger: {npcToTrigger != null}");
        }
        
        // Handle scene transition if enabled
        if (transitionToNewScene && !string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log($"Transitioning to new scene: {targetSceneName}");
            yield return StartCoroutine(TransitionToNewScene());
        }
        else
        {
            // Only re-enable player if we're NOT transitioning to a new scene
            // AND not triggering NPC dialogue (NPC dialogue will handle player control)
            if (!triggerNPCDialogueAfterCutscene)
            {
                Debug.Log($"Enabling player movement (no NPC dialogue or scene transition)");
                EnablePlayerMovement();
            }
            
            // Destroy objects if any
            DestroyObjects();
            
            // Destroy if configured to do so
            if (destroyAfterUse)
            {
                Debug.Log($"Destroying cutscene trigger: {gameObject.name}");
                Destroy(gameObject);
            }
        }
        
        Debug.Log($"=== WAIT FOR CUTSCENE TO END COMPLETE ===");
    }

    private IEnumerator TriggerNPCDialogue()
    {
        Debug.Log($"=== STARTING NPC DIALOGUE TRIGGER ===");
        Debug.Log($"Starting NPC dialogue after {dialogueStartDelay} seconds delay");
        
        // Optional delay before starting dialogue
        if (dialogueStartDelay > 0)
        {
            Debug.Log($"Waiting {dialogueStartDelay} seconds before starting dialogue...");
            yield return new WaitForSeconds(dialogueStartDelay);
        }
        
        // Make sure player is visible if it was hidden during cutscene
        if (hidePlayerDuringCutscene && player != null)
        {
            Debug.Log($"Making player visible (was hidden during cutscene)");
            player.SetActive(true);
        }
        
        // CRITICAL: Re-enable the InteractionDetector so we can receive input for dialogue
        Debug.Log($"=== ATTEMPTING TO RE-ENABLE INTERACTION DETECTOR ===");
        
        // Make sure we have the latest reference to interactionDetector
        if (interactionDetector == null)
        {
            Debug.LogWarning($"InteractionDetector reference is null, trying to find it again...");
            interactionDetector = FindObjectOfType<InteractionDetector>();
        }
        
        if (interactionDetector != null)
        {
            Debug.Log($"Found InteractionDetector: {interactionDetector.gameObject.name}");
            Debug.Log($"InteractionDetector enabled before: {interactionDetector.enabled}");
            Debug.Log($"InteractionDetector gameObject active: {interactionDetector.gameObject.activeInHierarchy}");
            
            interactionDetector.enabled = true;
            
            Debug.Log($"InteractionDetector enabled after: {interactionDetector.enabled}");
            Debug.Log($"=== INTERACTION DETECTOR RE-ENABLED ===");
        }
        else
        {
            Debug.LogError($"InteractionDetector is STILL null - cannot re-enable!");
        }
        
        // Small delay to ensure everything is ready
        yield return new WaitForEndOfFrame();
        
        if (npcToTrigger != null)
        {
            Debug.Log($"NPC found: {npcToTrigger.gameObject.name}");
            Debug.Log($"NPC CanInteract: {npcToTrigger.CanInteract()}");
            
            // Use the force method to bypass normal interaction checks
            Debug.Log($"Calling ForceStartDialogueFromCutscene on NPC...");
            
            // Get the method using reflection
            var forceMethod = npcToTrigger.GetType().GetMethod("ForceStartDialogueFromCutscene");
            if (forceMethod != null)
            {
                forceMethod.Invoke(npcToTrigger, null);
                Debug.Log($"ForceStartDialogueFromCutscene completed");
            }
            else
            {
                Debug.LogError($"ForceStartDialogueFromCutscene method not found on NPC!");
                
                // Fallback: try regular Interact
                if (npcToTrigger.CanInteract())
                {
                    Debug.Log($"Falling back to regular Interact()");
                    npcToTrigger.Interact();
                }
                else
                {
                    Debug.LogError($"Fallback failed - NPC cannot interact!");
                }
            }
        }
        else
        {
            Debug.LogError($"NPC is null - cannot trigger dialogue");
        }
        
        Debug.Log($"=== NPC DIALOGUE TRIGGER COMPLETE ===");
    }

    private IEnumerator TransitionToNewScene()
    {
        Debug.Log($"Transitioning to scene: {targetSceneName} after {sceneTransitionDelay} seconds");
        
        // Optional delay before scene transition
        if (sceneTransitionDelay > 0)
        {
            yield return new WaitForSeconds(sceneTransitionDelay);
        }
        
        // Load the new scene
        SceneManager.LoadScene(targetSceneName);
    }

    // Music Management Methods
    private void StartCutsceneMusic()
    {
        if (cutsceneMusic == null)
        {
            Debug.LogWarning("Cutscene music clip is not assigned!");
            return;
        }

        // Update loop setting in case it was changed in inspector
        if (musicAudioSource != null)
        {
            musicAudioSource.loop = loopMusic;
        }

        // Store current music if we want to resume it later
        if (resumePreviousMusic)
        {
            AudioSource currentMusicSource = FindCurrentMusicSource();
            if (currentMusicSource != null && currentMusicSource.isPlaying)
            {
                previousMusic = currentMusicSource.clip;
            }
        }

        // Stop current music if requested
        if (stopCurrentMusic)
        {
            StopAllCurrentMusic();
        }

        // Play the cutscene music
        if (musicAudioSource != null)
        {
            musicAudioSource.clip = cutsceneMusic;
            StartCoroutine(FadeInMusic(musicAudioSource, musicFadeInTime));
        }
        else
        {
            Debug.LogError("Music AudioSource not found for cutscene music!");
        }
    }

    private void StopCutsceneMusic()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            StartCoroutine(FadeOutMusic(musicAudioSource, musicFadeOutTime, () =>
            {
                // Resume previous music if requested
                if (resumePreviousMusic && previousMusic != null)
                {
                    AudioSource currentMusicSource = FindCurrentMusicSource();
                    if (currentMusicSource != null)
                    {
                        currentMusicSource.clip = previousMusic;
                        currentMusicSource.Play();
                    }
                }
            }));
        }
    }

    private AudioSource FindCurrentMusicSource()
    {
        // You might want to customize this method based on your music system
        // This looks for any AudioSource that's playing music (looping audio)
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in allAudioSources)
        {
            if (source != musicAudioSource && source.isPlaying && source.loop)
            {
                return source;
            }
        }
        return null;
    }

    private void StopAllCurrentMusic()
    {
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in allAudioSources)
        {
            if (source != musicAudioSource && source.isPlaying && source.loop)
            {
                source.Stop();
            }
        }
    }

    private IEnumerator FadeInMusic(AudioSource audioSource, float fadeTime)
    {
        audioSource.volume = 0f;
        audioSource.Play();

        float timer = 0f;
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, 1f, timer / fadeTime);
            yield return null;
        }

        audioSource.volume = 1f;
    }

    private IEnumerator FadeOutMusic(AudioSource audioSource, float fadeTime, System.Action onComplete = null)
    {
        float startVolume = audioSource.volume;

        float timer = 0f;
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeTime);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
        
        onComplete?.Invoke();
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
            Debug.Log("Player movement disabled for cutscene");
        }

        // Find the player animator to potentially freeze animations
        playerAnimator = FindObjectOfType<ScientistAnimator>();
        if (playerAnimator != null && playerAnimator.animator != null)
        {
            // Set to idle animation during cutscene
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

        // Hide player object if configured
        if (hidePlayerDuringCutscene && player != null)
        {
            player.SetActive(false);
            Debug.Log("Player hidden during cutscene");
        }
    }

    private void EnablePlayerMovement()
    {
        // Show player if hidden
        if (hidePlayerDuringCutscene && player != null)
        {
            player.SetActive(true);
            Debug.Log("Player shown after cutscene");
        }

        // Re-enable player movement
        if (playerController != null)
        {
            playerController.moveAction.Enable();
            Debug.Log("Player movement re-enabled after cutscene");
        }

        // Re-enable interaction detector
        if (interactionDetector != null)
        {
            interactionDetector.enabled = true;
        }
    }

    private void DestroyObjects()
    {
        foreach (GameObject obj in objectsToDestroy)
        {
            if (obj != null)
            {
                Destroy(obj);
                Debug.Log($"Destroyed object: {obj.name}");
            }
        }
    }
    
    // Optional: Visual feedback in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = requireInteraction ? Color.yellow : Color.green;
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            if (collider is BoxCollider2D boxCollider)
            {
                Gizmos.DrawWireCube(transform.position + (Vector3)boxCollider.offset, boxCollider.size);
            }
            else if (collider is CircleCollider2D circleCollider)
            {
                Gizmos.DrawWireSphere(transform.position + (Vector3)circleCollider.offset, circleCollider.radius);
            }
        }
    }

    // Debug method to check current state
    void Update()
    {
        // Debug: Show current state in console
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log($"CutsceneTrigger State - CanInteract: {CanInteract()}, HasBeenUsed: {hasBeenUsed}, RequireInteraction: {requireInteraction}");
            if (transitionToNewScene)
            {
                Debug.Log($"Scene Transition: {targetSceneName}, Delay: {sceneTransitionDelay}s");
            }
            if (triggerNPCDialogueAfterCutscene)
            {
                Debug.Log($"NPC Dialogue After: {npcToTrigger?.gameObject.name ?? "None"}, Delay: {dialogueStartDelay}s");
            }
        }
    }
}