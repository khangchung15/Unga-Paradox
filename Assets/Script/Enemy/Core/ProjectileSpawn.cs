using Unity.AppUI.Core;
using UnityEngine;

public class ProjectileSpawn : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject projectile;
    public Transform projectilePos;

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
        if(projectilePos == null)
        {
            projectilePos = transform;
        }
            
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    public void Spawn()
    {
       if (projectile == null)
        {
            Debug.LogWarning($"[{name}] ProjectileSpawn has no projectile assigned.");
        }

        var player = GameObject.FindGameObjectWithTag("Player");
        Vector2 targetPost = (Vector2)player.transform.position;

        Vector2 spawnPos = projectilePos.position;
        Vector2 direction = (targetPost - spawnPos).normalized;

        GameObject go = Instantiate(projectile, spawnPos, Quaternion.identity);
        var proj = go.GetComponent<EnemyProjectile>();
        if (proj != null)
        {
            proj.Initialize(direction, projectileSpeed, maxDistance, damage, lifetime, ownerTag);
        }
        else
        {
            // If prefab doesn't have EnemyProjectile, attempt to give it initial velocity via Rigidbody2D
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = direction * projectileSpeed;
            if (lifetime > 0f)
                Destroy(go, lifetime);
        }

    }
}
