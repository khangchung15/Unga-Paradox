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
        StartCoroutine(WaitForCutsceneToEnd());
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

    private IEnumerator WaitForCutsceneToEnd()
    {
        yield return new WaitUntil(() => timeline.state != PlayState.Playing);
        
        yield return StartCoroutine(TransitionToNewScene());
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
