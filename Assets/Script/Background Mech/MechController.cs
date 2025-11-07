using UnityEngine;

public class MechController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float moveDistance = 10f;
    
    [Header("References")]
    public Animator animator;
    
    private Vector3 startPosition;
    private float targetX;
    private bool movingRight = true;
    
    void Start()
    {
        // Get Animator if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        startPosition = transform.position;
        targetX = startPosition.x + moveDistance;
    }
    
    void Update()
    {
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
    
    void Flip()
    {
        // Flip the mech sprite
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}