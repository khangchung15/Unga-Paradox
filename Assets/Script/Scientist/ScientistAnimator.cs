using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handles interactions with the animator component of the player
/// It reads the player's state from the controller and animates accordingly
/// </summary>
public class ScientistAnimator : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The player controller script to read state information from")]
    public ScientistController playerController;
    [Tooltip("The animator component that controls the player's animations")]
    public Animator animator;

    void Start()
    {
        // Try to get the controller if not assigned
        if (playerController == null)
        {
            playerController = GetComponent<ScientistController>();
        }
        
        // Try to get the animator if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        ReadPlayerStateAndAnimate();
    }

    void Update()
    {
        ReadPlayerStateAndAnimate();
    }

    void ReadPlayerStateAndAnimate()
    {
        if (animator == null || playerController == null)
        {
            return;
        }
        animator.SetBool("isIdle", playerController.state == ScientistController.PlayerState.Idle);
        animator.SetBool("isRunning", playerController.state == ScientistController.PlayerState.Walk);
        animator.SetBool("isDead", playerController.state == ScientistController.PlayerState.Dead);
    }
}