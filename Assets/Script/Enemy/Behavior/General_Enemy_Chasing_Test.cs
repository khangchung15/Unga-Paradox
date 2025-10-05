using Pathfinding;
using UnityEngine;

// testing script for enemy chasing behavior using A* Pathfinding
public class General_Enemy_Chasing_Test : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Chasing Settings")]
    [Tooltip("Movement speed of the NPC.")]
    public float moveSpeed = 3.5f;

    private Rigidbody2D rb;
    private Animator anim;
    private General_Enemy_Movement enemyMovement;
    private Seeker seeker;
    private Path currentPath;
    private GameObject player;

    readonly float wayPointThreshold = 0.1f; // distance to waypoint to consider it reached
    public Vector2 target;
    private int currentWaypointIndex = 0; // to track current waypoint in path to destination for pathfinding
    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>(); // from parent gameObject
        anim = GetComponentInParent<Animator>(); // from parent gameObject
        enemyMovement = gameObject.AddComponent<General_Enemy_Movement>();
        enemyMovement.Initialized(rb, transform.parent);
        seeker = GetComponentInParent<Seeker>();
        player = GameObject.FindGameObjectWithTag("Player");
        InvokeRepeating(nameof(UpdatePath), 0f, 0.5f); // update path every 0.5 seconds
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (currentPath == null || currentPath.vectorPath.Count == 0)
        {
            enemyMovement.Stop();
            anim.Play("Idle");
            return;
        }

        if (currentWaypointIndex >= currentPath.vectorPath.Count)
        {
            enemyMovement.Stop();
            anim.Play("Idle");
            return;
        }

        Vector2 nextWayPoint = currentPath.vectorPath[currentWaypointIndex];
        enemyMovement.MoveTowards(nextWayPoint, moveSpeed);
        anim.Play("Run");

        if (Vector2.Distance(transform.parent.position, nextWayPoint) < wayPointThreshold)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= currentPath.vectorPath.Count)
            {
                enemyMovement.Stop();
                //anim.Play("Idle");
            }
        }
    }

    private void UpdatePath()
    {
        if (seeker.IsDone() && player != null)
        {
            seeker.StartPath(transform.parent.position, player.transform.position, OnPathComplete);
        }
    }
    private void OnPathComplete(Path p)
    {
        if(!p.error && p.vectorPath != null)
        {
            currentPath = p;
            currentWaypointIndex = 0;
        }
    }
}
