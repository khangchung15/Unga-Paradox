using System;
using UnityEngine;
using static EnemyAttacking;

public class EnemyAttacking : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private float attackCooldown = 2.0f;
    [SerializeField] private string playerTag = "Player";
        
    private Transform playerTransform;
    private AttackZoneState currentAttackZoneState = AttackZoneState.None;

    [Header("Attack Mode")]
    [SerializeField] private AttackType attackType = AttackType.Melee;

    private EnemyAnimation enemyAnimation;
    private EnemyStateMachine stateMachine;
    private AttackHitbox attackHitbox;
    private ProjectileSpawn projectileSpawn;

    public enum AttackZoneState { None, Player, OutOfRange }
    public enum AttackType { Melee, Ranged }    

    public event Action OnPlayerEnteredRange;
    public event Action OnPlayerExitedRange;

    private void Awake()
    {
        var player = GameObject.FindGameObjectWithTag(playerTag);
        var parent = transform.parent;
        playerTransform = player != null ? player.transform : null;
        currentAttackZoneState = GetAttackZoneState();

        enemyAnimation = GetComponentInParent<EnemyAnimation>();
        stateMachine = GetComponentInParent<EnemyStateMachine>();

        attackHitbox = parent.GetComponentInChildren<AttackHitbox>();
        projectileSpawn = parent.GetComponentInChildren<ProjectileSpawn>();

        // Defensive logs to help debugging configuration issues (optional)
        if (attackType == AttackType.Melee && attackHitbox == null)
            Debug.LogWarning($"[{name}] AttackType=Melee but no AttackHitbox found in children.");
        if (attackType == AttackType.Ranged && projectileSpawn == null)
            Debug.LogWarning($"[{name}] AttackType=Ranged but no ProjectileSpawn found in children.");
    }

    private void OnEnable()
    {
        if (enemyAnimation == null)
            enemyAnimation = GetComponentInParent<EnemyAnimation>();

        if (enemyAnimation != null)
            enemyAnimation.AttackAnimationCompleted += HandleAttackAnimationCompleted;
    }

    private void OnDisable()
    {
        if (enemyAnimation != null)
            enemyAnimation.AttackAnimationCompleted -= HandleAttackAnimationCompleted;
    }


    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        // Only log / react when the attack zone state changes
        var newState = GetAttackZoneState();
        
        if (newState != currentAttackZoneState)
        {
           var prevState = currentAttackZoneState;
            currentAttackZoneState = newState;

            if (newState == AttackZoneState.Player && prevState != AttackZoneState.Player)
            {
                OnPlayerEnteredRange?.Invoke();
                //Debug.Log("Player entered attack range.");
            }
            else if (newState != AttackZoneState.Player && prevState == AttackZoneState.Player)
            {
                OnPlayerExitedRange?.Invoke();
                //Debug.Log("Player exited attack range.");
            }   
        }
    }

    private void FixedUpdate()
    {
        
    }
    public bool IsPlayerInRange => currentAttackZoneState == AttackZoneState.Player;
    
    public AttackZoneState GetAttackZoneState()
    {
        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            playerTransform = p != null ? p.transform : null;
            if (playerTransform == null)
                return AttackZoneState.None;
        }

        float dist = Vector2.Distance((Vector2)transform.parent.position, (Vector2)playerTransform.position);
        return dist <= attackRange ? AttackZoneState.Player : AttackZoneState.OutOfRange;
    }

    // New: when the animation completes, if we are still in BasicAttack and player still in range, restart the attack animation

    private void HandleAttackAnimationCompleted()
    {
        if (stateMachine == null || enemyAnimation == null) return;

        if (stateMachine.GetState() == EnemyStateMachine.EnemyState.BasicAttack && IsPlayerInRange)
        {
            // restart the attack animation (animator bool will be set back to true)
            enemyAnimation.RestartAttackAnimation();
        }
        else
        {
            // Attack finished and we are not re-attacking -> apply any queued state change (e.g. Chasing)
            stateMachine.ApplyPendingState();
        }
    }

    public void PerformAttackEffect()
    {
        if (attackType == AttackType.Melee)
        {
            // enable the hitbox for a short window (animation event calls this at hit-frame)
            if (attackHitbox != null)
            {
                attackHitbox.EnableHitbox(); // using event from attackHitBox script
            }
            else
            {
                Debug.LogWarning($"[{name}] PerformAttackEffect: Melee attack requested but no AttackHitbox assigned.");
            }

            return;
        }

        if (attackType == AttackType.Ranged)
        {
            projectileSpawn.Spawn();
            return;
        }
    }

    public void FinishAttackEffect()
    {
        if (attackType == AttackType.Melee)
        {
            if (attackHitbox != null)
            {
                attackHitbox.DisableHitbox();
            }
            else
            {
                Debug.LogWarning($"[{name}] PerformAttackEffect: Melee attack requested but no AttackHitbox assigned.");
            }
        }

        if (attackType == AttackType.Ranged)
        {
            // call projectile spawn finish logic here, use events from it

            return;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.parent.position, attackRange);
    }
}
