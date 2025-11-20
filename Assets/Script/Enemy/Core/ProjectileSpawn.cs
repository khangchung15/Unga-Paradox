using UnityEngine;

public class ProjectileSpawn : MonoBehaviour
{
    [Header("Projectile (prefab must have an EnemyProjectile or Rigidbody2D)")]
    public GameObject projectile;
    public Transform bulletPos;

    [Header("Projectile defaults (used when Spawn is called)")]
    [Tooltip("Speed in world units/sec")]
    public float projectileSpeed = 8f;
    [Tooltip("Maximum travel distance before projectile destroys itself")]
    public float maxDistance = 10f;
    [Tooltip("Damage applied when projectile hits a Health component")]
    public int damage = 10;
    [Tooltip("Optional lifetime fallback (seconds). Set 0 to ignore.")]
    public float lifetime = 4f;
    [Tooltip("Tag applied as owner so projectile can ignore colliding with spawner")]
    public string ownerTag = "Enemy";

    private void Awake()
    {
        if (bulletPos == null)
            bulletPos = transform;
    }

    // Call this from an animation event (at the fire frame) or from code.
    public void Spawn()
    {
        if (projectile == null)
        {
            Debug.LogWarning($"[{name}] ProjectileSpawn.Spawn called but no projectile prefab assigned.");
            return;
        }

        var player = GameObject.FindGameObjectWithTag("Player");
        Vector2 targetPos = player != null ? (Vector2)player.transform.position : (Vector2)bulletPos.position;
        Vector2 spawnPos = bulletPos.position;
        Vector2 direction = (targetPos - spawnPos);
        if (direction.sqrMagnitude <= Mathf.Epsilon)
            direction = bulletPos.right;
        direction.Normalize();

        GameObject go = Instantiate(projectile, spawnPos, Quaternion.identity);
        if (go == null)
        {
            Debug.LogWarning($"[{name}] Spawn: Instantiate returned null for projectile prefab.");
            return;
        }

        // Make the projectile ignore collisions with enemies at spawn time
        var projCollider = go.GetComponent<Collider2D>();
        if (projCollider != null)
        {
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var e in enemies)
            {
                // ignore all colliders on the enemy (handles colliders on children)
                var enemyCols = e.GetComponentsInChildren<Collider2D>();
                foreach (var ec in enemyCols)
                {
                    if (ec != null)
                        Physics2D.IgnoreCollision(projCollider, ec, true);
                }
            }
        }

        // Configure projectile via its API if present
        var projComp = go.GetComponent<EnemyProjectile>();
        if (projComp != null)
        {
            projComp.Initialize(direction, projectileSpeed, maxDistance, damage, lifetime, ownerTag);
            return;
        }

        // Fallback: apply velocity to Rigidbody2D if present
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * projectileSpeed;
            if (lifetime > 0f)
                Destroy(go, lifetime);
            return;
        }

        // Last resort: the prefab must handle its own movement. Destroy after lifetime if set.
        Debug.LogWarning($"[{name}] Spawned projectile has no EnemyProjectile or Rigidbody2D. It will not move unless it self-handles movement.");
        if (lifetime > 0f)
            Destroy(go, lifetime);
    }
}