using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private string targetTag = "Player";
    [Tooltip("If true, hitbox Collider2D will be disabled on Start")]
    [SerializeField] private bool disableColliderOnStart = true;

    [Header("SFX (optional)")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip hitSfx;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    [Header("Knockback")]
    [Tooltip("Enable knockback on hit")]
    [SerializeField] private bool enableKnockback = true;
    [Tooltip("Knockback force applied to the target (used for Rigidbody fallback).")]
    [SerializeField] private float knockbackForce = 8f;
    [Tooltip("Duration of knockback effect on the target (used for PlayerController).")]
    [SerializeField] private float knockbackDuration = 0.2f;

    private Collider2D col;
    private HashSet<int> hitTargets = new HashSet<int>();
    private Coroutine timeoutCoroutine;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col == null)
            throw new System.Exception("AttackHitbox requires a Collider2D (set IsTrigger = true).");
        col.isTrigger = true;
        if (disableColliderOnStart)
            col.enabled = false;
    }

    // Enable hitbox and clear per-activation hit history
    public void EnableHitbox()
    {
        hitTargets.Clear();
        if (timeoutCoroutine != null) { StopCoroutine(timeoutCoroutine); timeoutCoroutine = null; }
        col.enabled = true;
    }

    // Disable hitbox immediately
    public void DisableHitbox()
    {
        if (timeoutCoroutine != null) { StopCoroutine(timeoutCoroutine); timeoutCoroutine = null; }
        col.enabled = false;
    }

    // Enable for a fixed duration (handy if you want frame-window by time)
    public void EnableHitboxForSeconds(float seconds)
    {
        EnableHitbox();
        if (timeoutCoroutine != null) StopCoroutine(timeoutCoroutine);
        timeoutCoroutine = StartCoroutine(DisableAfterSeconds(seconds));
    }

    private IEnumerator DisableAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        DisableHitbox();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!col.enabled)
            return;
        if (!other.CompareTag(targetTag))
            return;

        int id = other.gameObject.GetInstanceID();
        if (hitTargets.Contains(id))
            return; // already hit this activation

        hitTargets.Add(id);

        var health = other.GetComponent<Health>();
        if (health == null)
        {
            Debug.LogWarning("AttackHitbox: target has no Health component.", other.gameObject);
            return;
        }
        else
        {
            health.TakeDamage(damage);
            Debug.Log($"AttackHitbox: hit {other.gameObject.name} for {damage} damage.");
        }

        // Play SFX if assigned
        if (hitSfx != null)
        {
            if (sfxSource != null)
            {
                sfxSource.pitch = UnityEngine.Random.Range(0.9f, 1.15f);
                sfxSource.PlayOneShot(hitSfx, sfxVolume);
            }
            else
            {
                GameObject tempGO = new GameObject("TempAudio");
                tempGO.transform.position = transform.position;
                AudioSource tempSource = tempGO.AddComponent<AudioSource>();
                tempSource.pitch = UnityEngine.Random.Range(0.9f, 1.15f); // same range
                tempSource.volume = sfxVolume;
                tempSource.PlayOneShot(hitSfx);
                Destroy(tempGO, hitSfx.length / tempSource.pitch); // cleanup
            }
        }

        // Apply knockback if enabled
        if (enableKnockback)
        {
            // Compute direction away from the hitbox center to the target
            Vector2 direction = ((Vector2)other.transform.position - (Vector2)transform.position).normalized;
            if (direction == Vector2.zero) direction = Vector2.up;
            Vector2 knockVelocity = direction * knockbackForce;

            // Preferred: if target has a PlayerController (or other script) that implements ApplyKnockback(Vector2, float), call it.
            var playerCtrl = other.GetComponentInParent<PlayerController>();
            if (playerCtrl != null)
            {
                // Use reflection to call ApplyKnockback if it exists (keeps this script safe if method missing)
                MethodInfo mi = playerCtrl.GetType().GetMethod("ApplyKnockback", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (mi != null)
                {
                    try
                    {
                        mi.Invoke(playerCtrl, new object[] { knockVelocity, knockbackDuration });
                        return;
                    }
                    catch
                    {
                        // Fall through to rigidbody fallback if invoke fails
                    }
                }
            }

            // Alternative: target may have a Knockback component with a known method (optional)
            var knockComp = other.GetComponentInParent<Knockback>();
            if (knockComp != null)
            {
                // If your Knockback has a method like GetKnockedBack(Transform source, float force)
                MethodInfo kmi = knockComp.GetType().GetMethod("GetKnockedBack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (kmi != null)
                {
                    try
                    {
                        kmi.Invoke(knockComp, new object[] { transform, knockbackForce });
                        return;
                    }
                    catch
                    {
                        // ignore and fallback
                    }
                }
            }

            // Fallback: apply Rigidbody2D impulse on the root rigidbody
            var rb = other.attachedRigidbody ?? other.GetComponentInParent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(knockVelocity, ForceMode2D.Impulse);
            }
        }
    }
}