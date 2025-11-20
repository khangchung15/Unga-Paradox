using System.Collections;
using UnityEngine;

public class SpiderSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject spiderPrefab;      // Spider enemy prefab
    public Transform spawnPoint;         // Spawn location (default: nest position)
    public float spawnInterval = 3f;
    public int maxAliveSpiders = 5;
    public int totalToSpawn = -1;      

    [Header("Activation")]
    public bool spawnOnStart = true;
    public bool activateByPlayerProximity = false;
    public Transform player;
    public float activationRadius = 6f;

    [Header("Nest Health")]
    public int maxHealth = 50;
    public bool destroyOnDeath = true;
    public GameObject deathEffect;   

    private int currentHealth;
    private int currentAlive = 0;
    private int totalSpawned = 0;
    private bool isActive = false;
    private Coroutine spawnRoutine;
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;

        if (spawnOnStart)
        {
            ActivateNest();
        }
    }

    private void Update()
    {
        if (!activateByPlayerProximity || isActive || player == null || isDead) 
            return;

        float dist = Vector2.Distance(player.position, transform.position);
        if (dist <= activationRadius)
        {
            ActivateNest();
        }
    }
    
    public void ActivateNest()
    {
        if (isActive || isDead) return;

        isActive = true;
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void DeactivateNest()
    {
        isActive = false;
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (isActive && !isDead)
        {
            bool canSpawnMoreTotal = (totalToSpawn < 0) || (totalSpawned < totalToSpawn);
            bool canSpawnMoreAlive = currentAlive < maxAliveSpiders;

            if (canSpawnMoreTotal && canSpawnMoreAlive)
            {
                SpawnSpider();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnSpider()
    {
        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;

        GameObject spider = Instantiate(spiderPrefab, pos, Quaternion.identity);
        totalSpawned++;
        currentAlive++;
    }

    public void NotifySpiderDied()
    {
        currentAlive = Mathf.Max(0, currentAlive - 1);
    }

    // Health / Damage 

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log("HELLLOOOO");
        currentHealth = Mathf.Max(0, currentHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        DeactivateNest();

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        int damage = 10;

        TakeDamage(damage);
    }
}
