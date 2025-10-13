using UnityEngine;
using System.Collections;
using Pathfinding;
using System;

// this script mainly handles the wandering waypoint logic for enemies
public class EnemyWandering : MonoBehaviour
{

    public event Action OnReachedWaypoint;
    public event Action OnIdleComplete;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Wandering Settings")]
    private float moveSpeed;

    [Tooltip("Radius for wandering area.")]
    public float wanderRadius = 5f;

    [Tooltip("Minimum Rest Time between Patrol Points")]
    public float minRestTime = 0.5f;

    [Tooltip("Maximum Rest Time between Patrol Points")]
    public float maxRestTime = 2f;

    private Rigidbody2D rb;
    private EnemyMovement enemyMovement;
    private Seeker seeker;
    private Path currentPath;
    private EnemyStateMachine stateMachine;

    readonly float wayPointThreshold = 0.1f; // distance to waypoint to consider it reached
    private Vector2 homeLocation;
    private bool isResting;
    public Vector2 target;
    private int currentWaypointIndex = 0; // to track current waypoint in path to destination for pathfinding

    // for debugging purposes
    private Vector2 lastSelectedWanderPoint;

    private Coroutine idleCoroutine;
    private bool behaviorRunning;
    

    private void Awake()
    {
        homeLocation = transform.parent != null ? (Vector2)transform.parent.position : (Vector2)transform.position;
        rb = GetComponentInParent<Rigidbody2D>(); // from parent gameObject 
        enemyMovement = gameObject.GetComponent<EnemyMovement>();
        enemyMovement.Initialized(rb, transform.parent);
        seeker = GetComponentInParent<Seeker>();
        stateMachine = GetComponentInParent<EnemyStateMachine>();

        //moveSpeed = enemyMovement.DefaultMoveSpeed; // inherit the enemy movement speed based on enemy type
        
        if (rb == null)
            throw new System.Exception("EnemyWandering requires a Rigidbody2D component in parent.");
        if (enemyMovement == null)
            throw new System.Exception("EnemyWandering requires an EnemyMovement component in children.");
        if ( seeker == null)
            throw new System.Exception("EnemyWandering requires a Seeker component in parent.");
        if (stateMachine == null)
            throw new System.Exception("EnemyWandering requires an EnemyStateMachine component in parent.");
    }

    private void OnEnable()
    {
        //if (stateMachine.GetState() == EnemyStateMachine.EnemyState.Wandering)
        //    StartCoroutine(IdleAndSetNewWanderPoint());
    }
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {

        
        if (isResting || currentPath == null || currentPath.vectorPath.Count == 0)
        {
            enemyMovement.Stop();
            return;
        }
        //Debug.Log("balls");
        Vector2 nextWayPoint = currentPath.vectorPath[currentWaypointIndex];
        float moveSpeed = enemyMovement.DefaultMoveSpeed; // use configured speed
        enemyMovement.MoveTowards(nextWayPoint, moveSpeed);

        if (Vector2.Distance(transform.parent.position, nextWayPoint) < wayPointThreshold)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= currentPath.vectorPath.Count)
            {
                enemyMovement.Stop();
                OnReachedWaypoint?.Invoke();
                StartCoroutine(IdleAndSetNewWanderPoint());
                
            }
        }   
    }

    public void StartBehavior()
    {
        if (behaviorRunning)
            return;
        behaviorRunning = true;

        if (currentPath == null || currentPath.vectorPath == null || currentPath.vectorPath.Count == 0)
        {
            StartIdleCoroutine();
        }
    }
    public void StopBehavior()
    {
        behaviorRunning = false;
        StopIdleCoroutine();
        StopAllPathwork();
        enemyMovement.Stop();
    }
    private void StartIdleCoroutine()
    {
        if (idleCoroutine != null) 
            return;
        idleCoroutine = StartCoroutine(IdleAndSetNewWanderPoint());
    }
    private void StopIdleCoroutine()
    {
        if (idleCoroutine == null)
            return;
        StopCoroutine(idleCoroutine);
        idleCoroutine = null;
        isResting = false;
    }

    private void StopAllPathwork()
    {
        currentPath = null;
        currentWaypointIndex = 0;
    }
    public IEnumerator IdleAndSetNewWanderPoint()
    {
        //Debug.Log("Fuck you");
        if (!behaviorRunning)
        {
            idleCoroutine = null;
            yield break;
        }
        
        isResting = true;
        rb.linearVelocity = Vector2.zero;
        float restTime = UnityEngine.Random.Range(minRestTime, maxRestTime);
        yield return new WaitForSeconds(restTime);

        if (!behaviorRunning)
        {
            isResting = false;
            idleCoroutine = null;
            yield break;
        }

       
        target = GetRandomWanderPoint();
        Debug.Log(HelperFuncs.GetOwnerName(transform) + " New Wander Point Set: " + target);

        if (seeker != null && behaviorRunning)
        {
            seeker.StartPath(rb.position, target, OnPathComplete);
        }

        isResting = false;
        idleCoroutine = null;
        OnIdleComplete?.Invoke();

    }

    private void OnPathComplete(Path p)
    {
        if (!p.error && p.vectorPath != null)
        {
            currentPath = p;
            currentWaypointIndex = 0;
            
        }
    }
    private Vector2 GetRandomWanderPoint() // the enemy will wander within the circle defined by wanderRadius started off with homeLocation
    {
        int maxAttempts = 4; // limit attempts to optimize, if no valid point, goes back to base
        for (int i = 0; i < maxAttempts; i++)
        {
            float angle = UnityEngine.Random.Range(0f, 2f * Mathf.PI);
            float radius = UnityEngine.Random.Range(0f, wanderRadius); // ensure they will never wander out of the circle
            Vector2 randomPoint = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            Vector2 potentialPoint = homeLocation + randomPoint;
            lastSelectedWanderPoint = potentialPoint; // for debugging purposes
            Collider2D hit = Physics2D.OverlapCircle(homeLocation + randomPoint, 0.2f, LayerMask.GetMask("Unwalkable")); // check if this dumbass chooses a wall to walk to again

            if (hit == null)
            {
                return potentialPoint;

            }
            else
            {
                Debug.Log(HelperFuncs.GetOwnerName(transform) +
                    " Oops! I chose a wall, silly me :P . At: " + potentialPoint);
            }
        }
        return homeLocation; // if no valid point found, return to home location
    }

    private void OnDrawGizmosSelected()
    {
        //Debug.Log(gameObject.name + " spawnLocation: " + homeLocation);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(homeLocation, wanderRadius); // homeLocation will look weird before running the editor.

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(lastSelectedWanderPoint, 0.1f);
    }

    // misc shits

}
