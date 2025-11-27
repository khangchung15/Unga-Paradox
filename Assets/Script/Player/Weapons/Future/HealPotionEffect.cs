using UnityEngine;

public class HealPotionEffect : PotionEffect
{
    [Header("Heal Settings")]
    [SerializeField] private float healAmount = 30f;
    
    protected override void ApplyEffect(Vector3 position)
    {
        Debug.Log($"[HealPotionEffect] Applying heal effect at {position} with radius {aoeRadius}");
        
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position, aoeRadius);
        
        Debug.Log($"[HealPotionEffect] Found {hitColliders.Length} colliders in range");
        
        foreach (Collider2D hitCollider in hitColliders)
        {
            Debug.Log($"[HealPotionEffect] Checking {hitCollider.name} (tag: {hitCollider.tag})");
            
            HealGameObject(hitCollider.gameObject);
        }
    }
    
    private void HealGameObject(GameObject target)
    {
        HealthFuture healthFuture = target.GetComponent<HealthFuture>();
        if (healthFuture == null && target.transform.parent != null)
        {
            healthFuture = target.transform.parent.GetComponent<HealthFuture>();
        }
        if (healthFuture != null)
        {
            healthFuture.AddHealth(healAmount);
            Debug.Log($"[HealPotion] Healed {target.name} for {healAmount} HP (HealthFuture)");
            return;
        }
        
        Health health = target.GetComponent<Health>();
        if (health == null && target.transform.parent != null)
        {
            health = target.transform.parent.GetComponent<Health>();
        }
        if (health != null)
        {
            health.AddHealth(healAmount);
            Debug.Log($"[HealPotion] Healed {target.name} for {healAmount} HP (Health)");
            return;
        }
        
        BossHealthFuture bossHealthFuture = target.GetComponent<BossHealthFuture>();
        if (bossHealthFuture == null && target.transform.parent != null)
        {
            bossHealthFuture = target.transform.parent.GetComponent<BossHealthFuture>();
        }
        if (bossHealthFuture != null)
        {
            bossHealthFuture.Heal(healAmount);
            Debug.Log($"[HealPotion] Healed Boss for {healAmount} HP");
            return;
        }
        
        Debug.Log($"[HealPotionEffect] {target.name} has no health component (checked parent too)");
    }
}
