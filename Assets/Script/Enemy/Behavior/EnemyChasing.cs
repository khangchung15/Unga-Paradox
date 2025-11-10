using UnityEngine;
using System;

// Simple direct chase implementation (no pathfinding)
public class EnemyChasing : MonoBehaviour
{
    [Header("Chasing Settings")]
    [SerializeField] private float chaseSpeed = 3f;
    [Tooltip("Distance (world units) at which we consider the target reached")]
    [SerializeField] private float stopDistance = 0.5f;

    private Rigidbody2D rb;
    private EnemyMovement enemyMovement;
    private GameObject player;

    readonly float wayPointThreshold = 0.1f; // legacy; not used for pathfinding
    private bool behaviorRunning;

    public event System.Action OnChaseStarted;
    public event System.Action OnReachedTarget;

    public float ChaseSpeed
    {
        get => chaseSpeed;
        set => chaseSpeed = Mathf.Max(0f, value);
    }

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>(); // from parent gameObject
        enemyMovement = GetComponentInParent<EnemyMovement>() ?? GetComponent<EnemyMovement>();
        enemyMovement.Initialized(rb, rb != null ? rb.transform : transform.parent ?? transform);

        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void OnDisable()
    {
        StopBehaviorInternal();
    }

    private void Update()
    {
        if (!behaviorRunning) return;

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
        }

        Vector2 moveFrom = rb != null ? rb.position : (Vector2)transform.parent.position;
        Vector2 playerPos = player.transform.position;
        float dist = Vector2.Distance(moveFrom, playerPos);

        // If close enough, consider reached and stop movement
        if (dist <= stopDistance)
        {
            enemyMovement.Stop();
            OnReachedTarget?.Invoke();
            return;
        }

        float effectiveChaseSpeed = GetEffectiveChaseSpeed();
        if (effectiveChaseSpeed <= 0f)
        {
            enemyMovement.Stop();
            return;
        }

        // Move directly toward player's current position
        enemyMovement.MoveTowards(playerPos, effectiveChaseSpeed);
    }

    public void StartBehavior()
    {
        if (behaviorRunning) return;
        behaviorRunning = true;
        OnChaseStarted?.Invoke();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");
    }

    public void StopBehavior()
    {
        if (!behaviorRunning) return;
        behaviorRunning = false;
        StopBehaviorInternal();
    }

    private void StopBehaviorInternal()
    {
        behaviorRunning = false;
        enemyMovement.Stop();
    }

    // override methods for enemy types with different chase speeds
    public void SetChaseSpeed(float speed)
    {
        chaseSpeed = Mathf.Max(0f, speed);
        Debug.Log(HelperFuncs.GetOwnerName(transform) + " Chase Speed Set To: " + chaseSpeed);
    }

    public float GetChaseSpeed() => chaseSpeed;

    private float GetEffectiveChaseSpeed()
    {
        if (chaseSpeed > 0f) return chaseSpeed;
        if (enemyMovement != null) return enemyMovement.DefaultMoveSpeed;
        return 0f;
    }
}