using UnityEngine;

public class MechController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float moveDistance = 10f;
    
    [Header("Attack Settings")]
    public float minAttackInterval = 2f;
    public float maxAttackInterval = 5f;
    public float attackDuration = 1f;
    
    [Header("References")]
    public Animator animator;
    
    private Vector3 startPosition;
    private float targetX;
    private bool movingRight = true;
    private bool isAttacking = false;
    private float nextAttackTime;
    private float attackEndTime;
    
    void Start()
    {
        // Get Animator if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        startPosition = transform.position;
        targetX = startPosition.x + moveDistance;
        
        // Set first random attack time
        nextAttackTime = Time.time + Random.Range(minAttackInterval, maxAttackInterval);
    }
    
    void Update()
    {
        // Check if attack should end
        if (isAttacking)
        {
            if (Time.time >= attackEndTime)
            {
                // End attack, return to idle/running
                isAttacking = false;
                animator.SetBool("isAttack", false);
                animator.SetBool("isIdle", true);
                
                // Schedule next attack
                nextAttackTime = Time.time + Random.Range(minAttackInterval, maxAttackInterval);
            }
            return; // Don't move while attacking
        }
        
        // Check if it's time to attack
        if (Time.time >= nextAttackTime)
        {
            StartAttack();
            return;
        }
        
        // Move the mech
        if (movingRight)
        {
            transform.position += Vector3.right * moveSpeed * Time.deltaTime;
            
            // Check if reached target
            if (transform.position.x >= targetX)
            {
                movingRight = false;
                targetX = startPosition.x - moveDistance;
                Flip();
            }
        }
        else
        {
            transform.position += Vector3.left * moveSpeed * Time.deltaTime;
            
            // Check if reached target
            if (transform.position.x <= targetX)
            {
                movingRight = true;
                targetX = startPosition.x + moveDistance;
                Flip();
            }
        }
        
        // Update animator - mech is always moving in this example
        animator.SetBool("isRunning", true);
        animator.SetBool("isIdle", false);
    }
    
    void StartAttack()
    {
        isAttacking = true;
        attackEndTime = Time.time + attackDuration;
        
        // Trigger attack animation
        animator.SetBool("isAttack", true);
        animator.SetBool("isRunning", false);
        animator.SetBool("isIdle", false);
    }
    
    void Flip()
    {
        // Flip the mech sprite
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}