using UnityEngine;
using System;

public class EnemyAttacking : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private float attackCooldown = 2.0f;
    [SerializeField] private string playerTag = "Player";
        
    private Transform playerTransform;
    private AttackZoneState currentAttackZoneState = AttackZoneState.None;

    private EnemyAnimation enemyAnimation;
    private EnemyStateMachine stateMachine;

    public enum AttackZoneState { None, Player, OutOfRange }

    public event Action OnPlayerEnteredRange;
    public event Action OnPlayerExitedRange;

    private void Awake()
    {
        var player = GameObject.FindGameObjectWithTag(playerTag);
        playerTransform = player != null ? player.transform : null;
        currentAttackZoneState = GetAttackZoneState();

        enemyAnimation = GetComponentInParent<EnemyAnimation>();
        stateMachine = GetComponentInParent<EnemyStateMachine>();
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


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.parent.position, attackRange);
    }
}
