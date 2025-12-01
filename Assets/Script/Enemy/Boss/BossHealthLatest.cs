using UnityEngine;

public class BossHealthLatest : EnemyHealth
{
    [Header("Boss UI")]
    // "Latest" at the end indicates the new bosshealthbar scripts. The old ones are only ussd on the gun monkey boss
    [SerializeField] private BossHealthBarLatest bossHealthBar;

    [Header("Boss Portal / Extra Death Logic")]
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private Transform portalSpawnPoint;
    [SerializeField] private string destinationSceneName = "Hub";
    [SerializeField] private string destinationSpawnPointName = "SpawnPoint";

    public int CurrentHealth => currentHealth;

    protected void Start()
    {
        base.Start();

        // Auto-find boss health bar if not assigned
        if (bossHealthBar == null)
        {
            bossHealthBar = FindObjectOfType<BossHealthBarLatest>();
            if (bossHealthBar == null)
            {
                Debug.LogWarning("SpiderBossHealth: No BossHealthBar found in the scene.");
                return;
            }
        }

        if (bossHealthBar.mSlider != null)
        {
            bossHealthBar.mSlider.maxValue = StartingHealth;
            bossHealthBar.SetValue(StartingHealth);
        }
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        if (bossHealthBar != null && bossHealthBar.mSlider != null)
        {
            bossHealthBar.SetValue(currentHealth);
        }
    }

    public override void DetectDeath()
    {
        base.DetectDeath();

        if (bossHealthBar != null)
        {
            bossHealthBar.SetValue(0);
            bossHealthBar.gameObject.SetActive(false);
        }

        if (portalPrefab != null)
        {
            Vector3 spawnPosition = portalSpawnPoint != null ? portalSpawnPoint.position : transform.position;
            Quaternion spawnRotation = portalSpawnPoint != null ? portalSpawnPoint.rotation : Quaternion.identity;
            var portalGo = Instantiate(portalPrefab, spawnPosition, spawnRotation);

            var scenePortal = portalGo.GetComponent<ScenePortal>();
            if (scenePortal != null)
            {
                scenePortal.sceneName = destinationSceneName;
                scenePortal.spawnPointName = destinationSpawnPointName;
            }
        }
    }
}