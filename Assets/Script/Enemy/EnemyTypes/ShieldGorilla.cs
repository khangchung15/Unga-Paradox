using UnityEditor;
using UnityEngine;

public class ShieldGorilla : Enemy
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Shield Gorilla Stats")]
    [SerializeField] private float detectionRange = 4.5f;
    [SerializeField] private float wanderingMoveSpeed = 1.5f;
    protected override void Awake()
    {
        base.Awake();
        // set a custom detection range for this instance (does not require modifying the ScriptableObject)
        
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
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }  
    
}
