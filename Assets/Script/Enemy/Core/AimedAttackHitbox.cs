using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimedAttackHitbox : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private string targetTag = "Player";
    [Tooltip("If true, hitbox Collider2D will be disabled on Start")]
    [SerializeField] private bool disableColliderOnStart = true;

    [Header("Aim (so it hits up/down/left/right)")]
    [SerializeField] private bool aimAtTargetOnEnable = true;
    [SerializeField] private float frontOffset = 0.35f; // how far from enemy center to place hitbox
    [SerializeField] private Transform owner;          // usually the enemy root (defaults to parent)

    [Header("SFX (optional)")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip hitSfx;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;

    [Header("Knockback")]
    [Tooltip("Enable knockback on hit")]
    [SerializeField] private bool enableKnockback = true;
    [Tooltip("Knockback force applied to the target (used for Rigidbody fallback).")]
    [SerializeField] private float knockbackForce = 8f;
    [Tooltip("Duration of knockback effect on the target (used for Rigidbody fallback).")]
    [SerializeField] private float knockbackDuration = 0.2f;

    private Collider2D col;
    private HashSet<int> hitTargets = new HashSet<int>();
    private Coroutine timeoutCoroutine;
    private Transform target; // cached player

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col == null)
            throw new System.Exception("AttackHitbox requires a Collider2D (set IsTrigger = true).");
        col.isTrigger = true;
        if (disableColliderOnStart) col.enabled = false;

        if (owner == null) owner = transform.parent; // enemy root by default
        if (target == null)
        {
            var t = GameObject.FindGameObjectWithTag(targetTag);
            if (t != null) target = t.transform;
        }
    }

    // Enable hitbox and clear per-activation hit history
    public void EnableHitbox()
    {
        hitTargets.Clear();
        if (timeoutCoroutine != null) { StopCoroutine(timeoutCoroutine); timeoutCoroutine = null; }

        if (aimAtTargetOnEnable) AimAtTarget(); // <-- place hitbox toward player

        col.enabled = true;
    }

    // Enable for a fixed duration (handy if you want frame-window by time)
    public void EnableHitboxForSeconds(float seconds)
    {
        EnableHitbox();
        if (timeoutCoroutine != null) StopCoroutine(timeoutCoroutine);
        timeoutCoroutine = StartCoroutine(DisableAfterSeconds(seconds));
    }

    // Disable hitbox immediately
    public void DisableHitbox()
    {
        if (timeoutCoroutine != null) { StopCoroutine(timeoutCoroutine); timeoutCoroutine = null; }
        col.enabled = false;
    }

    IEnumerator DisableAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        DisableHitbox();
    }

    // Move the hitbox to the enemy's facing side toward the player (4-way snap)
    void AimAtTarget()
    {
        if (owner == null || target == null) return;

        Vector2 toTarget = (Vector2)(target.position - owner.position);
        if (toTarget.sqrMagnitude < 0.0001f) toTarget = Vector2.right;

        // Choose dominant axis (Left/Right/Up/Down)
        Vector2 dir = Mathf.Abs(toTarget.x) >= Mathf.Abs(toTarget.y)
            ? (toTarget.x >= 0 ? Vector2.right : Vector2.left)
            : (toTarget.y >= 0 ? Vector2.up    : Vector2.down);

        // For CircleCollider2D we can just move the offset. If not circle, use localPosition.
        if (col is CircleCollider2D cc)
            cc.offset = dir * frontOffset;
        else
            transform.localPosition = (Vector3)(dir * frontOffset);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!col.enabled) return;
        if (!other.CompareTag(targetTag)) return;

        int id = other.gameObject.GetInstanceID();
        if (hitTargets.Contains(id)) return; // already hit this activation
        hitTargets.Add(id);

        // Damage
        var health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
            // Debug.Log($"AttackHitbox: hit {other.gameObject.name} for {damage} damage.");
        }
        else
        {
            Debug.LogWarning("AttackHitbox: target has no Health component.", other.gameObject);
        }

        // SFX
        if (hitSfx != null)
        {
            if (sfxSource != null)
            {
                sfxSource.pitch = Random.Range(0.9f, 1.15f);
                sfxSource.PlayOneShot(hitSfx, sfxVolume);
            }
            else
            {
                GameObject tempGO = new GameObject("TempAudio");
                tempGO.transform.position = transform.position;
                var tempSource = tempGO.AddComponent<AudioSource>();
                tempSource.pitch = Random.Range(0.9f, 1.15f);
                tempSource.volume = sfxVolume;
                tempSource.PlayOneShot(hitSfx);
                Destroy(tempGO, hitSfx.length / tempSource.pitch);
            }
        }

        // Knockback (once per swing)
        if (enableKnockback)
        {
            var kb = other.GetComponent<Knockback>();
            if (kb != null)
            {
                kb.GetKnockedBack(transform, knockbackForce);
            }
            else
            {
                // Rigidbody fallback
                var rb = other.attachedRigidbody;
                if (rb != null)
                {
                    Vector2 dir = ((Vector2)other.transform.position - (Vector2)transform.position).normalized;
                    rb.linearVelocity = Vector2.zero;
                    rb.AddForce(dir * knockbackForce * rb.mass, ForceMode2D.Impulse);
                    StartCoroutine(StopRbAfter(rb, knockbackDuration));
                }
            }
        }
    }

    IEnumerator StopRbAfter(Rigidbody2D targetRb, float t)
    {
        yield return new WaitForSeconds(t);
        if (targetRb != null) targetRb.linearVelocity = Vector2.zero;
    }
}
