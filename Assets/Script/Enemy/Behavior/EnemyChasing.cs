using Pathfinding;
using UnityEngine;
using System;


// testing script for enemy chasing behavior using A* Pathfinding
public class EnemyChasing : MonoBehaviour
{
    [Header("Chasing Settings")]
    public float chaseSpeed = 3f;
    [Tooltip("How often (s) to poll for new paths if the target moves (minimum).")]
    [SerializeField] private float minRepathTime = 0.15f;
    [Tooltip("Minimum target displacement (world units) required to request a new path.")]
    [SerializeField] private float minRepathDistance = 0.3f;
    [Tooltip("When following a path, look this far ahead (world units) to reduce sharp turning.")]
    [SerializeField] private float lookAheadDistance = 0.6f;

    private float pathUpdateInterval = 0.5f; // fallback interval for InvokeRepeating if used

    private Rigidbody2D rb;
    private EnemyMovement enemyMovement;
    private Seeker seeker;
    private Path currentPath;
    private GameObject player;

    readonly float wayPointThreshold = 0.1f; // distance to waypoint to consider it reached
    private int currentWaypointIndex = 0; // to track current waypoint in path to destination for pathfinding

    private bool behaviorRunning;
    private float lastPathRequestTime;
    private Vector3 lastRequestedTargetPos = Vector3.positiveInfinity;

    public event Action OnChaseStarted;
    public event Action OnReachedTarget;
    public event Action OnPathReady;

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

        // prefer a Seeker on parent chain; parent (Enemy) can inject a different Seeker via SetSeeker(...)
        seeker = GetComponentInParent<Seeker>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void OnDisable()
    {
        StopBehaviorInternal();
    }

    void Update()
    {
        if (!behaviorRunning) return;

        // guard: ensure path and vectorPath present and non-empty (check null before Count)
        if (currentPath == null || currentPath.vectorPath == null || currentPath.vectorPath.Count == 0)
        {
            //enemyMovement.Stop();
            return;
        }

        // clamp currentWaypointIndex to valid range (prevents ArgumentOutOfRange if path shortened)
        currentWaypointIndex = Mathf.Clamp(currentWaypointIndex, 0, currentPath.vectorPath.Count - 1);

        // choose a lookahead waypoint to reduce quick snapping / turning
        Vector2 moveFrom = rb != null ? rb.position : (Vector2)transform.parent.position;
        int idx = currentWaypointIndex;
        int last = currentPath.vectorPath.Count - 1;

        // advance current index while we are close to it
        while (idx < last && Vector2.Distance(moveFrom, (Vector2)currentPath.vectorPath[idx]) < wayPointThreshold)
        {
            idx++;
        }
        currentWaypointIndex = idx;

        Vector2 nextWayPoint = currentPath.vectorPath[currentWaypointIndex];
        float effectiveChaseSpeed = ChaseSpeed;

        // defensive: ensure speed > 0 before moving
        if (effectiveChaseSpeed <= 0f)
        {
            //enemyMovement.Stop();
            return;
        }

        enemyMovement.MoveTowards(nextWayPoint, effectiveChaseSpeed);

        // advance index when close to the (non-lookahead) immediate waypoint
        if (currentWaypointIndex <= last && Vector2.Distance(moveFrom, (Vector2)currentPath.vectorPath[currentWaypointIndex]) < wayPointThreshold)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex > last)
            {
                // reached beyond last waypoint
                currentWaypointIndex = last;
                //enemyMovement.Stop();
                OnReachedTarget?.Invoke();
            }
        }
    }

    public void StartBehavior()
    {
        if (behaviorRunning)
            return;
        behaviorRunning = true;
        OnChaseStarted?.Invoke();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        // Reset last request so first UpdatePath runs immediately
        lastPathRequestTime = -999f;
        lastRequestedTargetPos = Vector3.positiveInfinity;

        UpdatePath();
        if (pathUpdateInterval > 0f)
            InvokeRepeating(nameof(UpdatePath), pathUpdateInterval, pathUpdateInterval);
    }

    public void StopBehavior()
    {
        if (!behaviorRunning)
            return;
        behaviorRunning = false;
        // cancel any in-progress request
        StopBehaviorInternal();
    }

    private void StopBehaviorInternal()
    {
        CancelInvoke(nameof(UpdatePath));
        currentPath = null;
        currentWaypointIndex = 0;
        enemyMovement.Stop();
    }

    private void UpdatePath()
    {
        // only run while chasing
        if (!behaviorRunning) return;
        if (seeker == null || rb == null || player == null) return;

        // Throttle path requests: time + target movement delta
        float now = Time.time;
        if (now - lastPathRequestTime < minRepathTime)
            return;

        Vector3 targetPos = player.transform.position;
        if (lastRequestedTargetPos != Vector3.positiveInfinity && Vector3.Distance(targetPos, lastRequestedTargetPos) < minRepathDistance)
            return;

        if (seeker.IsDone())
        {
            seeker.StartPath(rb.position, targetPos, OnPathComplete);
            lastPathRequestTime = now;
            lastRequestedTargetPos = targetPos;
        }
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error && p.vectorPath != null)
        {
            currentPath = p;
            currentWaypointIndex = 0;
            OnPathReady?.Invoke();
        }
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
        if (chaseSpeed > 0f)
            return chaseSpeed;
        if (enemyMovement != null)
            return enemyMovement.DefaultMoveSpeed;
        return 0f;
    }
}