using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Where the player should appear after teleporting")]
    public Transform teleportPoint;

    [Header("Cooldown (seconds) between teleports through THIS portal")]
    public float cooldownDuration = 1f;

    private float lastTeleportTime = -999f; // so it works immediately at start

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only teleport the player
        if (!other.CompareTag("Player"))
            return;

        // Check cooldown
        if (Time.time - lastTeleportTime < cooldownDuration)
            return;

        if (teleportPoint == null)
        {
            Debug.LogError("PortalTeleporter: No teleportPoint assigned on " + gameObject.name);
            return;
        }

        // Teleport the player
        other.transform.position = teleportPoint.position;

        // Optional: reset velocity if the player uses Rigidbody2D
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Record teleport time for cooldown
        lastTeleportTime = Time.time;
    }
}