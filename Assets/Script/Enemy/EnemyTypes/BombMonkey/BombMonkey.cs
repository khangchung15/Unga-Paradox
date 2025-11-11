using UnityEditor;
using UnityEngine;

public class BombMonkey : Enemy
{
    [Header("Bomb Monkey Stats")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float wanderingMoveSpeed = 5f;
    [SerializeField] private float chasingMoveSpeed = 10f;
    [SerializeField] private int maxHealth = 50;

    private BombMonkeyExploder exploder;

    protected override void Awake()
    {
        base.Awake();
        enemyHealth.StartingHealth = maxHealth;
        exploder = GetComponent<BombMonkeyExploder>();
        if (exploder == null) exploder = gameObject.AddComponent<BombMonkeyExploder>();
    }

    protected override void Start()
    {
        base.Start();
        SetState(EnemyStateMachine.EnemyState.Idle);

        if (enemyDetection != null) enemyDetection.DetectionRange = detectionRange;
        if (enemyMovement != null) enemyMovement.DefaultMoveSpeed = wanderingMoveSpeed;
        if (enemyChasing != null) enemyChasing.ChaseSpeed = chasingMoveSpeed;
        if (enemyHealth != null) enemyHealth.StartingHealth = maxHealth;

        if (enemyAttacking != null)
            enemyAttacking.OnPlayerEnteredRange += HandleDetonateOnProximity;

        if (stateMachine != null)
            stateMachine.OnStateChanged += HandleStateChangedForExplosion;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (enemyAttacking != null)
            enemyAttacking.OnPlayerEnteredRange -= HandleDetonateOnProximity;
        if (stateMachine != null)
            stateMachine.OnStateChanged -= HandleStateChangedForExplosion;
    }

    private void HandleDetonateOnProximity()
    {
        if (exploder != null && !exploder.HasExploded)
            exploder.TriggerExplosion(selfDetonated: true);
    }

    private void HandleStateChangedForExplosion(EnemyStateMachine.EnemyState oldState, EnemyStateMachine.EnemyState newState)
    {
        if (newState == EnemyStateMachine.EnemyState.Dead && exploder != null && !exploder.HasExploded)
            exploder.TriggerExplosion(selfDetonated: false);
    }
}

