using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyDistancing : MonoBehaviour
{
    public Transform player;

    [Header("Speeds & Forces")]
    public float maxSpeed = 4f;
    public float acceleration = 20f;

    [Header("Engagement Ring")]
    public float desiredRange = 2.5f;       // preferred distance from player
    public float rangeSlack = 0.75f;        // +/- slack around desiredRange

    [Header("Orbiting")]
    [Range(-1f, 1f)] public float orbitDirection = 1f; // +1 CW, -1 CCW (randomize per enemy)
    public float orbitStrength = 2.0f;      // tangential desire

    [Header("Separation")]
    public float neighborRadius = 1.2f;     // how far to "feel" other enemies
    public float separationWeight = 3.0f;   // how strongly to push apart
    public LayerMask enemyMask;             // set to your Enemy layer

    Rigidbody2D rb;
    Collider2D col;
    readonly Collider2D[] hits = new Collider2D[16];

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Optional: randomize orbit direction for variety
        if (Mathf.Approximately(orbitDirection, 0f))
            orbitDirection = Random.value < 0.5f ? -1f : 1f;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector2 pos = rb.position;
        Vector2 toPlayer = (Vector2)player.position - pos;
        float dist = toPlayer.magnitude;
        Vector2 dirToPlayer = dist > 0.0001f ? toPlayer / dist : Vector2.zero;

        // 1) Keep a ring around the player (move in if too far, out if too close)
        float inner = desiredRange - rangeSlack;
        float outer = desiredRange + rangeSlack;
        Vector2 ringForce = Vector2.zero;
        if (dist > outer)
        {
            ringForce = dirToPlayer;           // go inwards
        }
        else if (dist < inner)
        {
            ringForce = -dirToPlayer;          // step back out
        }
        // else: within the ring — no radial push

        // 2) Orbit/strafe tangentially around the player
        Vector2 tangent = new Vector2(-dirToPlayer.y, dirToPlayer.x) * orbitDirection;
        Vector2 orbitForce = tangent * orbitStrength;

        // 3) Separation from neighbors (repulsion)
        Vector2 separation = Vector2.zero;
        int count = Physics2D.OverlapCircleNonAlloc(pos, neighborRadius, hits, enemyMask);
        for (int i = 0; i < count; i++)
        {
            var h = hits[i];
            if (h == null || h.attachedRigidbody == rb) continue;
            // ignore self and non-enemy layers
            Vector2 away = (Vector2)(pos - (Vector2)h.transform.position);
            float d = away.magnitude;
            if (d > 0.0001f)
            {
                // inverse-distance weighting; stronger when closer
                separation += (away / (d * d));
            }
        }
        separation *= separationWeight;

        // Combine desired velocity
        Vector2 desiredVel = (ringForce + orbitForce + separation);
        if (desiredVel.sqrMagnitude > 1f) desiredVel = desiredVel.normalized;
        desiredVel *= maxSpeed;

        // Steering = desired - current
        Vector2 steering = desiredVel - rb.velocity;
        Vector2 force = Vector2.ClampMagnitude(steering * acceleration, acceleration);
        rb.AddForce(force, ForceMode2D.Force);
    }

    void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, desiredRange - rangeSlack);
        Gizmos.DrawWireSphere(player.position, desiredRange + rangeSlack);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, neighborRadius);
    }
}
