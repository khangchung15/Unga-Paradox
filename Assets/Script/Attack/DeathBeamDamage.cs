using UnityEngine;

public class DeathBeamDamage : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float damageCooldown = 0.5f;
    [SerializeField] private string targetTag = "Player";
    
    private float lastDamageTime = -999f;
    private bool hasDealtDamageThisActivation = false;
    private int triggerCallCount = 0;
    
    private void OnEnable()
    {
        lastDamageTime = -999f;
        hasDealtDamageThisActivation = false;
        triggerCallCount = 0;
    }
    
    private void OnDisable()
    {
        hasDealtDamageThisActivation = false;
        triggerCallCount = 0;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        triggerCallCount++;
        
        float timeSinceLastDamage = Time.time - lastDamageTime;
        
        Health playerHealth = other.GetComponent<Health>();
        if (playerHealth != null)
        {
            float healthBefore = playerHealth.currentHealth;
            
            playerHealth.TakeDamage(damage);
            lastDamageTime = Time.time;
            hasDealtDamageThisActivation = true;
            
            float healthAfter = playerHealth.currentHealth;
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        triggerCallCount++;
        
        if (!other.CompareTag(targetTag)) return;
        if (hasDealtDamageThisActivation) return;
        if (Time.time < lastDamageTime + damageCooldown) return;
        
        Health playerHealth = other.GetComponent<Health>();
        if (playerHealth != null)
        {
            float healthBefore = playerHealth.currentHealth;
            
            playerHealth.TakeDamage(damage);
            lastDamageTime = Time.time;
            hasDealtDamageThisActivation = true;
            
            float healthAfter = playerHealth.currentHealth;
        }
    }
}
