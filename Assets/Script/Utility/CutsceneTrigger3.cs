using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System.Collections;

public class CutsceneTrigger3 : MonoBehaviour, IInteractable
{
    [Header("Dialogue & Cutscene References")]
    [SerializeField] private NPC npcWithDialogue;
    [SerializeField] private PlayableDirector timelineAfterDialogue;
    
    [Header("Activation Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool requireInteraction = false;
    [SerializeField] private bool playOnce = true;
    
    [Header("Interaction Visuals")]
    [SerializeField] private GameObject interactionIcon;
    
    [Header("Player Control During Cutscene")]
    [SerializeField] private bool hidePlayerDuringCutscene = true;
    
    [Header("Scene Transition (Optional)")]
    [SerializeField] private bool transitionAfterCutscene = false;
    [SerializeField] private string targetSceneName = "";
    [SerializeField] private float sceneTransitionDelay = 0f;
    
    [Header("Skip Settings")]
    [SerializeField] private bool allowSkipCutscene = true;
    [SerializeField] private GameObject skipPrompt;
    [SerializeField] private float skipDelay = 1f;
    
    private bool hasBeenUsed = false;
    private bool isWaitingForDialogue = false;
    private bool isCutsceneActive = false;
    private float skipTimer = 0f;
    private bool canSkip = false;
    private GameObject player;
    private ScientistController playerController;
    private ScientistAnimator playerAnimator;
    private InteractionDetector interactionDetector;
    private Collider2D triggerCollider;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag(playerTag);
        triggerCollider = GetComponent<Collider2D>();
        
        if (interactionIcon != null)
            interactionIcon.SetActive(false);
            
        if (skipPrompt != null)
            skipPrompt.SetActive(false);
    }

    void Update()
    {
        if (isWaitingForDialogue && npcWithDialogue != null)
        {
            if (!npcWithDialogue.IsDialogueActive())
            {
                isWaitingForDialogue = false;
                StartCoroutine(PlayCutsceneAfterDialogue());
            }
        }
        
        if (isCutsceneActive && allowSkipCutscene)
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
                if (!(playOnce && hasBeenUsed))
                {
                    StartDialogueThenCutscene();
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
            bool shouldShow = CanInteract();
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
        StartDialogueThenCutscene();
    }

    private void StartDialogueThenCutscene()
    {
        if (playOnce && hasBeenUsed)
            return;

        if (npcWithDialogue == null)
        {
            Debug.LogWarning("CutsceneTrigger3: NPC with dialogue is not assigned!");
            return;
        }

        if (playOnce)
        {
            hasBeenUsed = true;
        }

        if (interactionIcon != null)
            interactionIcon.SetActive(false);

        if (playOnce && triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        var forceMethod = npcWithDialogue.GetType().GetMethod("ForceStartDialogueFromCutscene");
        if (forceMethod != null)
        {
            forceMethod.Invoke(npcWithDialogue, null);
        }
        else
        {
            if (npcWithDialogue.CanInteract())
            {
                npcWithDialogue.Interact();
            }
        }

        isWaitingForDialogue = true;
    }

    private IEnumerator PlayCutsceneAfterDialogue()
    {
        yield return new WaitForSeconds(0.2f);

        if (timelineAfterDialogue == null)
        {
            Debug.LogWarning("CutsceneTrigger3: Timeline is not assigned!");
            yield break;
        }

        FindAndDisablePlayerMovement();
        
        skipTimer = 0f;
        canSkip = false;
        isCutsceneActive = true;
        
        if (skipPrompt != null)
            skipPrompt.SetActive(false);

        timelineAfterDialogue.Play();

        yield return StartCoroutine(WaitForCutsceneToEnd());
    }

    private IEnumerator WaitForCutsceneToEnd()
    {
        yield return new WaitUntil(() => timelineAfterDialogue.state != PlayState.Playing);

        isCutsceneActive = false;
        
        if (skipPrompt != null)
            skipPrompt.SetActive(false);

        if (transitionAfterCutscene && !string.IsNullOrEmpty(targetSceneName))
        {
            yield return StartCoroutine(TransitionToScene());
        }
        else
        {
            EnablePlayerMovement();
        }
    }

    private IEnumerator TransitionToScene()
    {
        if (sceneTransitionDelay > 0)
        {
            yield return new WaitForSeconds(sceneTransitionDelay);
        }

        SceneManager.LoadScene(targetSceneName);
    }

    private bool CheckSkipInput()
    {
        #if ENABLE_INPUT_SYSTEM
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        
        if (keyboard != null)
        {
            bool spacePressed = keyboard.spaceKey.wasPressedThisFrame;
            
            if (spacePressed)
            {
                bool isAltPressed = keyboard.leftAltKey.isPressed || keyboard.rightAltKey.isPressed;
                bool isCtrlPressed = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
                
                if (isAltPressed || isCtrlPressed)
                {
                    return false;
                }
                
                return true;
            }
        }
        
        return false;
        #else
        return Input.GetKeyDown(KeyCode.Space);
        #endif
    }

    private void SkipCutscene()
    {
        if (!isCutsceneActive || !canSkip) return;

        if (timelineAfterDialogue != null && timelineAfterDialogue.state == PlayState.Playing)
        {
            timelineAfterDialogue.Stop();
        }

        if (skipPrompt != null)
            skipPrompt.SetActive(false);

        isCutsceneActive = false;

        if (transitionAfterCutscene && !string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            EnablePlayerMovement();
        }
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = requireInteraction ? Color.cyan : Color.blue;
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
