using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System.Collections;

public class CutsceneTrigger2 : MonoBehaviour, IInteractable
{
    [Header("Cutscene References")]
    [SerializeField] private PlayableDirector timeline;
    [SerializeField] private string playerTag = "Player";
    
    [Header("Interaction Visuals")]
    [SerializeField] private GameObject interactionIcon;
    
    [Header("Credits Timing")]
    [SerializeField] private float delayAfterCredits = 1f;
    
    [Header("Audio Fade Settings")]
    [SerializeField] private float audioFadeOutDuration = 2f;
    [Tooltip("If true, fades out all AudioSources in the scene. If false, only fades looping music.")]
    [SerializeField] private bool fadeAllAudioSources = true;
    
    [Header("Scene Transition")]
    [SerializeField] private string targetSceneName = "";
    [SerializeField] private float sceneTransitionDelay = 0f;
    
    private bool hasBeenUsed = false;
    private GameObject player;
    private ScientistController playerController;
    private ScientistAnimator playerAnimator;
    private InteractionDetector interactionDetector;
    private Rigidbody2D playerRigidbody;
    private Vector3 lockedPosition;
    private CreditsScroller creditsScroller;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag(playerTag);
        
        if (interactionIcon != null)
            interactionIcon.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            UpdateInteractionIcon();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            UpdateInteractionIcon();
        }
    }

    private void UpdateInteractionIcon()
    {
        if (interactionIcon != null)
        {
            bool shouldShow = CanInteract();
            interactionIcon.SetActive(shouldShow);
        }
    }

    public bool CanInteract()
    {
        return !hasBeenUsed;
    }

    public void Interact()
    {
        if (hasBeenUsed)
            return;

        hasBeenUsed = true;
        TriggerCutscene();
    }

    private void TriggerCutscene()
    {
        FindAndLockPlayer();
        
        if (interactionIcon != null)
            interactionIcon.SetActive(false);
        
        timeline.Play();
        
        creditsScroller = FindObjectOfType<CreditsScroller>();
        if (creditsScroller != null)
        {
            creditsScroller.StartCredits();
        }
        
        StartCoroutine(WaitForCreditsToFinish());
    }

    private void FindAndLockPlayer()
    {
        playerController = FindObjectOfType<ScientistController>();
        if (playerController != null)
        {
            playerController.moveAction.Disable();
            
            playerRigidbody = playerController.GetComponent<Rigidbody2D>();
            if (playerRigidbody != null)
            {
                lockedPosition = playerRigidbody.position;
                playerRigidbody.linearVelocity = Vector2.zero;
                playerRigidbody.bodyType = RigidbodyType2D.Static;
            }
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
    }

    private IEnumerator WaitForCreditsToFinish()
    {
        if (creditsScroller == null)
        {
            yield return StartCoroutine(FadeOutAllAudio());
            yield return StartCoroutine(TransitionToNewScene());
            yield break;
        }
        
        yield return new WaitUntil(() => creditsScroller.IsFinished());
        
        if (delayAfterCredits > 0)
        {
            yield return new WaitForSeconds(delayAfterCredits);
        }
        
        yield return StartCoroutine(FadeOutAllAudio());
        
        yield return StartCoroutine(TransitionToNewScene());
    }

    private IEnumerator FadeOutAllAudio()
    {
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        
        if (allAudioSources.Length == 0)
        {
            yield break;
        }

        System.Collections.Generic.Dictionary<AudioSource, float> originalVolumes = new System.Collections.Generic.Dictionary<AudioSource, float>();
        
        foreach (AudioSource audioSource in allAudioSources)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                if (fadeAllAudioSources || audioSource.loop)
                {
                    originalVolumes[audioSource] = audioSource.volume;
                }
            }
        }
        
        if (originalVolumes.Count == 0)
        {
            yield break;
        }
        
        float elapsed = 0f;
        
        while (elapsed < audioFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / audioFadeOutDuration;
            
            foreach (var kvp in originalVolumes)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.volume = Mathf.Lerp(kvp.Value, 0f, t);
                }
            }
            
            yield return null;
        }
        
        foreach (var kvp in originalVolumes)
        {
            if (kvp.Key != null)
            {
                kvp.Key.volume = 0f;
                kvp.Key.Stop();
            }
        }
    }

    private IEnumerator TransitionToNewScene()
    {
        if (sceneTransitionDelay > 0)
        {
            yield return new WaitForSeconds(sceneTransitionDelay);
        }
        
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
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
