using UnityEngine;

public class MechAnimator : MonoBehaviour
{
    [Header("Animation Clips")]
    public AnimationClip idleAnimation;
    public AnimationClip moveAnimation;
    
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    
    private Animation animationComponent;
    private bool isMoving = false;
    
    void Start()
    {
        // Get or add Animation component
        animationComponent = GetComponent<Animation>();
        if (animationComponent == null)
        {
            animationComponent = gameObject.AddComponent<Animation>();
        }
        
        // Add animation clips if they exist
        if (idleAnimation != null)
        {
            animationComponent.AddClip(idleAnimation, "Idle");
        }
        
        if (moveAnimation != null)
        {
            animationComponent.AddClip(moveAnimation, "Move");
        }
        
        // Start with idle animation
        PlayIdle();
    }
    
    void Update()
    {
        HandleMovement();
        HandleAnimation();
    }
    
    void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            // Move the character
            Vector2 movement = new Vector2(horizontalInput * moveSpeed * Time.deltaTime, 0);
            transform.Translate(movement);
            
            // Flip character based on direction
            if (horizontalInput > 0)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            else if (horizontalInput < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            
            if (!isMoving)
            {
                isMoving = true;
                PlayMove();
            }
        }
        else
        {
            if (isMoving)
            {
                isMoving = false;
                PlayIdle();
            }
        }
    }
    
    void HandleAnimation()
    {
        // Additional animation logic can go here if needed
    }
    
    void PlayIdle()
    {
        if (idleAnimation != null && animationComponent != null)
        {
            animationComponent.CrossFade("Idle", 0.2f);
        }
    }
    
    void PlayMove()
    {
        if (moveAnimation != null && animationComponent != null)
        {
            animationComponent.CrossFade("Move", 0.2f);
        }
    }
}