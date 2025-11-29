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
    }
    
    public bool IsFrozen => isFrozen;
    
    public void Freeze(float duration, Sprite overlaySprite, Color tint)
    {
        if (isFrozen)
        {
            freezeDuration = Mathf.Max(freezeDuration, duration);
            return;
        }
        
        isFrozen = true;
        freezeDuration = duration;
        freezeTimer = 0f;
        
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
        
        if (animator != null)
        {
            animator.speed = 0f;
        }
        
        freezeTimer += Time.deltaTime;
        
        if (freezeTimer >= freezeDuration)
        {
            Unfreeze();
        }
    }
    
    public void Unfreeze()
    {
        if (!isFrozen)
        {
            return;
        }
        
        isFrozen = false;
        
        
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
        }
        
        if (freezeOverlayObject != null)
        {
            Destroy(freezeOverlayObject);
        }
        
        Destroy(this);
    }
}
