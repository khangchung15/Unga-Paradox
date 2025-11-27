using System.Collections;
using UnityEngine;

public class SpiderBoss : Spider
{
    [Header("Boss Health")]
    [SerializeField] private BossHealthLatest bossHealth;

    [Header("Phase 2 Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float phase2ThresholdPercent = 0.5f;  // 50% HP

    [Header("Minion Spawning")]
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private Transform[] minionSpawnPoints;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private int maxMinionsAlive = 4;

    [Header("Minion Spawn FX")]
    [SerializeField] private AudioSource spawnSfxSource;
    [SerializeField] private AudioClip spawnSfxClip;
    [SerializeField] private string spawnAnimTrigger = "Spawn";

    private bool inPhase2 = false;
    private Coroutine spawnRoutine;

    protected override void Awake()
    {
        base.Awake();

        // Get the boss health component (inherits EnemyHealth)
        if (bossHealth == null)
            bossHealth = GetComponent<BossHealthLatest>();

        if (bossHealth == null)
        {
            Debug.LogError("SpiderBoss: BossHealthLatest component is required on this GameObject.");
        }

        // Hook death so we can stop spawning, play extra logic, etc.
        if (enemyHealth != null && enemyHealth.onDeath != null)
        {
            enemyHealth.onDeath.AddListener(OnBossDeath);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!inPhase2 && bossHealth != null)
        {
            // Check if HP dropped under threshold
            float hpPercent = (float)bossHealth.CurrentHealth / bossHealth.StartingHealth;
            if (hpPercent <= phase2ThresholdPercent)
            {
                EnterPhase2();
            }
        }
    }

    private void EnterPhase2()
    {
        inPhase2 = true;
        Debug.Log("SpiderBoss: Entering phase 2!");

        // You can also tweak movement/attack here:
        // e.g. increase speed, change attack pattern, etc.

        if (minionPrefab != null && minionSpawnPoints != null && minionSpawnPoints.Length > 0)
        {
            spawnRoutine = StartCoroutine(SpawnMinionsLoop());
        }
        else
        {
            Debug.LogWarning("SpiderBoss: Phase 2 minion settings are incomplete (no prefab or spawn points).");
        }
    }

    private IEnumerator SpawnMinionsLoop()
    {
        var wait = new WaitForSeconds(spawnInterval);

        while (true)
        {
            // Stop if boss is dead (safety)
            if (bossHealth == null || bossHealth.CurrentHealth <= 0)
                yield break;

            // Count current minions in scene (simple tag-based approach)
            int alive = GameObject.FindGameObjectsWithTag("Enemy").Length;
            if (alive < maxMinionsAlive)
            {
                // Play spawn sound once per spawn cycle
                if (spawnSfxSource != null && spawnSfxClip != null)
                {
                    spawnSfxSource.PlayOneShot(spawnSfxClip);
                }

                // Ensure we have at least 2 spawn points
                if (minionSpawnPoints.Length >= 2)
                {
                    Transform p0 = minionSpawnPoints[0];
                    Transform p1 = minionSpawnPoints[1];

                    if (p0 != null)
                    {
                        Instantiate(minionPrefab, p0.position, p0.rotation);
                        var anim0 = p0.GetComponent<Animator>();
                        if (anim0 != null && !string.IsNullOrEmpty(spawnAnimTrigger))
                        {
                            anim0.SetTrigger(spawnAnimTrigger);
                        }
                    }

                    if (p1 != null)
                    {
                        Instantiate(minionPrefab, p1.position, p1.rotation);
                        var anim1 = p1.GetComponent<Animator>();
                        if (anim1 != null && !string.IsNullOrEmpty(spawnAnimTrigger))
                        {
                            anim1.SetTrigger(spawnAnimTrigger);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("SpiderBoss: Requires exactly 2 spawn points for minion spawning.");
                }
            }

            yield return wait;
        }
    }

    private void OnBossDeath()
    {
        Debug.Log("SpiderBoss: Boss died, stopping phase 2 behaviour.");

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    protected override void OnDestroy()
    {
        if (enemyHealth != null && enemyHealth.onDeath != null)
        {
            enemyHealth.onDeath.RemoveListener(OnBossDeath);
        }

        base.OnDestroy();
    }
}