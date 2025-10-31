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
            
    }

    // Automatic trigger when entering region (if interaction not required)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            
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
            UpdateInteractionIcon();
        }
    }

    private void UpdateInteractionIcon()
    {
        if (interactionIcon != null)
        {
            bool shouldShow = requireInteraction && CanInteract();
            interactionIcon.SetActive(shouldShow);
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
        TriggerCutscene();
    }

    private void TriggerCutscene()
    {
        if (playOnce && hasBeenUsed)
            return;
        
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
        yield return new WaitUntil(() => timeline.state != PlayState.Playing);
        
        
        // Stop music after cutscene
        if (playMusicDuringCutscene)
        {
            StopCutsceneMusic();
        }
        
        // Trigger NPC dialogue if configured
        if (triggerNPCDialogueAfterCutscene && npcToTrigger != null)
        {
            yield return StartCoroutine(TriggerNPCDialogue());
        }
        
        // Handle scene transition if enabled
        if (transitionToNewScene && !string.IsNullOrEmpty(targetSceneName))
        {
            yield return StartCoroutine(TransitionToNewScene());
        }
        else
        {
            // Only re-enable player if we're NOT transitioning to a new scene
            // AND not triggering NPC dialogue (NPC dialogue will handle player control)
            if (!triggerNPCDialogueAfterCutscene)
            {
                EnablePlayerMovement();
            }
            
            // Destroy objects if any
            DestroyObjects();
            
            // Destroy if configured to do so
            if (destroyAfterUse)
            {
                Destroy(gameObject);
            }
        }
        
    }

    private IEnumerator TriggerNPCDialogue()
    {
        
        // Optional delay before starting dialogue
        if (dialogueStartDelay > 0)
        {
            yield return new WaitForSeconds(dialogueStartDelay);
        }
        
        // Make sure player is visible if it was hidden during cutscene
        if (hidePlayerDuringCutscene && player != null)
        {
            player.SetActive(true);
        }
        
        // Make sure we have the latest reference to interactionDetector
        if (interactionDetector == null)
        {
            interactionDetector = FindObjectOfType<InteractionDetector>();
        }
        
        if (interactionDetector != null)
        {
            interactionDetector.enabled = true;
        }
        
        // Small delay to ensure everything is ready
        yield return new WaitForEndOfFrame();
        
        if (npcToTrigger != null)
        {
            // Get the method using reflection
            var forceMethod = npcToTrigger.GetType().GetMethod("ForceStartDialogueFromCutscene");
            if (forceMethod != null)
            {
                forceMethod.Invoke(npcToTrigger, null);
            }
            else
            {
                
                // Fallback: try regular Interact
                if (npcToTrigger.CanInteract())
                {
                    npcToTrigger.Interact();
                }
            }
        }
    }

    private IEnumerator TransitionToNewScene()
    {
        
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
        }
    }

    private void EnablePlayerMovement()
    {
        // Show player if hidden
        if (hidePlayerDuringCutscene && player != null)
        {
            player.SetActive(true);
        }

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

    private void DestroyObjects()
    {
        foreach (GameObject obj in objectsToDestroy)
        {
            if (obj != null)
            {
                Destroy(obj);
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
}