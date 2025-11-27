using UnityEngine;

public class ThrownPotion : MonoBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] private LayerMask triggerLayers;
    [SerializeField] private float activationDelay = 0.1f;
    
    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private PotionEffect potionEffect;
    
    private bool hasTriggered = false;
    private bool isActive = false;
    private float activationTimer = 0f;
    
    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        potionEffect = GetComponent<PotionEffect>();
        
        if (rb != null)
        {
            rb.gravityScale = 1f;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = "Ground";
            spriteRenderer.sortingOrder = 6;
        }
    }
    
    private void Update()
    {
        if (!isActive)
        {
            activationTimer += Time.deltaTime;
            if (activationTimer >= activationDelay)
            {
                isActive = true;
                Debug.Log("[ThrownPotion] Potion activated and ready to trigger");
            }
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isActive || hasTriggered) return;
        
        if (IsValidTarget(collision.gameObject))
        {
            hasTriggered = true;
            Debug.Log($"[ThrownPotion] Collision with {collision.gameObject.name}");
            TriggerPotionEffect(collision.contacts[0].point);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive || hasTriggered) return;
        
        if (IsValidTarget(collision.gameObject))
        {
            hasTriggered = true;
            Debug.Log($"[ThrownPotion] Trigger with {collision.gameObject.name}");
            TriggerPotionEffect(collision.transform.position);
        }
    }
    
    private bool IsValidTarget(GameObject target)
    {
        if (target.CompareTag("Player")) 
        {
            Debug.Log("[ThrownPotion] Valid target: Player");
            return true;
        }
        if (target.CompareTag("Boss")) 
        {
            Debug.Log("[ThrownPotion] Valid target: Boss");
            return true;
        }
        if (target.CompareTag("Enemy")) 
        {
            Debug.Log("[ThrownPotion] Valid target: Enemy");
            return true;
        }
        if (target.layer == LayerMask.NameToLayer("Ground")) 
        {
            Debug.Log("[ThrownPotion] Valid target: Ground");
            return true;
        }
        
        Debug.Log($"[ThrownPotion] Invalid target: {target.name} (tag: {target.tag}, layer: {LayerMask.LayerToName(target.layer)})");
        return false;
    }
    
    private void TriggerPotionEffect(Vector3 position)
    {
        Debug.Log($"[ThrownPotion] Triggering potion effect at {position}");
        
        if (potionEffect != null)
        {
            potionEffect.TriggerEffect(position);
        }
        else
        {
            Debug.LogWarning("[ThrownPotion] No PotionEffect component found!");
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        if (rb != null)
        {
            rb.simulated = false;
        }
        
        Destroy(gameObject, 0.5f);
    }
}
