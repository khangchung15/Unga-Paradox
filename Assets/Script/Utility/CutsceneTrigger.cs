using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using System.Collections.Generic;
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
    
    [Header("Gravity Control")]
    [Tooltip("Rigidbody2D objects that will have gravity disabled during cutscenes")]
    [SerializeField] private Rigidbody2D[] rigidbodiesWithGravity;
    [SerializeField] private bool disableGravityDuringCutscenes = false;
    [SerializeField] private bool enableGravityAfterSecondCutscene = true;
    
    [Header("Collision Control")]
    [Tooltip("Colliders that will be disabled at start and enabled after second cutscene")]
    [SerializeField] private Collider2D[] collidersToDisable;
    [SerializeField] private bool disableCollisionDuringCutscenes = false;
    
    [Header("Opposite Collision Control")]
    [Tooltip("Colliders that will be enabled at start and disabled after second cutscene")]
    [SerializeField] private Collider2D[] collidersToEnableAtStart;
    [SerializeField] private bool enableCollisionAtStart = false;
    
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
    
    [Header("Second Cutscene After Dialogue")]
    [SerializeField] private bool triggerSecondCutscene = false;
    [SerializeField] private PlayableDirector secondTimeline;
    [SerializeField] private float secondCutsceneDelay = 0.5f;
    [SerializeField] private bool hidePlayerDuringSecondCutscene = true;
    [SerializeField] private bool transitionAfterSecondCutscene = false;
    [SerializeField] private string secondCutsceneTargetScene = "";
    [SerializeField] private float secondCutsceneTransitionDelay = 0f;
    
    [Header("Skip Settings")]
    [SerializeField] private bool allowSkip = true;
    [SerializeField] private GameObject skipPrompt;
    [SerializeField] private float skipDelay = 1f;
    [SerializeField] private bool useNewInputSystem = true;
    [SerializeField] private string skipTargetScene = "";
    
    private bool hasBeenTriggered = false;
    private bool hasBeenUsed = false;
    private GameObject player;
    private InteractionDetector interactionDetector;
    private ScientistController playerController;
    private ScientistAnimator playerAnimator;
    private Collider2D triggerCollider;
    private AudioSource musicAudioSource;
    private AudioClip previousMusic;
    private bool isWaitingForDialogue = false;
    private bool isCutsceneActive = false;
    private float skipTimer = 0f;
    private bool canSkip = false;
    private bool isFirstCutscene = true;
    
    // Store original gravity and collider states
    private Dictionary<Rigidbody2D, float> originalGravityScales = new Dictionary<Rigidbody2D, float>();
    private Dictionary<Collider2D, bool> originalColliderStates = new Dictionary<Collider2D, bool>();
    private Dictionary<Collider2D, bool> originalOppositeColliderStates = new Dictionary<Collider2D, bool>();

    void Start()
    {
        player = GameObject.FindGameObjectWithTag(playerTag);
        triggerCollider = GetComponent<Collider2D>();
        
        // Store original gravity scales
        if (disableGravityDuringCutscenes && rigidbodiesWithGravity != null)
        {
            foreach (Rigidbody2D rb in rigidbodiesWithGravity)
            {
                if (rb != null)
                {
                    originalGravityScales[rb] = rb.gravityScale;
                }
            }
        }
        
        // Store original collider states and disable them at start
        if (disableCollisionDuringCutscenes && collidersToDisable != null)
        {
            foreach (Collider2D col in collidersToDisable)
            {
                if (col != null)
                {
                    originalColliderStates[col] = col.enabled;
                    col.enabled = false; // Disable at start so player can walk through
                }
            }
        }
        
        // Store original opposite collider states and enable them at start
        if (enableCollisionAtStart && collidersToEnableAtStart != null)
        {
            foreach (Collider2D col in collidersToEnableAtStart)
            {
                if (col != null)
                {
                    originalOppositeColliderStates[col] = col.enabled;
                    col.enabled = true; // Enable at start (solid)
                }
            }
        }
        
        // Get or create AudioSource for music
        if (playMusicDuringCutscene)
        {
            musicAudioSource = GetComponent<AudioSource>();
            if (musicAudioSource == null)
            {
                musicAudioSource = gameObject.AddComponent<AudioSource>();
                musicAudioSource.playOnAwake = false;
                musicAudioSource.loop = loopMusic;
            }
        }
        
        if (interactionIcon != null)
            interactionIcon.SetActive(false);
            
        if (skipPrompt != null)
            skipPrompt.SetActive(false);
    }

    void Update()
    {
        if (isWaitingForDialogue && npcToTrigger != null)
        {
            if (!npcToTrigger.IsDialogueActive())
            {
                isWaitingForDialogue = false;
                StartCoroutine(TriggerSecondCutsceneAfterDelay());
            }
        }
        
        if (isCutsceneActive && allowSkip)
        {
            if (!canSkip)
            {
                skipTimer += Time.deltaTime;
                if (skipTimer >= skipDelay)
                {
                    canSkip = true;
                    if (skipPrompt != null)
                        skipPrompt.SetActive(true);
                }
            }
            else
            {
                if (CheckSkipInput())
                {
                    SkipCutscene();
                }
            }
        }
    }

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

    public bool CanInteract()
    {
        if (playOnce && hasBeenUsed)
        {
            return false;
        }
        
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
        
        if (playOnce)
        {
            hasBeenUsed = true;
            hasBeenTriggered = true;
        }
        
        // Disable gravity if configured
        if (disableGravityDuringCutscenes)
        {
            DisableGravity();
        }
        
        if (playMusicDuringCutscene)
        {
            StartCutsceneMusic();
        }
        
        FindAndDisablePlayerMovement();
        SetupSkipSystem();
        timeline.Play();
        
        if (interactionIcon != null)
            interactionIcon.SetActive(false);
        
        if (playOnce && triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }
        
        StartCoroutine(WaitForCutsceneToEnd());
    }

    private void DisableGravity()
    {
        if (rigidbodiesWithGravity != null)
        {
            foreach (Rigidbody2D rb in rigidbodiesWithGravity)
            {
                if (rb != null)
                {
                    rb.gravityScale = 0f;
                    rb.linearVelocity = Vector2.zero; // Stop any current movement
                }
            }
        }
    }

    private void EnableGravity()
    {
        // Re-enable gravity
        if (rigidbodiesWithGravity != null)
        {
            foreach (Rigidbody2D rb in rigidbodiesWithGravity)
            {
                if (rb != null && originalGravityScales.ContainsKey(rb))
                {
                    rb.gravityScale = 1;
                }
            }
        }
        
        // Re-enable colliders that were disabled at start
        if (disableCollisionDuringCutscenes && collidersToDisable != null)
        {
            foreach (Collider2D col in collidersToDisable)
            {
                if (col != null && originalColliderStates.ContainsKey(col))
                {
                    col.enabled = originalColliderStates[col];
                }
            }
        }
        
        // Disable colliders that were enabled at start (opposite behavior)
        if (enableCollisionAtStart && collidersToEnableAtStart != null)
        {
            foreach (Collider2D col in collidersToEnableAtStart)
            {
                if (col != null)
                {
                    col.enabled = false; // Disable after second cutscene
                }
            }
        }
    }

    private void SetupSkipSystem()
    {
        isCutsceneActive = true;
        isFirstCutscene = true;
        skipTimer = 0f;
        canSkip = false;
        
        if (skipPrompt != null)
            skipPrompt.SetActive(false);
    }

    private void SetupSecondCutsceneSkip()
    {
        isCutsceneActive = true;
        isFirstCutscene = false;
        skipTimer = 0f;
        canSkip = false;
        
        if (skipPrompt != null)
            skipPrompt.SetActive(false);
    }

    private bool CheckSkipInput()
    {
        if (useNewInputSystem)
        {
            #if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            var gamepad = UnityEngine.InputSystem.Gamepad.current;
            
            return (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || 
                                        keyboard.escapeKey.wasPressedThisFrame)) ||
                   (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame);
            #else
            return UnityEngine.Input.GetKeyDown(KeyCode.Space) || 
                   UnityEngine.Input.GetKeyDown(KeyCode.Escape);
            #endif
        }
        else
        {
            return UnityEngine.Input.GetKeyDown(KeyCode.Space) || 
                   UnityEngine.Input.GetKeyDown(KeyCode.Escape);
        }
    }

    private void SkipCutscene()
    {
        if (!isCutsceneActive || !canSkip) return;
        
        if (isFirstCutscene && timeline != null && timeline.state == PlayState.Playing)
        {
            timeline.Stop();
        }
        else if (!isFirstCutscene && secondTimeline != null && secondTimeline.state == PlayState.Playing)
        {
            secondTimeline.Stop();
        }
        
        if (playMusicDuringCutscene && musicAudioSource != null && musicAudioSource.isPlaying)
        {
            StopCutsceneMusic();
        }
        
        if (skipPrompt != null)
            skipPrompt.SetActive(false);
        
        // Enable gravity if skipping after second cutscene
        if (!isFirstCutscene && enableGravityAfterSecondCutscene)
        {
            EnableGravity();
        }
        
        string sceneToLoad = skipTargetScene;
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            if (isFirstCutscene && transitionToNewScene && !string.IsNullOrEmpty(targetSceneName))
            {
                sceneToLoad = targetSceneName;
            }
            else if (!isFirstCutscene && transitionAfterSecondCutscene && !string.IsNullOrEmpty(secondCutsceneTargetScene))
            {
                sceneToLoad = secondCutsceneTargetScene;
            }
        }
        
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            EnablePlayerMovement();
            DestroyObjects();
            
            if (destroyAfterUse)
            {
                Destroy(gameObject);
            }
        }
        
        isCutsceneActive = false;
    }

    public void SkipButton()
    {
        SkipCutscene();
    }

    private IEnumerator WaitForCutsceneToEnd()
    {
        yield return new WaitUntil(() => timeline.state != PlayState.Playing);
        
        isCutsceneActive = false;
        if (skipPrompt != null)
            skipPrompt.SetActive(false);
        
        if (playMusicDuringCutscene)
        {
            StopCutsceneMusic();
        }
        
        if (triggerNPCDialogueAfterCutscene && npcToTrigger != null)
        {
            yield return StartCoroutine(TriggerNPCDialogue());
            
            if (triggerSecondCutscene && secondTimeline != null)
            {
                isWaitingForDialogue = true;
            }
            else
            {
                // No second cutscene, enable gravity now if needed
                if (disableGravityDuringCutscenes && !triggerSecondCutscene)
                {
                    EnableGravity();
                }
            }
        }
        else if (transitionToNewScene && !string.IsNullOrEmpty(targetSceneName))
        {
            yield return StartCoroutine(TransitionToNewScene());
        }
        else
        {
            EnablePlayerMovement();
            
            // Enable gravity if no second cutscene
            if (disableGravityDuringCutscenes && !triggerSecondCutscene)
            {
                EnableGravity();
            }
            
            DestroyObjects();
            
            if (destroyAfterUse)
            {
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator TriggerNPCDialogue()
    {
        if (dialogueStartDelay > 0)
        {
            yield return new WaitForSeconds(dialogueStartDelay);
        }
        
        if (hidePlayerDuringCutscene && player != null)
        {
            player.SetActive(true);
        }
        
        if (interactionDetector == null)
        {
            interactionDetector = FindObjectOfType<InteractionDetector>();
        }
        
        if (interactionDetector != null)
        {
            interactionDetector.enabled = true;
        }
        
        yield return new WaitForEndOfFrame();
        
        if (npcToTrigger != null)
        {
            var forceMethod = npcToTrigger.GetType().GetMethod("ForceStartDialogueFromCutscene");
            if (forceMethod != null)
            {
                forceMethod.Invoke(npcToTrigger, null);
            }
            else
            {
                if (npcToTrigger.CanInteract())
                {
                    npcToTrigger.Interact();
                }
            }
        }
    }

    private IEnumerator TriggerSecondCutsceneAfterDelay()
    {
        if (secondCutsceneDelay > 0)
        {
            yield return new WaitForSeconds(secondCutsceneDelay);
        }
        
        FindAndDisablePlayerMovement();
        
        if (hidePlayerDuringSecondCutscene && player != null)
        {
            player.SetActive(false);
        }
        
        SetupSecondCutsceneSkip();
        secondTimeline.Play();
        
        yield return StartCoroutine(WaitForSecondCutsceneToEnd());
    }

    private IEnumerator WaitForSecondCutsceneToEnd()
    {
        yield return new WaitUntil(() => secondTimeline.state != PlayState.Playing);
        
        isCutsceneActive = false;
        if (skipPrompt != null)
            skipPrompt.SetActive(false);
        
        // Enable gravity after second cutscene ends
        if (enableGravityAfterSecondCutscene)
        {
            EnableGravity();
        }
        
        if (transitionAfterSecondCutscene && !string.IsNullOrEmpty(secondCutsceneTargetScene))
        {
            if (secondCutsceneTransitionDelay > 0)
            {
                yield return new WaitForSeconds(secondCutsceneTransitionDelay);
            }
            
            SceneManager.LoadScene(secondCutsceneTargetScene);
        }
        else
        {
            EnablePlayerMovement();
            DestroyObjects();
            
            if (destroyAfterUse)
            {
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator TransitionToNewScene()
    {
        if (sceneTransitionDelay > 0)
        {
            yield return new WaitForSeconds(sceneTransitionDelay);
        }
        
        SceneManager.LoadScene(targetSceneName);
    }

    private void StartCutsceneMusic()
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.loop = loopMusic;
        }

        if (resumePreviousMusic)
        {
            AudioSource currentMusicSource = FindCurrentMusicSource();
            if (currentMusicSource != null && currentMusicSource.isPlaying)
            {
                previousMusic = currentMusicSource.clip;
            }
        }

        if (stopCurrentMusic)
        {
            StopAllCurrentMusic();
        }

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

    private void FindAndDisablePlayerMovement()
    {
        playerController = FindObjectOfType<ScientistController>();
        if (playerController != null)
        {
            playerController.moveAction.Disable();
        }

        playerAnimator = FindObjectOfType<ScientistAnimator>();
        if (playerAnimator != null && playerAnimator.animator != null)
        {
            playerAnimator.animator.SetBool("isIdle", true);
            playerAnimator.animator.SetBool("isRunning", false);
        }

        interactionDetector = FindObjectOfType<InteractionDetector>();
        if (interactionDetector != null)
        {
            interactionDetector.enabled = false;
            if (interactionDetector.interactionIcon != null)
                interactionDetector.interactionIcon.SetActive(false);
        }

        if (hidePlayerDuringCutscene && player != null)
        {
            player.SetActive(false);
        }
    }

    private void EnablePlayerMovement()
    {
        if (hidePlayerDuringCutscene && player != null)
        {
            player.SetActive(true);
        }

        if (playerController != null)
        {
            playerController.moveAction.Enable();
        }

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