using UnityEngine;

public class ThrownPotion : MonoBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] private LayerMask triggerLayers;
    [SerializeField] private float activationDelay = 0.1f;
    [SerializeField] private float reflectedActivationDelay = 0.05f;
    
    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private PotionEffect potionEffect;
    
    private bool hasTriggered = false;
    private bool isActive = false;
    private bool isReflected = false;
    private bool isBeingReflected = false;
    private float activationTimer = 0f;
    private float currentActivationDelay;
    
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
        
        currentActivationDelay = activationDelay;
    }
    
    public void MarkAsReflected()
    {
        isReflected = true;
        isBeingReflected = false;
        activationTimer = 0f;
        isActive = false;
        hasTriggered = false;
        currentActivationDelay = reflectedActivationDelay;
        Debug.Log($"[ThrownPotion] Potion reflected! Activation delay reduced to {reflectedActivationDelay}s");
    }
    
    private void Update()
    {
        if (!isActive && !isBeingReflected)
        {
            activationTimer += Time.deltaTime;
            if (activationTimer >= currentActivationDelay)
            {
                isActive = true;
                Debug.Log($"[ThrownPotion] Potion activated (reflected: {isReflected})");
            }
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasTriggered || isBeingReflected) return;
        
        if (IsShieldCollision(collision.gameObject))
        {
            isBeingReflected = true;
            Debug.Log("[ThrownPotion] Hit boss shield, waiting for reflection");
            return;
        }
        
        if (!isActive)
        {
            Debug.Log($"[ThrownPotion] Hit {collision.gameObject.name} but not active yet (timer: {activationTimer:F2}/{currentActivationDelay:F2})");
            return;
        }
        
        if (IsValidTarget(collision.gameObject))
        {
            hasTriggered = true;
            Debug.Log($"[ThrownPotion] Collision with {collision.gameObject.name} - TRIGGERING");
            TriggerPotionEffect(collision.contacts[0].point);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered || isBeingReflected) return;
        
        if (IsShieldCollision(collision.gameObject))
        {
            isBeingReflected = true;
            Debug.Log("[ThrownPotion] Hit boss shield (trigger), waiting for reflection");
            return;
        }
        
        if (!isActive)
        {
            Debug.Log($"[ThrownPotion] Triggered {collision.gameObject.name} but not active yet (timer: {activationTimer:F2}/{currentActivationDelay:F2})");
            return;
        }
        
        if (IsValidTarget(collision.gameObject))
        {
            hasTriggered = true;
            Debug.Log($"[ThrownPotion] Trigger with {collision.gameObject.name} - TRIGGERING");
            TriggerPotionEffect(collision.transform.position);
        }
    }
    
    private bool IsShieldCollision(GameObject target)
    {
        BossShield shield = target.GetComponent<BossShield>();
        if (shield != null && shield.IsShieldActive)
        {
            Debug.Log("[ThrownPotion] Detected active shield collision");
            return true;
        }
        
        if (target.transform.parent != null)
        {
            shield = target.transform.parent.GetComponent<BossShield>();
            if (shield != null && shield.IsShieldActive)
            {
                Debug.Log("[ThrownPotion] Detected active shield collision on parent");
                return true;
            }
        }
        
        return false;
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
