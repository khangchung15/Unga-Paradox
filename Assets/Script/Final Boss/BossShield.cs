using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class BossShield : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Collider2D shieldCollider;
    
    [Header("Animation Names")]
    [SerializeField] private string chargedAnimationName = "charged";
    [SerializeField] private string brokenAnimationName = "broken";
    [SerializeField] private string regenerateAnimationName = "regenerate";
    
    [Header("Regeneration Settings")]
    [SerializeField] private float regenerationDelay = 3f;
    
    [Header("Boss Damage")]
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private int damagePerHit = 25;
    
    public UnityAction OnShieldBroken;
    public UnityAction OnShieldRegenerateComplete;
    
    private bool isBroken = false;
    
    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
            
        if (shieldCollider == null)
            shieldCollider = GetComponent<Collider2D>();
            
        if (bossHealth == null)
            bossHealth = GetComponentInParent<BossHealth>();
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isBroken) return;
        
        DeathBeamDamage deathBeam = collision.GetComponent<DeathBeamDamage>();
        if (deathBeam != null)
        {
            BreakShield();
        }
    }
    
    private void BreakShield()
    {
        isBroken = true;
        
        animator.Play(brokenAnimationName);
        
        DamageBoss();
        
        OnShieldBroken?.Invoke();
        
        StartCoroutine(RegenerateShieldAfterDelay());
    }
    
    private IEnumerator RegenerateShieldAfterDelay()
    {
        AnimationClip brokenClip = GetAnimationClip(brokenAnimationName);
        if (brokenClip != null)
        {
            yield return new WaitForSeconds(brokenClip.length);
        }
        
        yield return new WaitForSeconds(regenerationDelay);
        
        animator.Play(regenerateAnimationName);
        
        AnimationClip regenerateClip = GetAnimationClip(regenerateAnimationName);
        if (regenerateClip != null)
        {
            yield return new WaitForSeconds(regenerateClip.length);
        }
        
        animator.Play(chargedAnimationName);
        
        isBroken = false;
        
        OnShieldRegenerateComplete?.Invoke();
    }
    
    private void DamageBoss()
    {
        if (bossHealth != null)
        {
            bossHealth.TakeDamage(damagePerHit);
        }
    }
    
    private AnimationClip GetAnimationClip(string clipName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return null;
            
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip;
        }
        return null;
    }
}
