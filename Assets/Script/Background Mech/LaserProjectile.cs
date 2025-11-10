using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    [Header("Laser Settings")]
    public float speed = 10f;
    public float lifetime = 3f;
    public int damage = 10;
    public int direction = 1; // Set by mech
    
    void Start()
    {
        // Set velocity
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(direction * speed, 0);
        }
        
        // Destroy after lifetime
        Destroy(gameObject, lifetime);
        
        // Flip sprite if going left
        if (direction < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Don't collide with the mech that fired it
        if (other.CompareTag("Enemy") || other.CompareTag("Player")) // Adjust tags as needed
        {
            // Apply damage if the other object has a Health component
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
            
            // Add impact effect here if desired
            Destroy(gameObject);
        }
    }
}