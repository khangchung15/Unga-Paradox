using UnityEngine;

[RequireComponent(typeof(EnemyStateMachine))]
public class BombMonkeyExploder : MonoBehaviour
{
    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 2.5f;
    [SerializeField] private float explosionDamage = 35f;
    [SerializeField] private LayerMask damageMask;
    [SerializeField] private bool applyKnockback = true;
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private GameObject explosionVFX;
    [SerializeField] private AudioClip explosionSFX;

    [Header("Audio")]
    [SerializeField] private AudioClip deathSFX;  // <- new
    [SerializeField] private float selfDestructDelay = 0.05f;

    private bool exploded;
    public bool HasExploded => exploded;
    
    public void TriggerExplosion(bool selfDetonated)
    {
        if (exploded) return;
        exploded = true;

        var hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damageMask);
        foreach (var hit in hits)
        {
            var hp = hit.GetComponent<Health>();
            if (hp != null) hp.TakeDamage(explosionDamage);

            if (applyKnockback)
            {
                var rb = hit.attachedRigidbody;
                if (rb != null)
                {
                    Vector2 dir = (hit.transform.position - transform.position).normalized;
                    rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
                }
            }
        }

        if (explosionVFX != null)
            Instantiate(explosionVFX, transform.position, Quaternion.identity);
        if (explosionSFX != null)
            AudioSource.PlayClipAtPoint(explosionSFX, transform.position);
        
        if (deathSFX != null)
            AudioSource.PlayClipAtPoint(deathSFX, transform.position);

        var enemy = GetComponent<Enemy>();
        if (enemy != null) enemy.ForceState(EnemyStateMachine.EnemyState.Dead);
        else Destroy(gameObject, selfDestructDelay);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
#endif
}