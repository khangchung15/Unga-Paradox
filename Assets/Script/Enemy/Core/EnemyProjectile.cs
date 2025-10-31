using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Enemy Projectile Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float speed = 8.0f;
    [SerializeField] private float maxDistance = 10.0f;
    [SerializeField] private float lifetime = 4.0f;
    [SerializeField] private string ownerTag = "Enemy";
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private bool useRigidbody = true;

    private Rigidbody2D rb;
    private Vector2 velocity;
    private Vector2 startPos;
    private float createdTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (useRigidbody && rb == null)
        {
            useRigidbody = false;
            Debug.LogWarning($"[{name}] EnemyProjectile: useRigidbody is true but no Rigidbody2D found. Switching to manual movement.");
        }
    }

    private void Start()
    {
        startPos = transform.position;
        createdTime = Time.time;

        if (lifetime > 0f)
            Destroy(gameObject, lifetime);

        if (useRigidbody && rb != null && velocity != Vector2.zero)
            rb.linearVelocity = velocity;
    }

    private void Update()
    {
        if (!useRigidbody)
            transform.position += (Vector3)(velocity * Time.deltaTime);

        if (maxDistance > 0f)
        {
            float traveled = Vector2.Distance(startPos, transform.position);
            if (traveled >= maxDistance)
                Destroy(gameObject);
        }
    }

    // Public API used by spawner
    public void Initialize(Vector2 direction, float speedValue, float maxDistanceValue, int damageValue, float lifetimeValue = 0f, string ownerTagValue = "")
    {
        damage = damageValue;
        speed = speedValue;
        maxDistance = maxDistanceValue;
        if (lifetimeValue > 0f) lifetime = lifetimeValue;
        if (!string.IsNullOrEmpty(ownerTagValue)) ownerTag = ownerTagValue;

        velocity = direction.normalized * speed;
        if (useRigidbody && rb != null)
            rb.linearVelocity = velocity;
    }

    private GameObject GetHitRoot(Collider2D col)
    {
        if (col == null) return null;
        return col.attachedRigidbody != null ? col.attachedRigidbody.gameObject : col.gameObject;
    }

    private void HandleHit(GameObject hitRoot, Collider2D rawCollider)
    {
        if (hitRoot == null) return;

        // ignore owner
        if (!string.IsNullOrEmpty(ownerTag) && hitRoot.CompareTag(ownerTag))
            return;

        // ignore enemies (covers child colliders and parent-tagged cases)
        var enemyComp = hitRoot.GetComponent<Enemy>();
        if (hitRoot.CompareTag("Enemy") || enemyComp != null)
            return;

        // ignore other projectiles
        if (hitRoot.CompareTag("Projectile"))
            return;

        // damage target (use Health on parent if collider is child)
        if (hitRoot.CompareTag(targetTag))
        {
            var h = hitRoot.GetComponentInParent<Health>();
            if (h != null)
            {
                h.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }

        // environment hit
        if (hitRoot.CompareTag("Wall") || hitRoot.CompareTag("Obstacle") || hitRoot.CompareTag("Ground"))
        {
            Destroy(gameObject);
            return;
        }

        // fallback: destroy on anything else
        Destroy(gameObject);
    }

    // Trigger path (recommended prefab collider = isTrigger)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        var hitRoot = GetHitRoot(other);
        HandleHit(hitRoot, other);
    }

    // Fallback for non-trigger colliders
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null || collision.collider == null) return;

        var hitRoot = GetHitRoot(collision.collider);
        HandleHit(hitRoot, collision.collider);
    }
}