using UnityEngine;

public class TrapDamage : MonoBehaviour
{
    [SerializeField] float damage = 15f;
    [SerializeField] string targetTag = "Player";
    [SerializeField] bool onlyWhenDangerous = true;

    // Start SAFE; animation will toggle this on/off
    [HideInInspector] public bool isDangerous = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(targetTag)) return;
        if (onlyWhenDangerous && !isDangerous) return;

        var hp = other.GetComponent<Health>();
        if (hp != null) hp.TakeDamage(damage);
    }
}
