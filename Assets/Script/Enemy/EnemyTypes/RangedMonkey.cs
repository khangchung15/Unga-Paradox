using UnityEditor;
using UnityEngine;

public class RangedMonkey : Enemy
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Ranged Monkey Stats")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float wanderingMoveSpeed = 1.5f;
    [SerializeField] private float chasingMoveSpeed = 3f;
    [SerializeField] private int maxHealth = 50;
    protected override void Awake()
    {
        base.Awake();
        // set a custom detection range for this instance (does not require modifying the ScriptableObject)
        enemyHealth.StartingHealth = maxHealth;

    }

    protected override void Start()
    {
        base.Start();

        SetState(EnemyStateMachine.EnemyState.Idle);
        if (enemyDetection != null)
        {
            enemyDetection.DetectionRange = detectionRange;
        }
        else
        {
            Debug.LogWarning($"{name}: EnemyDetection not found; cannot set detection range.");
        }

        if (enemyMovement != null)
        {
            enemyMovement.DefaultMoveSpeed = wanderingMoveSpeed;
        }
        else
        {
            Debug.LogWarning($"{name}: EnemyMovement not found; cannot set walking speed.");
        }

        if (enemyChasing != null)
        {
            enemyChasing.ChaseSpeed = chasingMoveSpeed;
        }
        else
        {
            Debug.LogWarning($"{name}: EnemyChasing not found; cannot set chase speed.");
        }

        if (enemyHealth != null)
        {
            enemyHealth.StartingHealth = maxHealth;
        }
        else
        {
            Debug.LogWarning($"{name}: EnemyHealth not found; cannot set health.");
        }


    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

}
