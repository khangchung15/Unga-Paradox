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
    [SerializeField] private LayerMask groundMask;    
    [SerializeField] private float webLifetime = 4f;   

    [Header("Auto-Drop Web After Travel")]
    [SerializeField] private float distanceBeforeAutoWeb = 6f;
    [Tooltip("If false, only one auto-drop happens.")]
    [SerializeField] private bool repeatAutoDrop = false;
    [SerializeField] private float repeatInterval = 6f; 
    [SerializeField] private float groundProbeDistance = 2.0f; 

    private Rigidbody2D rb;
    private Vector2 lastPos;
    private Vector2 moveDir;
    private float distanceTraveled = 0f;
    private bool autoWebSpawned = false; 

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
        rb.linearVelocity = moveDir * speed;
        Destroy(gameObject, lifeSeconds);
    }

    void Update()
    {
        var p = rb.position;
        distanceTraveled += Vector2.Distance(p, lastPos);
        lastPos = p;

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
                while (distanceTraveled >= distanceBeforeAutoWeb + repeatInterval * (CountDropsSoFar()))
                {
                    SpawnWebAtGround(p);
                    IncrementDrops();
                }
            }
        }
    }

    private int drops = 0;
    private int CountDropsSoFar() => drops + (autoWebSpawned ? 1 : 0);
    private void IncrementDrops()
    {
        if (!autoWebSpawned) autoWebSpawned = true;
        else drops++;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            var hp = other.GetComponent<Health>();
            if (hp) hp.TakeDamage(damage);
            SpawnWebAtGround(other.ClosestPoint(transform.position));
            Destroy(gameObject);
            return;
        }

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

        Vector2 origin = fromPos;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundProbeDistance, groundMask);
        Vector2 spawnPos = hit ? hit.point : origin;

        var go = Instantiate(webPrefab, spawnPos, Quaternion.identity);

    }
}
