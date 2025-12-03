using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePortal : MonoBehaviour
{
    [Header("Scene to load")]
    public string sceneName;

    [Header("Name of spawn point inside the new scene (optional)")]
    public string spawnPointName;

    [Header("Cooldown to prevent instant retrigger")]
    public float cooldown = 1f;

    private static float lastUseTime = -999f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (Time.time - lastUseTime < cooldown)
            return;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("ScenePortal: No scene name assigned!");
            return;
        }

        lastUseTime = Time.time;

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (string.IsNullOrEmpty(spawnPointName))
            return;

        // Find spawn point inside the new scene
        GameObject spawnPoint = GameObject.Find(spawnPointName);

        if (spawnPoint == null)
        {
            Debug.LogWarning("ScenePortal: Spawn point '" + spawnPointName + "' not found in scene.");
            return;
        }

        // Move player to spawn point
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = spawnPoint.transform.position;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }
    }
}