using System.Collections;
using UnityEngine;

public class AnimatedKnife : MonoBehaviour
{
    [SerializeField] private Animator knifeAnimator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D knifeCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private Transform playerTransform;
    private Transform bossTransform;
    private BossShield bossShield;
    private Transform currentTarget;
    private Vector3 hoverPosition;
    private float flyUpSpeed;
    private float hoverDuration;
    private float throwRange;
    private float throwSpeed;
    private int bossThrowDamage;
    private int playerBackfireDamage;
    private int currentDamage;

    private float hoverTimer = 0f;
    private float aimingTimer = 0f;
    private bool hasHit = false;
    
    private enum KnifeState
    {
        FlyingUp,
        Hovering,
        Aiming,
        FlyingToTarget
    }

    private const float AIMING_DURATION = 1.2f;

    
    private KnifeState currentState = KnifeState.FlyingUp;
    
    private void Awake()
    {
        if (knifeAnimator == null)
        {
            knifeAnimator = GetComponent<Animator>();
        }
        
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
        if (knifeCollider == null)
        {
            knifeCollider = GetComponent<Collider2D>();
        }
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = "Ground";
            spriteRenderer.sortingOrder = 6;
        }
    }
    
    public void Initialize(
        Transform player,
        Transform boss,
        BossShield shield,
        Vector3 targetHoverPosition,
        float upSpeed,
        float hoverTime,
        float range,
        float speed,
        int bossDamage,
        int playerDamage,
        string animationName)
    {
        playerTransform = player;
        bossTransform = boss;
        bossShield = shield;
        hoverPosition = targetHoverPosition;
        flyUpSpeed = upSpeed;
        hoverDuration = hoverTime;
        throwRange = range;
        throwSpeed = speed;
        bossThrowDamage = bossDamage;
        playerBackfireDamage = playerDamage;
        
        transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        
        if (knifeCollider != null)
        {
            knifeCollider.enabled = false;
        }
        
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
        }
        
        if (knifeAnimator != null)
        {
            knifeAnimator.Play(animationName);
        }
        
        currentState = KnifeState.FlyingUp;
        hoverTimer = 0f;
        aimingTimer = 0f;
        hasHit = false;
    }

    
    private void Update()
    {
        switch (currentState)
        {
            case KnifeState.FlyingUp:
                FlyToHoverPosition();
                break;
                
            case KnifeState.Hovering:
                HoverAndCheckConditions();
                break;
                
            case KnifeState.Aiming:
                AimAtTarget();
                break;
                
            case KnifeState.FlyingToTarget:
                FlyToTarget();
                break;
        }
    }

    
    private void FlyToHoverPosition()
    {
        Vector2 direction = (hoverPosition - transform.position).normalized;
        
        if (rb != null)
        {
            rb.linearVelocity = direction * flyUpSpeed;
        }
        
        if (Vector3.Distance(transform.position, hoverPosition) < 0.1f)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            
            transform.position = hoverPosition;
            currentState = KnifeState.Hovering;
            hoverTimer = 0f;
        }
    }
    
    private void HoverAndCheckConditions()
    {
        transform.position = hoverPosition;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        hoverTimer += Time.deltaTime;
        
        if (IsInRangeOfBoss() && IsBossShieldDown())
        {
            StartFlyingToBoss();
        }
        else if (hoverTimer >= hoverDuration)
        {
            StartFlyingToPlayer();
        }
    }
    
    private bool IsInRangeOfBoss()
    {
        if (bossTransform == null || playerTransform == null)
        {
            return false;
        }
        
        float distance = Vector3.Distance(playerTransform.position, bossTransform.position);
        return distance <= throwRange;
    }
    
    private bool IsBossShieldDown()
    {
        if (bossShield == null)
        {
            return true;
        }
        
        return !bossShield.IsShieldActive;
    }
    
    private void StartFlyingToBoss()
    {
        currentTarget = bossTransform;
        currentDamage = bossThrowDamage;
        aimingTimer = 0f;
        currentState = KnifeState.Aiming;
    }

    private void StartFlyingToPlayer()
    {
        currentTarget = playerTransform;
        currentDamage = playerBackfireDamage;
        aimingTimer = 0f;
        currentState = KnifeState.Aiming;
    }

    private void AimAtTarget()
    {
        if (currentTarget == null) return;
        
        transform.position = hoverPosition;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        
        float currentAngle = transform.rotation.eulerAngles.z;
        float smoothAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * 10f);
        transform.rotation = Quaternion.Euler(0f, 0f, smoothAngle);
        
        aimingTimer += Time.deltaTime;
        
        if (aimingTimer >= AIMING_DURATION)
        {
            if (knifeCollider != null)
            {
                knifeCollider.enabled = true;
            }
            currentState = KnifeState.FlyingToTarget;
        }
    }

    
    private void FlyToTarget()
    {
        if (currentTarget == null) return;
        
        if (currentTarget == bossTransform && !IsBossShieldDown())
        {
            Destroy(gameObject);
            return;
        }
        
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        
        if (rb != null)
        {
            rb.linearVelocity = direction * throwSpeed;
        }
        else
        {
            transform.position += (Vector3)direction * throwSpeed * Time.deltaTime;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;
        
        if (collision.CompareTag("Boss"))
        {
            hasHit = true;
            
            try
            {
                if (currentTarget == bossTransform)
                {
                    BossHealthFuture bossHealth = collision.GetComponent<BossHealthFuture>();
                    if (bossHealth != null)
                    {
                        bossHealth.TakeDamage(currentDamage);
                    }
                }
            }
            catch (System.Exception e)
            {
            }
            finally
            {
                Destroy(gameObject);
            }
            return;
        }
        
        if (collision.CompareTag("Player"))
        {
            hasHit = true;
            
            try
            {
                if (currentTarget == playerTransform)
                {
                    HealthFuture playerHealth = collision.GetComponent<HealthFuture>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(currentDamage);
                    }
                }
            }
            catch (System.Exception e)
            {
            }
            finally
            {
                Destroy(gameObject);
            }
            return;
        }
    }
}
