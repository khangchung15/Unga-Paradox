using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GroundWebTrap : MonoBehaviour
{
    [SerializeField] private float lifetime = 4f;
    [Range(0.05f, 1f)] [SerializeField] private float slowMultiplier = 0.40f;
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private bool blockDash = true;

    private readonly HashSet<PlayerController> affected = new HashSet<PlayerController>();
    private string ModKey => $"web_{GetInstanceID()}";

    private void Start()
    {
        if (lifetime > 0f) Destroy(gameObject, lifetime);
    }

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;

        // Ensure there is a RB2D on the trap so triggers are reliable
        if (GetComponent<Rigidbody2D>() == null)
        {
            var rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.simulated = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Accept root or child collider; allow tag on root RB
        if (!other.CompareTag(targetTag) && !(other.attachedRigidbody && other.attachedRigidbody.CompareTag(targetTag)))
            return;

        var pc = other.GetComponent<PlayerController>() ?? other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        if (affected.Add(pc))
        {
            pc.AddOrUpdateSpeedMod(ModKey, slowMultiplier); // <-- updates slowMul in PlayerController
            if (blockDash) pc.AddDashBlock();
            Debug.Log($"[WEB] ENTER {pc.name} m={slowMultiplier}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(targetTag) && !(other.attachedRigidbody && other.attachedRigidbody.CompareTag(targetTag)))
            return;

        var pc = other.GetComponent<PlayerController>() ?? other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        if (affected.Remove(pc))
        {
            pc.RemoveSpeedMod(ModKey);        // <-- restores slowMul
            if (blockDash) pc.RemoveDashBlock();
            Debug.Log($"[WEB] EXIT {pc.name}");
        }
    }

    private void OnDestroy()
    {
        foreach (var pc in affected)
        {
            if (!pc) continue;
            pc.RemoveSpeedMod(ModKey);
            if (blockDash) pc.RemoveDashBlock();
        }
        affected.Clear();
    }
}

