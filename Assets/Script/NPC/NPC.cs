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
            return false;
        }
        
        // Return true when not handling own input (normal interaction mode)
        // Return false when handling own input (cutscene mode - InteractionDetector should ignore us)
        bool canInteract = !isHandlingOwnInput;
        return canInteract;
    }

    public void Interact()
    {
        
        // If no dialogue data or the game is paused, then no dialogue would be active
        if (dialogueData == null || (PauseController.IsGamePaused && !isDialogueActive))
        {
            return;
        }

        // Don't block Interact() during active dialogue - this allows fast-forwarding
        if (isDialogueActive)
        {
            NextLine();
        }
        else
        {
            // Only check one-time dialogue when starting new dialogue
            if (oneTimeDialogue && hasBeenInteracted)
            {
                return;
            }
            StartDialogue(false); // Normal mode - not handling own input
        }
    }

    void StartDialogue(bool handleOwnInput)
    {
        isDialogueActive = true;
        isHandlingOwnInput = handleOwnInput;
        dialogueIndex = 0;

        // Mark as interacted if it's one-time dialogue
        if (oneTimeDialogue)
        {
            hasBeenInteracted = true;
        }
        FindAndDisablePlayerMovement();

        dialoguePanel.SetActive(true);
        PauseController.SetPause(true);
        StartCoroutine(TypeLine());
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
            Destroy(gameObject);
        }
    }

    private void FindAndDisablePlayerMovement()
    {
        // Find the player controller component
        playerController = FindObjectOfType<ScientistController>();
        if (playerController != null)
        {
            playerController.moveAction.Disable();
        }

        // Find the player animator to potentially freeze animations
        playerAnimator = FindObjectOfType<ScientistAnimator>();
        if (playerAnimator != null && playerAnimator.animator != null)
        {
            // Set to idle animation during dialogue
            playerAnimator.animator.SetBool("isIdle", true);
            playerAnimator.animator.SetBool("isRunning", false);
        }

        // Find the interaction detector for reference
        interactionDetector = FindObjectOfType<InteractionDetector>();
        if (interactionDetector != null)
        {
            if (!isHandlingOwnInput)
            {
                interactionDetector.enabled = false;
            }
            
            // Always hide the interaction icon
            if (interactionDetector.interactionIcon != null)
            {
                interactionDetector.interactionIcon.SetActive(false);
            }
        }
    }

    private void EnablePlayerMovement()
    {
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
            }
        }
    }

    // Called by CutsceneTrigger - NPC handles its own input
    public void ForceStartDialogueFromCutscene()
    {
        StartDialogue(true);
    }
}