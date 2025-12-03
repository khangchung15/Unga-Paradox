using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System.Collections;

public class BossDeathCutscene : MonoBehaviour
{
    [Header("Boss Reference")]
    [SerializeField] private BossHealthLatest bossHealth;
    
    [Header("Cutscene References")]
    [SerializeField] private PlayableDirector timeline;
    
    [Header("Objects to Hide During Cutscene")]
    [Tooltip("GameObjects that will be hidden when cutscene starts")]
    [SerializeField] private GameObject[] objectsToHide;
    
    [Header("Objects to Disable During Cutscene")]
    [Tooltip("GameObjects that will be disabled when cutscene starts")]
    [SerializeField] private GameObject[] objectsToDisable;
    
    [Header("Player Control")]
    [SerializeField] private bool hidePlayerDuringCutscene = true;
    [SerializeField] private string playerTag = "Player";
    
    [Header("Scene Transition")]
    [SerializeField] private string targetSceneName = "";
    [SerializeField] private float sceneTransitionDelay = 0f;
    
    private GameObject player;
    private ScientistController playerController;
    private ScientistAnimator playerAnimator;
    private InteractionDetector interactionDetector;
    private bool cutsceneTriggered = false;
    
    void Start()
    {
        if (bossHealth == null)
        {
            Debug.LogError("BossDeathCutscene: BossHealthLatest reference is required.");
            return;
        }
        
        if (bossHealth.onDeath != null)
        {
            bossHealth.onDeath.AddListener(OnBossDeath);
        }
        
        player = GameObject.FindGameObjectWithTag(playerTag);
        
        if (timeline != null)
        {
            timeline.stopped += OnTimelineStopped;
        }
    }
    
    private void OnBossDeath()
    {
        if (cutsceneTriggered)
            return;
            
        cutsceneTriggered = true;
        Debug.Log("BossDeathCutscene: Boss died, starting cutscene coroutine");
        StartCoroutine(PlayDeathCutscene());
    }
    
    private IEnumerator PlayDeathCutscene()
    {
        Debug.Log("BossDeathCutscene: Waiting 0.5 seconds before starting cutscene");
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log("BossDeathCutscene: Disabling player movement and hiding objects");
        DisablePlayerMovement();
        HideAndDisableObjects();
        
        if (timeline != null)
        {
            Debug.Log("BossDeathCutscene: Starting timeline playback");
            timeline.Play();
            Debug.Log($"BossDeathCutscene: Timeline duration: {timeline.duration} seconds");
        }
        else
        {
            Debug.LogError("BossDeathCutscene: Timeline is null!");
            LoadTargetScene();
        }
    }
    
    private void OnTimelineStopped(PlayableDirector director)
    {
        if (director == timeline)
        {
            Debug.Log("BossDeathCutscene: Timeline stopped event received");
            StartCoroutine(TransitionAfterTimeline());
        }
    }
    
    private IEnumerator TransitionAfterTimeline()
    {
        Debug.Log("BossDeathCutscene: Timeline finished playing");
        
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log($"BossDeathCutscene: Preparing to load scene '{targetSceneName}'");
            
            if (sceneTransitionDelay > 0)
            {
                Debug.Log($"BossDeathCutscene: Waiting {sceneTransitionDelay} seconds before transition");
                yield return new WaitForSeconds(sceneTransitionDelay);
            }
            
            Debug.Log($"BossDeathCutscene: Loading scene '{targetSceneName}'");
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogWarning("BossDeathCutscene: No target scene name set! Enabling player movement instead.");
            EnablePlayerMovement();
        }
    }
    
    private void LoadTargetScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log($"BossDeathCutscene: Loading scene '{targetSceneName}' (fallback)");
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            EnablePlayerMovement();
        }
    }
    
    private void HideAndDisableObjects()
    {
        if (objectsToHide != null)
        {
            foreach (GameObject obj in objectsToHide)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
        
        if (objectsToDisable != null)
        {
            foreach (GameObject obj in objectsToDisable)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }
    
    private void DisablePlayerMovement()
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
    
    void OnDestroy()
    {
        if (bossHealth != null && bossHealth.onDeath != null)
        {
            bossHealth.onDeath.RemoveListener(OnBossDeath);
        }
        
        if (timeline != null)
        {
            timeline.stopped -= OnTimelineStopped;
        }
    }
}
