using System.Collections;
using UnityEngine;

public class FrozenEntity : MonoBehaviour
{
    private bool isFrozen = false;
    private float freezeTimer = 0f;
    private float freezeDuration = 0f;
    
    private Animator animator;
    private Rigidbody2D rb;
    private MonoBehaviour[] scriptsToDisable;
    private SpriteRenderer mainSpriteRenderer;
    private GameObject freezeOverlayObject;
    private Color originalColor;
    
    private float originalAnimatorSpeed;
    private Vector2 originalVelocity;
    private float originalAngularVelocity;
    
    private BossShield bossShield;
    private BossController bossController;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        mainSpriteRenderer = GetComponent<SpriteRenderer>();
        bossShield = GetComponentInChildren<BossShield>();
        bossController = GetComponent<BossController>();
        
        Debug.Log($"[FrozenEntity] Component created on {gameObject.name}");
    }
    
    public bool IsFrozen => isFrozen;
    
    public void Freeze(float duration, Sprite overlaySprite, Color tint)
    {
        if (isFrozen)
        {
            freezeDuration = Mathf.Max(freezeDuration, duration);
            Debug.Log($"[FrozenEntity] {gameObject.name} already frozen! Extended duration to {freezeDuration}s");
            return;
        }
        
        isFrozen = true;
        freezeDuration = duration;
        freezeTimer = 0f;
        
        Debug.Log($"[FrozenEntity] Freezing {gameObject.name} for {duration}s");
        
        if (mainSpriteRenderer != null)
        {
            originalColor = mainSpriteRenderer.color;
            mainSpriteRenderer.color = tint;
        }
        
        if (animator != null)
        {
            originalAnimatorSpeed = animator.speed;
            animator.speed = 0f;
        }
        
        if (rb != null)
        {
            originalVelocity = rb.linearVelocity;
            originalAngularVelocity = rb.angularVelocity;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        if (bossController == null)
        {
            DisableMovementScripts();
        }
        else
        {
            DisableNonBossMovementScripts();
        }
        
        if (bossShield != null)
        {
            bossShield.SetFrozen(true);
            Debug.Log("[FrozenEntity] Notified boss shield of freeze");
        }
        
        if (overlaySprite != null)
        {
            CreateFreezeOverlay(overlaySprite);
        }
    }
    
    private void DisableMovementScripts()
    {
        scriptsToDisable = new MonoBehaviour[]
        {
            GetComponent<PlayerController>(),
            GetComponent<ScientistController>(),
            GetComponent<BossController>(),
            GetComponent<EnemyChase>(),
            GetComponent<EnemyPathfinding>()
        };
        
        foreach (var script in scriptsToDisable)
        {
            if (script != null && script.enabled)
            {
                script.enabled = false;
                Debug.Log($"[FrozenEntity] Disabled {script.GetType().Name}");
            }
        }
    }
    
    private void DisableNonBossMovementScripts()
    {
        scriptsToDisable = new MonoBehaviour[]
        {
            GetComponent<PlayerController>(),
            GetComponent<ScientistController>(),
            GetComponent<EnemyChase>(),
            GetComponent<EnemyPathfinding>()
        };
        
        foreach (var script in scriptsToDisable)
        {
            if (script != null && script.enabled)
            {
                script.enabled = false;
                Debug.Log($"[FrozenEntity] Disabled {script.GetType().Name}");
            }
        }
    }
    
    private void EnableMovementScripts()
    {
        if (scriptsToDisable == null) return;
        
        foreach (var script in scriptsToDisable)
        {
            if (script != null)
            {
                script.enabled = true;
                Debug.Log($"[FrozenEntity] Re-enabled {script.GetType().Name}");
            }
        }
        
        scriptsToDisable = null;
    }
    
    private void CreateFreezeOverlay(Sprite overlaySprite)
    {
        freezeOverlayObject = new GameObject("FreezeOverlay");
        freezeOverlayObject.transform.SetParent(transform);
        freezeOverlayObject.transform.localPosition = Vector3.zero;
        freezeOverlayObject.transform.localScale = Vector3.one;
        
        SpriteRenderer overlayRenderer = freezeOverlayObject.AddComponent<SpriteRenderer>();
        overlayRenderer.sprite = overlaySprite;
        overlayRenderer.sortingLayerName = "Player";
        overlayRenderer.sortingOrder = 100;
        overlayRenderer.color = new Color(1f, 1f, 1f, 0.7f);
    }
    
    private void Update()
    {
        if (!isFrozen) return;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        freezeTimer += Time.deltaTime;
        
        if (freezeTimer >= freezeDuration)
        {
            Unfreeze();
        }
    }
    
    private void Unfreeze()
    {
        isFrozen = false;
        
        Debug.Log($"[FrozenEntity] Unfreezing {gameObject.name}");
        
        if (mainSpriteRenderer != null)
        {
            mainSpriteRenderer.color = originalColor;
        }
        
        if (animator != null)
        {
            animator.speed = originalAnimatorSpeed;
        }
        
        EnableMovementScripts();
        
        if (bossShield != null)
        {
            bossShield.SetFrozen(false);
            Debug.Log("[FrozenEntity] Notified boss shield of unfreeze");
        }
        
        if (freezeOverlayObject != null)
        {
            Destroy(freezeOverlayObject);
        }
        
        Debug.Log($"[FrozenEntity] Destroying FrozenEntity component on {gameObject.name}");
        Destroy(this);
    }
}
