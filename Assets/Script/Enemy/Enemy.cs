using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    protected EnemyDetection enemyDetection;
    protected EnemyMovement enemyMovement;
    protected EnemyStateMachine stateMachine;
    protected Rigidbody2D rb;
    protected EnemyWandering enemyWandering;

    readonly float initialIdleDuration = 1f; // time to idle before starting wandering
    protected virtual void Awake()
    {
        enemyDetection = GetComponentInChildren<EnemyDetection>();
        rb = GetComponent<Rigidbody2D>();
        stateMachine = GetComponent<EnemyStateMachine>();
        enemyWandering = GetComponentInChildren<EnemyWandering>();
        enemyMovement = GetComponentInChildren<EnemyMovement>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>(); 
        }

        if (enemyWandering == null)
            throw new System.Exception("Enemy requires an EnemyWandering component in children.");
        if (enemyMovement == null)
            throw new System.Exception("Enemy requires an EnemyMovement component in children.");
        if (enemyDetection == null)
            throw new System.Exception("Enemy requires an EnemyDetection component in children.");
        if (stateMachine == null)
            throw new System.Exception("Enemy requires an EnemyStateMachine component.");

         enemyWandering.OnReachedWaypoint += HandleReachedDestination;
         enemyWandering.OnIdleComplete += HandleIdleComplete;
         stateMachine.OnStateChanged += HandleStateChanged;

    }
    protected virtual void Start()
    {
       
        SetState(EnemyStateMachine.EnemyState.Idle);
        if (initialIdleDuration <= 0f)
        {
            StartWandering();
        }
        else
        {
            Invoke(nameof(StartWandering), initialIdleDuration);
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

    // for debugging current state
    private void HandleStateChanged(EnemyStateMachine.EnemyState oldState, EnemyStateMachine.EnemyState newState)
    {
        Debug.Log($"{name} state changed: {oldState} -> {newState}");
    }
    private void HandleReachedDestination()
    {
        
        stateMachine.ChangeState(EnemyStateMachine.EnemyState.Idle);
    }

    private void HandleIdleComplete()
    {
        
        stateMachine.ChangeState(EnemyStateMachine.EnemyState.Wandering);
        enemyWandering.StartBehavior();
    }

    protected void SetState(EnemyStateMachine.EnemyState newState)
    {
        stateMachine.ChangeState(newState);

        switch (newState)
        {
            case EnemyStateMachine.EnemyState.Wandering:
                enemyWandering.StartBehavior();
                break;
            case EnemyStateMachine.EnemyState.Chasing:
                // Start chasing behavior
                break;
            case EnemyStateMachine.EnemyState.BasicAttack:
                // Start attack behavior
                break;
            case EnemyStateMachine.EnemyState.Dead:
                // Handle death
                break;
            case EnemyStateMachine.EnemyState.Idle:
            default:
                // Handle idle state
                enemyWandering.StopBehavior();
                break;
        }
    }
    public void ForceState(EnemyStateMachine.EnemyState state)
    {
        SetState(state);
    }
    private void OnSightStateChanged(EnemyDetection.SightState sightState)
    {
        // Map detection states to high-level enemy states
        switch (sightState)
        {
            case EnemyDetection.SightState.Player:
                stateMachine.ChangeState(EnemyStateMachine.EnemyState.Chasing);
                break;

            case EnemyDetection.SightState.Obstacle:
            case EnemyDetection.SightState.None:
            case EnemyDetection.SightState.OutOfRange:
            default:
                SetState(EnemyStateMachine.EnemyState.Wandering);
                break;
        }
    }

    private void StartWandering()
    {
        SetState(EnemyStateMachine.EnemyState.Wandering);
      
    }
    protected virtual void OnDestroy()
    {
        if (enemyDetection != null)
            enemyDetection.OnSightStateChanged -= OnSightStateChanged;
    }
}
