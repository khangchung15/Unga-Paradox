using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null;
    public GameObject interactionIcon;

    void Start()
    {
        if (interactionIcon != null)
            interactionIcon.SetActive(false);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && interactableInRange != null)
        {
            Debug.Log($"Attempting to interact with: {interactableInRange}");
            interactableInRange.Interact();
        }
        else if (context.performed)
        {
            Debug.Log("Interact button pressed but no interactable in range");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable))
        {
            if (interactable.CanInteract())
            {
                interactableInRange = interactable;
                Debug.Log($"Interactable found: {interactable}. Can interact: true");
                if (interactionIcon != null)
                    interactionIcon.SetActive(true);
            }
            else
            {
                Debug.Log($"Interactable found: {interactable}. Can interact: false");
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
        {
            Debug.Log($"Interactable left range: {interactable}");
            interactableInRange = null;
            if (interactionIcon != null)
                interactionIcon.SetActive(false);
        }
    }
}