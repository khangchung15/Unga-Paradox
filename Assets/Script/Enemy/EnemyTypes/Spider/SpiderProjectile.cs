using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SpiderProjectile : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeSeconds = 3f;

    [Header("Damage / Target")]
    [SerializeField] private float damage = 8f;
    [SerializeField] private string targetTag = "Player";

    [Header("Web Spawn on Impact")]
    [SerializeField] private GameObject webPrefab;
    [SerializeField] private LayerMask groundMask;     // set to Ground/Wall/Obstacle layers
    [SerializeField] private float webLifetime = 4f;   // if your web script exposes lifetime

    [Header("Auto-Drop Web After Travel")]
    [SerializeField] private float distanceBeforeAutoWeb = 6f; // drop after traveling this far
    [Tooltip("If false, only one auto-drop happens. If true, drop every N units (see interval).")]
    [SerializeField] private bool repeatAutoDrop = false;
    [SerializeField] private float repeatInterval = 6f; // used only if repeatAutoDrop = true
    [SerializeField] private float groundProbeDistance = 2.0f; // raycast down to snap to ground

    private Rigidbody2D rb;
    private Vector2 lastPos;
    private Vector2 moveDir;
    private float distanceTraveled = 0f;
    private bool autoWebSpawned = false; // for single-drop mode

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = GetComponent<Collider2D>();
        col.isTrigger = true; // using trigger collisions

        lastPos = rb.position;
    }

    public void Launch(Vector2 direction)
    {
        moveDir = direction.normalized;
        rb.velocity = moveDir * speed;
        Destroy(gameObject, lifeSeconds);
    }

    void Update()
    {
        // ---- track distance traveled ----
        var p = rb.position;
        distanceTraveled += Vector2.Distance(p, lastPos);
        lastPos = p;

        // ---- auto-drop logic ----
        if (webPrefab != null)
        {
            if (!repeatAutoDrop)
            {
                if (!autoWebSpawned && distanceTraveled >= distanceBeforeAutoWeb)
                {
                    SpawnWebAtGround(p);
                    autoWebSpawned = true;
                }
            }
            else
            {
                // drop every repeatInterval units after the first threshold
                while (distanceTraveled >= distanceBeforeAutoWeb + repeatInterval * (CountDropsSoFar()))
                {
                    SpawnWebAtGround(p);
                    IncrementDrops();
                }
            }
        }
    }

    // Track how many auto-drops we did when repeat is enabled
    private int drops = 0;
    private int CountDropsSoFar() => drops + (autoWebSpawned ? 1 : 0);
    private void IncrementDrops()
    {
        if (!autoWebSpawned) autoWebSpawned = true;
        else drops++;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Hit player → damage + web + destroy
        if (other.CompareTag(targetTag))
        {
            var hp = other.GetComponent<Health>();
            if (hp) hp.TakeDamage(damage);
            SpawnWebAtGround(other.ClosestPoint(transform.position));
            Destroy(gameObject);
            return;
        }

        // Hit ground/obstacle → web + destroy
        if (IsGround(other.gameObject.layer))
        {
            SpawnWebAtGround(other.ClosestPoint(transform.position));
            Destroy(gameObject);
        }
    }

    private bool IsGround(int layer) => (groundMask.value & (1 << layer)) != 0;

    private void SpawnWebAtGround(Vector2 fromPos)
    {
        if (webPrefab == null) return;

        // Raycast down to snap to ground surface
        Vector2 origin = fromPos;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundProbeDistance, groundMask);
        Vector2 spawnPos = hit ? hit.point : origin;

        var go = Instantiate(webPrefab, spawnPos, Quaternion.identity);

        // If your GroundWebTrap has a public lifetime field, set it here:
        // var trap = go.GetComponent<GroundWebTrap>();
        // if (trap) trap.lifetime = webLifetime;
    }
}
