using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;

    [Header("Objects to Destroy After Dialogue")]
    [Tooltip("GameObjects that will be destroyed when dialogue ends")]
    public GameObject[] objectsToDestroy;

    [Header("Dialogue Settings")]
    [Tooltip("If true, this dialogue can only be triggered once")]
    public bool oneTimeDialogue = false;
    [Tooltip("If true, the NPC will be destroyed after one-time dialogue")]
    public bool destroyNPCAfterOneTime = false;

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;
    private bool hasBeenInteracted = false;
    private ScientistController playerController;
    private InteractionDetector interactionDetector;
    private ScientistAnimator playerAnimator;
    
    // For self-handling input during cutscene-triggered dialogue
    private InputAction dialogueAdvanceAction;
    private bool isHandlingOwnInput = false;

    void Awake()
    {
        // Create an input action for dialogue advancement (separate from InteractionDetector)
        dialogueAdvanceAction = new InputAction(binding: "<Keyboard>/e");
        dialogueAdvanceAction.AddBinding("<Mouse>/leftButton");
    }

    void OnEnable()
    {
        if (dialogueAdvanceAction != null)
            dialogueAdvanceAction.Enable();
    }

    void OnDisable()
    {
        if (dialogueAdvanceAction != null)
            dialogueAdvanceAction.Disable();
    }

    void Update()
    {
        // Handle input ourselves when we're managing our own input (cutscene-triggered dialogue)
        if (isHandlingOwnInput && isDialogueActive)
        {
            if (dialogueAdvanceAction.WasPressedThisFrame())
            {
                Debug.Log("NPC handling own input - advancing dialogue");
                NextLine();
            }
        }
    }

    public bool CanInteract()
    {
        // If it's one-time dialogue and already been interacted with, can't interact again
        // BUT only if dialogue is NOT currently active
        if (oneTimeDialogue && hasBeenInteracted && !isDialogueActive)
        {
            Debug.Log($"CanInteract: false (one-time dialogue already used and not active)");
            return false;
        }
        
        // Return true when not handling own input (normal interaction mode)
        // Return false when handling own input (cutscene mode - InteractionDetector should ignore us)
        bool canInteract = !isHandlingOwnInput;
        Debug.Log($"CanInteract: {canInteract} (isDialogueActive: {isDialogueActive}, isHandlingOwnInput: {isHandlingOwnInput})");
        return canInteract;
    }

    public void Interact()
    {
        Debug.Log($"=== NPC.Interact() CALLED ===");
        Debug.Log($"NPC: {gameObject.name}");
        Debug.Log($"isDialogueActive: {isDialogueActive}");
        Debug.Log($"PauseController.IsGamePaused: {PauseController.IsGamePaused}");
        Debug.Log($"dialogueData is null: {dialogueData == null}");
        
        // If no dialogue data or the game is paused, then no dialogue would be active
        if (dialogueData == null || (PauseController.IsGamePaused && !isDialogueActive))
        {
            Debug.LogWarning($"Cannot start dialogue - conditions not met");
            return;
        }

        // Don't block Interact() during active dialogue - this allows fast-forwarding
        if (isDialogueActive)
        {
            Debug.Log($"Fast-forwarding dialogue line");
            NextLine();
        }
        else
        {
            // Only check one-time dialogue when starting new dialogue
            if (oneTimeDialogue && hasBeenInteracted)
            {
                Debug.LogWarning($"Cannot start dialogue - already interacted with one-time NPC");
                return;
            }
            Debug.Log($"Starting new dialogue - calling StartDialogue()");
            StartDialogue(false); // Normal mode - not handling own input
        }
        
        Debug.Log($"=== NPC.Interact() COMPLETE ===");
    }

    void StartDialogue(bool handleOwnInput)
    {
        Debug.Log($"=== NPC.StartDialogue(handleOwnInput: {handleOwnInput}) ===");
        isDialogueActive = true;
        isHandlingOwnInput = handleOwnInput;
        dialogueIndex = 0;

        // Mark as interacted if it's one-time dialogue
        if (oneTimeDialogue)
        {
            hasBeenInteracted = true;
            Debug.Log($"Marked as interacted (one-time dialogue)");
        }

        // Find and disable player movement
        Debug.Log($"Calling FindAndDisablePlayerMovement()");
        FindAndDisablePlayerMovement();

        dialoguePanel.SetActive(true);
        Debug.Log($"Dialogue panel activated: {dialoguePanel.activeInHierarchy}");
        
        PauseController.SetPause(true);
        Debug.Log($"Game paused: {PauseController.IsGamePaused}");

        Debug.Log($"Starting TypeLine coroutine");
        StartCoroutine(TypeLine());
        Debug.Log($"=== NPC.StartDialogue() COMPLETE ===");
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            DisplayCurrentLine();
            isTyping = false;
        }
        else if (++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            // If another line, type next line
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    void DisplayCurrentLine()
    {
        NPCDialogue.DialogueLine currentLine = dialogueData.dialogueLines[dialogueIndex];
        nameText.SetText(currentLine.speakerName);
        portraitImage.sprite = currentLine.speakerPortrait;
        dialogueText.SetText(currentLine.text);
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");

        NPCDialogue.DialogueLine currentLine = dialogueData.dialogueLines[dialogueIndex];
        
        // Update name and portrait for current speaker
        nameText.SetText(currentLine.speakerName);
        portraitImage.sprite = currentLine.speakerPortrait;

        // Use line-specific values or fallback to defaults
        AudioClip voiceClip = currentLine.voiceSound != null ? currentLine.voiceSound : dialogueData.defaultVoiceSound;
        float voicePitch = currentLine.voicePitch;
        float typingSpeed = currentLine.typingSpeed > 0 ? currentLine.typingSpeed : dialogueData.defaultTypingSpeed;

        foreach (char letter in currentLine.text)
        {
            dialogueText.text += letter;
            SoundEffectManager.PlayVoice(voiceClip, voicePitch);
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        if (dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    public void EndDialogue()
    {
        Debug.Log($"=== NPC.EndDialogue() ===");
        StopAllCoroutines();
        isDialogueActive = false;
        isHandlingOwnInput = false; // Reset input handling mode
        dialogueText.SetText("");
        dialoguePanel.SetActive(false);
        PauseController.SetPause(false);
        
        EnablePlayerMovement();
        ActivateEnemies();
        DestroyObjects();

        // Destroy NPC if it's one-time dialogue and the option is enabled
        if (oneTimeDialogue && destroyNPCAfterOneTime)
        {
            Debug.Log($"Destroying NPC after one-time dialogue: {gameObject.name}");
            Destroy(gameObject);
        }
    }

    private void FindAndDisablePlayerMovement()
    {
        Debug.Log($"=== NPC.FindAndDisablePlayerMovement() ===");
        
        // Find the player controller component
        playerController = FindObjectOfType<ScientistController>();
        if (playerController != null)
        {
            Debug.Log($"Found PlayerController, moveAction enabled: {playerController.moveAction.enabled}");
            // Disable the moveAction to prevent movement input
            playerController.moveAction.Disable();
            Debug.Log($"Player movement disabled for dialogue. moveAction enabled: {playerController.moveAction.enabled}");
        }
        else
        {
            Debug.LogError($"PlayerController not found!");
        }

        // Find the player animator to potentially freeze animations
        playerAnimator = FindObjectOfType<ScientistAnimator>();
        if (playerAnimator != null && playerAnimator.animator != null)
        {
            Debug.Log($"Found PlayerAnimator, setting to idle");
            // Set to idle animation during dialogue
            playerAnimator.animator.SetBool("isIdle", true);
            playerAnimator.animator.SetBool("isRunning", false);
        }
        else
        {
            Debug.LogWarning($"PlayerAnimator not found or animator is null");
        }

        // Find the interaction detector for reference
        interactionDetector = FindObjectOfType<InteractionDetector>();
        if (interactionDetector != null)
        {
            Debug.Log($"Found InteractionDetector, enabled: {interactionDetector.enabled}");
            
            // Only disable if NOT handling own input (normal mode)
            // If handling own input (cutscene mode), leave it enabled but we return false from CanInteract
            if (!isHandlingOwnInput)
            {
                interactionDetector.enabled = false;
                Debug.Log($"InteractionDetector disabled (normal mode)");
            }
            else
            {
                Debug.Log($"InteractionDetector left enabled (handling own input mode)");
            }
            
            // Always hide the interaction icon
            if (interactionDetector.interactionIcon != null)
            {
                interactionDetector.interactionIcon.SetActive(false);
                Debug.Log($"Interaction icon hidden");
            }
        }
        else
        {
            Debug.LogError($"InteractionDetector not found!");
        }
        
        Debug.Log($"=== NPC.FindAndDisablePlayerMovement() COMPLETE ===");
    }

    private void EnablePlayerMovement()
    {
        Debug.Log($"=== NPC.EnablePlayerMovement() ===");
        
        // Re-enable player movement
        if (playerController != null)
        {
            playerController.moveAction.Enable();
            Debug.Log("Player movement re-enabled after dialogue");
        }

        // Re-enable interaction detector
        if (interactionDetector != null)
        {
            interactionDetector.enabled = true;
            Debug.Log("InteractionDetector re-enabled after dialogue");
        }
    }

    private void ActivateEnemies()
    {
        // Find all enemies that are waiting for dialogue
        EnemyChase[] allEnemies = FindObjectsOfType<EnemyChase>();
        foreach (EnemyChase enemy in allEnemies)
        {
            if (enemy != null && enemy.waitForDialogue)
            {
                enemy.StartChasing();
            }
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

    // Called by CutsceneTrigger - NPC handles its own input
    public void ForceStartDialogueFromCutscene()
    {
        Debug.Log($"=== NPC.ForceStartDialogueFromCutscene() ===");
        
        if (dialogueData == null)
        {
            Debug.LogError("Cannot start dialogue - dialogueData is null!");
            return;
        }
        
        if (isDialogueActive)
        {
            Debug.LogWarning("Dialogue is already active!");
            return;
        }
        
        // Start dialogue in "handle own input" mode
        StartDialogue(true); // TRUE = NPC handles its own input via Update()
        
        Debug.Log($"=== NPC.ForceStartDialogueFromCutscene() COMPLETE ===");
    }
}