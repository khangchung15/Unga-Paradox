using UnityEngine;

public class ClusterNadeProjectile : MonoBehaviour
{
    [Header("Fuse Settings")]
    [Tooltip("Time before the grenade splits into clusters")]
    public float fuseTime = 2.0f;
    private float timer = 0.0f;

    [Header("Cluster Settings")]
    [Tooltip("Prefab of the smaller grenades spawned on explosion")]
    public GameObject clusterPartPrefab;
    [Tooltip("Number of smaller grenades to spawn")]
    public int clusterCount = 5;
    [Tooltip("Speed of the smaller grenades after explosion")]
    public float clusterVelocity = 5.0f;

    [Header("Explosion Settings")]
    [Tooltip("Visual explosion effect prefab (optional)")]
    public GameObject explosionEffect;
    [Tooltip("Explosion sound (optional)")]
    public AudioClip explodeSound;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > fuseTime)
        {
            Explode();
        }
    }

    private void Explode()
    {
        // Play explosion sound
        if (explodeSound)
            audioSource.PlayOneShot(explodeSound);

        // Spawn explosion visual
        if (explosionEffect)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        // Spawn smaller grenades in a circle
        for (int i = 0; i < clusterCount; i++)
        {
            float angle = i * (360f / clusterCount);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            GameObject cluster = Instantiate(clusterPartPrefab, transform.position, Quaternion.identity);
            Rigidbody2D rb = cluster.GetComponent<Rigidbody2D>();
            if (rb)
                rb.linearVelocity = dir * clusterVelocity;
        }

        // Destroy main grenade
        Destroy(gameObject);
    }
}
