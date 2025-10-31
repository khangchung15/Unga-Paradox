using UnityEngine;
using System;

public class EnemyAnimation : MonoBehaviour
{
    private Animator animator;
    private EnemyMovement enemyMovement;
    private EnemyStateMachine stateMachine;
    private EnemyHealth enemyHealth;

    readonly float movementThreshold = 0.1f; // threshold to consider as moving

    [Header("Animation State Names (must match your Animator state names)")]
    [SerializeField] private string idleAnim = "Idle";
    [SerializeField] private string wanderingAnim = "Walk";      // changed to match Animator
    [SerializeField] private string chasingAnim = "Run";         // changed to match Animator
    [SerializeField] private string basicAttackAnim = "BasicAttack";
    [SerializeField] private string deadAnim = "Dead";

    [Header("Crossfade settings")]
    [SerializeField] private float transitionDuration = 0.05f;
    [SerializeField] private int animatorLayer = 0;

    public event Action AttackAnimationCompleted;

    // track last applied animation name so Update can avoid redundant CrossFade calls
    private string lastAppliedAnimation;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemyMovement = GetComponentInChildren<EnemyMovement>();
        stateMachine = GetComponent<EnemyStateMachine>();
        enemyHealth = GetComponent<EnemyHealth>();

        if (animator == null)
            throw new System.Exception("EnemyAnimation requires an Animator component.");
        if (enemyMovement == null)
            throw new System.Exception("EnemyAnimation requires an EnemyMovement component in children.");
        if (stateMachine == null)
            throw new System.Exception("EnemyAnimation requires an EnemyStateMachine component.");
        if (enemyHealth == null)
            throw new System.Exception("EnemyAnimation requires an EnemyHealth component.");

        lastAppliedAnimation = null;
    }

    private void OnEnable()
    {
        if (stateMachine != null)
            stateMachine.OnStateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        if (stateMachine != null)
            stateMachine.OnStateChanged -= OnStateChanged;
    }

    // We no longer drive animations every frame with params.
    // State changes are handled centrally via ApplyState() when the state machine changes.
    private void Update()
    {
        // If logical state is Idle or Wandering, pick Idle/Walk based on current movement
        if (stateMachine != null)
        {
            var logical = stateMachine.GetState();
            if (logical == EnemyStateMachine.EnemyState.Idle || logical == EnemyStateMachine.EnemyState.Wandering)
            {
                string desired = HasMovement() ? wanderingAnim : idleAnim;
                if (!string.IsNullOrEmpty(desired) && desired != lastAppliedAnimation)
                {
                    int layer = Mathf.Clamp(animatorLayer, 0, Mathf.Max(0, animator.layerCount - 1));
                    //Debug.Log($"[EnemyAnimation] Update: switching animation to '{desired}' (movementSpeed={GetMovementSpeed():F2})");
                    animator.CrossFade(desired, transitionDuration, layer, 0f);
                    lastAppliedAnimation = desired;
                }
            }
        }
    }

    private void OnStateChanged(EnemyStateMachine.EnemyState oldState, EnemyStateMachine.EnemyState newState)
    {  
        if (enemyHealth != null && enemyHealth.IsDead() && lastAppliedAnimation == deadAnim)
            return;
        ApplyState(newState);
    }

    // CrossFade to the animation corresponding to the logical state
    private void ApplyState(EnemyStateMachine.EnemyState state)
    {
        if (animator == null) return;

        string stateName = StateToAnimationName(state);
        if (string.IsNullOrEmpty(stateName)) 
            return;

        if (enemyHealth != null && enemyHealth.IsDead() && lastAppliedAnimation == deadAnim)
            return;
         //Debug.Log($"[EnemyAnimation] Transitioning to logical state '{state}' -> anim '{stateName}'");
        animator.CrossFade(stateName, transitionDuration);
        lastAppliedAnimation = stateName;
    }

    private string StateToAnimationName(EnemyStateMachine.EnemyState state)
    {

        if (enemyHealth != null && enemyHealth.IsDead())
            return deadAnim;

        switch (state)
        {
            case EnemyStateMachine.EnemyState.Idle:
                return HasMovement() ? wanderingAnim : idleAnim;
            case EnemyStateMachine.EnemyState.Wandering:
                return HasMovement() ? wanderingAnim : idleAnim;
            case EnemyStateMachine.EnemyState.Chasing:
                return chasingAnim;
            case EnemyStateMachine.EnemyState.BasicAttack:
                return basicAttackAnim;
            case EnemyStateMachine.EnemyState.Dead:
                return deadAnim;
            default:
                return idleAnim;
        }
    }
    private bool HasMovement()
    {
        return GetMovementSpeed() > movementThreshold;
    }

    private float GetMovementSpeed()
    {
        if (enemyMovement == null) return 0f;
        try { return enemyMovement.GetCurrentMovementSpeed; }
        catch { return 0f; }
    }

    // Called by animation event or StateMachineBehaviour when attack clip finishes.
    public void OnAttackAnimationComplete()
    {
        //Debug.Log($"[EnemyAnimation] OnAttackAnimationComplete invoked on '{gameObject.name}'");
        AttackAnimationCompleted?.Invoke();
    }

    // Immediately restart the attack animation (called by EnemyAttacking when conditions still hold).
    public void RestartAttackAnimation()
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(basicAttackAnim)) return;
        int layer = Mathf.Clamp(animatorLayer, 0, Mathf.Max(0, animator.layerCount - 1));
        //Debug.Log($"[EnemyAnimation] RestartAttackAnimation -> '{basicAttackAnim}'");
        animator.CrossFade(basicAttackAnim, transitionDuration, layer, 0f);
        lastAppliedAnimation = basicAttackAnim;
    }

    // Stop attack visuals and transition to a logical fallback state (uses current state machine state)
    public void StopAttackAnimation()
    {
        if (animator == null || stateMachine == null) return;
        // crossfade to whatever the logical current state maps to (e.g. Chasing or Idle)
        var current = stateMachine.GetState();
        var name = StateToAnimationName(current);
        if (!string.IsNullOrEmpty(name))
        {
            int layer = Mathf.Clamp(animatorLayer, 0, Mathf.Max(0, animator.layerCount - 1));
            //Debug.Log($"[EnemyAnimation] StopAttackAnimation -> transitioning to '{name}' (logical state '{current}')");
            animator.CrossFade(name, transitionDuration, layer, 0f);
            lastAppliedAnimation = name;
        }
    }
    public void TriggerAttackEffect()
    {
        var attack = GetComponentInChildren<EnemyAttacking>();
        if (attack != null)
        {
            attack.PerformAttackEffect();
        }
    }

    public void FinishAttackEffect()
    {
        var attack = GetComponentInChildren<EnemyAttacking>();
        if (attack != null)
        {
            attack.FinishAttackEffect();
        }
    }
    public void EnableAttackHitbox()
    {
        var hb = GetComponentInChildren<AttackHitbox>();
        if (hb == null)
        {
            Debug.LogWarning("[EnemyAnimation] EnableAttackHitbox: no AttackHitbox found in children.");
            return;
        }
        hb.EnableHitbox();
    }

    // Disable attack hitbox
    public void DisableAttackHitbox()
    {
        var hb = GetComponentInChildren<AttackHitbox>();
        if (hb == null) return;
        hb.DisableHitbox();
    }

    // Enable for a short time (frame-window by time)
    public void EnableAttackHitboxForSeconds(float seconds)
    {
        var hb = GetComponentInChildren<AttackHitbox>();
        if (hb == null) return;
        hb.EnableHitboxForSeconds(seconds);
    }

}