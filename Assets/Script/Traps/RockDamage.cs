using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class RockDamage : MonoBehaviour
{
    [SerializeField] float damage = 20f;
    [SerializeField] string targetTag = "Player";

    [Header("Landing / Debris")]
    [SerializeField] LayerMask worldLayers;      // set to Ground/Walls/etc.
    [SerializeField] float debrisLifetime = 12f; // 0 = never auto-despawn
    [SerializeField] bool playShatterOnPlayerHit = true;

    Rigidbody2D rb;
    bool hasDropped;
    bool resolved;  // already handled a collision with player or world

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.isKinematic = true; // until Drop() is called
    }

    public void ResetRock()
    {
        resolved = false;
        hasDropped = false;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0f;
        rb.isKinematic = true;

        var ps = GetComponentInChildren<ParticleSystem>(true);
        if (ps) { ps.gameObject.SetActive(false); ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); }
    }

    public void Drop()
    {
        if (hasDropped) return;
        hasDropped = true;
        rb.isKinematic = false;
        rb.gravityScale = 2.5f; // tweak
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (resolved) return;

        // 1) Hit the player -> damage + destroy
        if (col.collider.CompareTag(targetTag))
        {
            resolved = true;

            var hp = col.collider.GetComponent<Health>();
            if (hp) hp.TakeDamage(damage);

            if (playShatterOnPlayerHit)
                PlayShatterFX();

            Destroy(gameObject, 0.05f);
            return;
        }

        // 2) Hit world (walls/floor) -> land and stay (optional timed despawn)
        if (IsInLayerMask(col.collider.gameObject.layer, worldLayers))
        {
            resolved = true;

            // land: stop physics & stick
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
            rb.gravityScale = 0f;

            // ensure it stops dead (no bounce)
            var mat = GetComponent<Collider2D>()?.sharedMaterial;
            if (mat != null) { /* optional: use 0 bounciness material */ }

            if (debrisLifetime > 0f)
                Destroy(gameObject, debrisLifetime);

            return;
        }

        // 3) Other collisions (enemies, pickups, etc.) -> ignore by default
    }

    bool IsInLayerMask(int layer, LayerMask mask) => (mask.value & (1 << layer)) != 0;

    void PlayShatterFX()
    {
        var ps = GetComponentInChildren<ParticleSystem>(true);
        if (ps)
        {
            ps.gameObject.SetActive(true);
            ps.Play();
        }
    }
    
    [SerializeField] float maxLifetime = 8f;

    void Start()
    {
        if (maxLifetime > 0f)
            Destroy(gameObject, maxLifetime);
    }
}
