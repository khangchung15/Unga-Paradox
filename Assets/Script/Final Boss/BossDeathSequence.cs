using UnityEngine;
using UnityEngine.Playables;
using System.Collections;

public class BossDeathSequence : MonoBehaviour
{
    [Header("Cutscene References")]
    [SerializeField] private PlayableDirector firstCutscene;
    [SerializeField] private PlayableDirector finalCutscene;
    
    [Header("Boss References")]
    [SerializeField] private GameObject boss;

    [Header("Player Settings")]
    [SerializeField] private bool hidePlayerDuringFirstCutscene = false;
    [SerializeField] private bool hidePlayerDuringFinalCutscene = false;

    [Header("Scene Transition")]
    [SerializeField] private bool transitionAfterFinalCutscene = false;
    [SerializeField] private string targetSceneName = "";
    [SerializeField] private float transitionDelay = 0f;

    private GameObject player;
    private ScientistController playerController;
    private InteractionDetector interactionDetector;
    private NPC bossNPC;
    private bool isWaitingForDialogue = false;
    private bool dialogueHasStarted = false;
    private bool sequenceStarted = false;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        
        if (boss == null)
        {
            boss = GameObject.FindGameObjectWithTag("Boss");
        }
        
        if (bossNPC == null && boss != null)
        {
            bossNPC = boss.GetComponent<NPC>();
        }
    }

    private void Update()
    {
        if (isWaitingForDialogue && bossNPC != null)
        {
            bool dialogueActive = bossNPC.IsDialogueActive();
            
            if (dialogueActive && !dialogueHasStarted)
            {
                dialogueHasStarted = true;
            }
            
            if (dialogueHasStarted && !dialogueActive)
            {
                isWaitingForDialogue = false;
                dialogueHasStarted = false;
                StartCoroutine(PlayFinalCutscene());
            }
        }
    }

    public void StartDeathSequence()
    {
        if (sequenceStarted) return;
        sequenceStarted = true;
        
        StartCoroutine(DeathSequenceCoroutine());
    }

    private IEnumerator DeathSequenceCoroutine()
    {
        if (firstCutscene != null)
        {
            DisablePlayerMovement();
            
            if (hidePlayerDuringFirstCutscene && player != null)
            {
                player.SetActive(false);
            }
            
            firstCutscene.Play();
            
            yield return new WaitUntil(() => firstCutscene.state != PlayState.Playing);
            
            
            if (hidePlayerDuringFirstCutscene && player != null)
            {
                player.SetActive(true);
            }
            
            EnablePlayerMovement();
        }
        
        if (boss != null)
        {
            NPC npcOnBoss = boss.GetComponent<NPC>();
            if (npcOnBoss != null)
            {
                npcOnBoss.enabled = true;
            }
        }
        
        isWaitingForDialogue = true;
        dialogueHasStarted = false;
    }

    private IEnumerator PlayFinalCutscene()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (finalCutscene != null)
        {
            DisablePlayerMovement();
            
            if (hidePlayerDuringFinalCutscene && player != null)
            {
                player.SetActive(false);
            }
            
            finalCutscene.Play();
            
            yield return new WaitUntil(() => finalCutscene.state != PlayState.Playing);
            
            if (hidePlayerDuringFinalCutscene && player != null)
            {
                player.SetActive(true);
            }
        }
        
        if (transitionAfterFinalCutscene && !string.IsNullOrEmpty(targetSceneName))
        {
            if (transitionDelay > 0)
            {
                yield return new WaitForSeconds(transitionDelay);
            }
            
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            EnablePlayerMovement();
        }
    }

    private void DisablePlayerMovement()
    {
        if (playerController == null)
        {
            playerController = FindObjectOfType<ScientistController>();
        }
        
        if (playerController != null)
        {
            playerController.moveAction.Disable();
        }
        
        if (interactionDetector == null)
        {
            interactionDetector = FindObjectOfType<InteractionDetector>();
        }
        
        if (interactionDetector != null)
        {
            interactionDetector.enabled = false;
            if (interactionDetector.interactionIcon != null)
            {
                interactionDetector.interactionIcon.SetActive(false);
            }
        }
    }

    private void EnablePlayerMovement()
    {
        if (playerController != null)
        {
            playerController.moveAction.Enable();
        }
        
        if (interactionDetector != null)
        {
            interactionDetector.enabled = true;
        }
    }
}