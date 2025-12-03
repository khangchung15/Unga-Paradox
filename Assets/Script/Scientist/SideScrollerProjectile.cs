using UnityEngine;

public class SideScrollerProjectile : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private LayerMask hitLayers;
    
    private bool hasHit = false;
    
    public void SetDamage(int damageAmount)
    {
        damage = damageAmount;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;
        
        if (collision.CompareTag("Player"))
            return;
        
        BossHealth bossHealth = collision.GetComponent<BossHealth>();
        if (bossHealth != null)
        {
            bossHealth.TakeDamage(damage);
            HitTarget(collision.transform.position);
            return;
        }
        
        BossShield shield = collision.GetComponent<BossShield>();
        if (shield != null)
        {
            return;
        }
        
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") || 
            collision.gameObject.layer == LayerMask.NameToLayer("Unwalkable"))
        {
            HitTarget(collision.transform.position);
        }
    }
    
    private void HitTarget(Vector3 hitPosition)
    {
        hasHit = true;
        
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, hitPosition, Quaternion.identity);
        }
        
        Destroy(gameObject);
    }
}
