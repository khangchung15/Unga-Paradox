using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScientistAnimator : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The player controller script to read state information from")]
    public ScientistController playerController;
    [Tooltip("The animator component that controls the player's animations")]
    public Animator animator;

    private bool hasEnteredDeathState = false;

    void Start()
    {
        if (playerController == null)
        {
            playerController = GetComponent<ScientistController>();
        }
        
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        ReadPlayerStateAndAnimate();
    }

    void Update()
    {
        if (!hasEnteredDeathState)
        {
            ReadPlayerStateAndAnimate();
        }
    }

    void ReadPlayerStateAndAnimate()
    {
        if (animator == null || playerController == null)
        {
            return;
        }

        if (playerController.state == ScientistController.PlayerState.Dead)
        {
            if (!hasEnteredDeathState)
            {
                hasEnteredDeathState = true;
                animator.SetBool("isIdle", false);
                animator.SetBool("isRunning", false);
                animator.SetBool("isDead", true);
            }
        }
        else
        {
            animator.SetBool("isIdle", playerController.state == ScientistController.PlayerState.Idle);
            animator.SetBool("isRunning", playerController.state == ScientistController.PlayerState.Walk);
            animator.SetBool("isDead", false);
        }
    }
}
