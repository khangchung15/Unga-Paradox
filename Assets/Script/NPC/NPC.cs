using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;

    [Header("Objects to Destroy After Dialogue")]
    [Tooltip("GameObjects that will be destroyed when dialogue ends")]
    public GameObject[] objectsToDestroy;

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;
    private ScientistController playerController;
    private InteractionDetector interactionDetector;
    private ScientistAnimator playerAnimator;

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        // If no dialogue data or the game is paused, then no dialogue would be active
        if (dialogueData == null || (PauseController.IsGamePaused && !isDialogueActive))
            return;

        if (isDialogueActive)
        {
            NextLine();
        }
        else
        {
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;

        // Find and disable player movement
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
        dialogueText.SetText("");
        dialoguePanel.SetActive(false);
        PauseController.SetPause(false);
        
        EnablePlayerMovement();
        ActivateEnemies();
        DestroyObjects(); // Add this line
    }

    private void FindAndDisablePlayerMovement()
    {
        // Find the player controller component
        playerController = FindObjectOfType<ScientistController>();
        if (playerController != null)
        {
            // Disable the moveAction to prevent movement input
            playerController.moveAction.Disable();
            Debug.Log("Player movement disabled for dialogue");
        }

        // Find the player animator to potentially freeze animations
        playerAnimator = FindObjectOfType<ScientistAnimator>();
        if (playerAnimator != null && playerAnimator.animator != null)
        {
            // Set to idle animation during dialogue
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
    }

    private void EnablePlayerMovement()
    {
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
}