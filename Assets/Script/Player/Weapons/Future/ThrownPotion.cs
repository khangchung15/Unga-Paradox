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

    [Header("Visual Settings")]
    [SerializeField] private float spinSpeed = 360f;

    [Header("Acceleration Settings")]
    [SerializeField] private float accelerationForce = 10f;
    [SerializeField] private float maxSpeed = 25f;

    private bool hasTriggered = false;
    private bool isActive = false;
    private bool isReflected = false;
    private bool isBeingReflected = false;
    private float activationTimer = 0f;
    private float currentActivationDelay;
    private Vector2 throwDirection;

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
    }
    
    public void SetThrowDirection(Vector2 direction)
    {
        throwDirection = direction.normalized;
    }

    private void FixedUpdate()
    {
        if (rb != null && throwDirection != Vector2.zero && !hasTriggered)
        {
            Vector2 horizontalForce = new Vector2(throwDirection.x, 0) * accelerationForce;
            rb.AddForce(horizontalForce);
            
            Vector2 horizontalVelocity = new Vector2(rb.linearVelocity.x, 0);
            if (horizontalVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = new Vector2(
                    Mathf.Sign(rb.linearVelocity.x) * maxSpeed,
                    rb.linearVelocity.y
                );
            }
        }
    }



    private void Update()
    {
        if (!hasTriggered && spriteRenderer != null && spriteRenderer.enabled)
        {
            transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
        }
        
        if (!isActive && !isBeingReflected)
        {
            activationTimer += Time.deltaTime;
            if (activationTimer >= currentActivationDelay)
            {
                isActive = true;
            }
        }
    }

    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasTriggered || isBeingReflected) return;
        
        if (IsShieldCollision(collision.gameObject))
        {
            isBeingReflected = true;
            return;
        }
        
        if (!isActive && RequiresActivation(collision.gameObject))
        {
            return;
        }
        
        if (IsValidTarget(collision.gameObject))
        {
            hasTriggered = true;
            TriggerPotionEffect(collision.contacts[0].point);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Trigger hit: {collision.gameObject.name}, Layer: {LayerMask.LayerToName(collision.gameObject.layer)}, Tag: {collision.tag}, IsActive: {isActive}");
        if (hasTriggered || isBeingReflected) return;
        
        if (IsShieldCollision(collision.gameObject))
        {
            isBeingReflected = true;
            return;
        }
        
        if (!isActive && RequiresActivation(collision.gameObject))
        {
            return;
        }
        
        if (IsValidTarget(collision.gameObject))
        {
            hasTriggered = true;
            TriggerPotionEffect(collision.transform.position);
        }
    }
    
    private bool RequiresActivation(GameObject target)
    {
        if (isReflected && target.CompareTag("Player"))
        {
            return true;
        }
        
        if (!isReflected && target.CompareTag("Player"))
        {
            return true;
        }
        
        return false;
    }
    
    private bool IsShieldCollision(GameObject target)
    {
        BossShield shield = target.GetComponent<BossShield>();
        if (shield != null && shield.IsShieldActive)
        {
            return true;
        }
        
        if (target.transform.parent != null)
        {
            shield = target.transform.parent.GetComponent<BossShield>();
            if (shield != null && shield.IsShieldActive)
            {
                return true;
            }
        }
        
        return false;
    }
    
    private bool IsValidTarget(GameObject target)
    {
        if (target.CompareTag("Player")) 
        {
            return true;
        }
        if (target.CompareTag("Boss")) 
        {
            return true;
        }
        if (target.CompareTag("Enemy")) 
        {
            return true;
        }
        if (target.layer == LayerMask.NameToLayer("Ground")) 
        {
            return true;
        }
        return false;
    }
    
    private void TriggerPotionEffect(Vector3 position)
    {
        
        if (potionEffect != null)
        {
            potionEffect.TriggerEffect(position);
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
