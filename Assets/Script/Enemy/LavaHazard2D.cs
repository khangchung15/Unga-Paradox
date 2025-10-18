using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LavaHazard2D : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damagePerSecond = 1000f;
    [SerializeField] private float enterBonusDamage = 0f;

    [Header("Filtering")]
    [SerializeField] private string damageTag = "Player";
    [SerializeField] private bool damageOnlyRoot = true;

    private readonly HashSet<Health> _inside = new HashSet<Health>();
    private Collider2D _col;

    void Awake()
    {
        _col = GetComponent<Collider2D>();
        if (_col && !_col.isTrigger) _col.isTrigger = true; // ensure trigger
    }

    void OnEnable()
    {
        _inside.Clear();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var h = FindHealth(other);
        if (h == null) return;

        if (_inside.Add(h))
        {
            if (enterBonusDamage > 0f)
            {
                h.TakeDamage(enterBonusDamage);
                Debug.Log($"[LavaHazard2D] {h.name} entered lava. Bonus damage: {enterBonusDamage}");
            }
            else
            {
                Debug.Log($"[LavaHazard2D] {h.name} entered lava.");
            }
        }
    }

    // Optional: keeps set correct if you enter without firing Enter (complex collider setups)
    void OnTriggerStay2D(Collider2D other)
    {
        var h = FindHealth(other);
        if (h != null) _inside.Add(h);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var h = FindHealth(other);
        if (h == null) return;

        if (_inside.Remove(h))
            Debug.Log($"[LavaHazard2D] {h.name} exited lava. Stopping damage.");
    }

    void Update()
    {
        if (_inside.Count == 0) return;

        float dmg = damagePerSecond * Time.deltaTime;
        foreach (var h in _inside)
        {
            if (h == null) continue;
            h.TakeDamage(dmg);
            // Debug.Log($"[LavaHazard2D] Damaging {h.name}: {dmg:F2} this frame. Current HP: {h.currentHealth:F2}");
        }
    }

    Health FindHealth(Collider2D col)
    {
        if (!string.IsNullOrEmpty(damageTag))
        {
            bool tagMatch = col.CompareTag(damageTag);

            if (!tagMatch && damageOnlyRoot && col.attachedRigidbody)
                tagMatch = col.attachedRigidbody.CompareTag(damageTag);

            if (!tagMatch) return null;
        }

        Health h = null;

        if (damageOnlyRoot && col.attachedRigidbody)
            h = col.attachedRigidbody.GetComponent<Health>();

        if (h == null)
            h = col.GetComponent<Health>() 
                ?? col.GetComponentInParent<Health>() 
                ?? col.GetComponentInChildren<Health>();

        return h;
    }
}
