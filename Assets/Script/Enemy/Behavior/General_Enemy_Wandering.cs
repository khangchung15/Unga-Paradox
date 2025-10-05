using UnityEngine;
using System.Collections;
using Pathfinding;

// this script mainly handles the wandering waypoint logic for enemies
public class General_Enemy_Wandering : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Wandering Settings")]
    [Tooltip("Movement speed of the NPC.")]
    public float moveSpeed = 2f;

    [Tooltip("Radius for wandering area.")]
    public float wanderRadius = 5f;

    [Tooltip("Minimum Rest Time between Patrol Points")]
    public float minRestTime = 0.5f;

    [Tooltip("Maximum Rest Time between Patrol Points")]
    public float maxRestTime = 2f;

    private Rigidbody2D rb;
    private Animator anim;
    private General_Enemy_Movement enemyMovement;
    private Seeker seeker;
    private Path currentPath;

    readonly float wayPointThreshold = 0.1f; // distance to waypoint to consider it reached
    private Vector2 homeLocation;
    private bool isResting;
    public Vector2 target;
    private int currentWaypointIndex = 0; // to track current waypoint in path to destination for pathfinding

    // for debugging purposes
    private Vector2 lastSelectedWanderPoint;

    private void Awake()
    {
        homeLocation = transform.parent != null ? (Vector2)transform.parent.position : (Vector2)transform.position;
        rb = GetComponentInParent<Rigidbody2D>(); // from parent gameObject
        anim = GetComponentInParent<Animator>(); // from parent gameObject
        enemyMovement = gameObject.AddComponent<General_Enemy_Movement>();
        enemyMovement.Initialized(rb, transform.parent);
        seeker = GetComponentInParent<Seeker>();

    }
    private void OnEnable()
    {
        StartCoroutine(IdleAndSetNewWanderPoint());
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isResting || currentPath == null || currentPath.vectorPath.Count == 0)
        {
            enemyMovement.Stop();
            return;
        }

        Vector2 nextWayPoint = currentPath.vectorPath[currentWaypointIndex];
        enemyMovement.MoveTowards(nextWayPoint, moveSpeed);
        anim.Play("Walk");

        if (Vector2.Distance(transform.parent.position, nextWayPoint) < wayPointThreshold)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= currentPath.vectorPath.Count)
            {
                enemyMovement.Stop();
                StartCoroutine(IdleAndSetNewWanderPoint());
            }
        }   
    }
    public IEnumerator IdleAndSetNewWanderPoint()
    {
        isResting = true;
        anim.Play("Idle");
        rb.linearVelocity = Vector2.zero;
        float restTime = UnityEngine.Random.Range(minRestTime, maxRestTime);
        yield return new WaitForSeconds(restTime);

        target = GetRandomWanderPoint();
        //Debug.Log(HelperFuncs.GetOwnerName(transform) + " New Wander Point Set: " + target);
        seeker.StartPath(rb.position, target, OnPathComplete);
        isResting = false;
        anim.Play("Walk");
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error && p.vectorPath != null)
        {
            currentPath = p;
            currentWaypointIndex = 0;
            //Debug.Log("Path to target:");
            //foreach (var point in p.vectorPath)
            //{
            //    Debug.Log($"Waypoint: {point.x}, {point.y}, {point.z}");
            //}
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
