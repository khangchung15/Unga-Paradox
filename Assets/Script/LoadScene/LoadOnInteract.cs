using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadOnInteract : MonoBehaviour, IInteractable
{
    [Header("Scene Loading Settings")]
    [Tooltip("Name of the scene to load")]
    public string sceneToLoad;
    
    [Header("Interaction Settings")]
    [Tooltip("If true, player can interact multiple times")]
    public bool canInteractMultipleTimes = true;

    private bool hasBeenInteracted = false;

    void Start()
    {
        // Ensure there's a trigger collider
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            
        }
        else if (!collider.isTrigger)
        {
            collider.isTrigger = true;
        }
    }

    public bool CanInteract()
    {
        // If can't interact multiple times and already interacted, return false
        if (!canInteractMultipleTimes && hasBeenInteracted)
        {
            return false;
        }
        return true;
    }

    public void Interact()
    {
        if (!CanInteract())
            return;

        // Mark as interacted if it's one-time interaction
        if (!canInteractMultipleTimes)
        {
            hasBeenInteracted = true;
        }

        // Load the scene
        LoadScene();
    }

    private void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}