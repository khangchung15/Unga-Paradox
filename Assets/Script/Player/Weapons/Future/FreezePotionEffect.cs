using UnityEngine;

public class FreezePotionEffect : PotionEffect
{
    [Header("Freeze Settings")]
    [SerializeField] private float freezeDuration = 3f;
    [SerializeField] private Sprite freezeOverlaySprite;
    [SerializeField] private Color freezeTint = new Color(0.5f, 0.8f, 1f, 1f);
    
    protected override void ApplyEffect(Vector3 position)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position, aoeRadius);
        Debug.Log($"[FreezePotion] AoE hit {hitColliders.Length} colliders at {position}");
        
        foreach (Collider2D hitCollider in hitColliders)
        {
            GameObject targetObject = hitCollider.gameObject;
            
            if (targetObject.CompareTag("Player") || targetObject.CompareTag("Boss") || targetObject.CompareTag("Enemy"))
            {
                Debug.Log($"[FreezePotion] Direct freeze on {targetObject.name}");
                FreezeTarget(targetObject);
            }
            else if (targetObject.transform.parent != null)
            {
                GameObject parent = targetObject.transform.parent.gameObject;
                if (parent.CompareTag("Player") || parent.CompareTag("Boss") || parent.CompareTag("Enemy"))
                {
                    Debug.Log($"[FreezePotion] Freeze parent {parent.name} (hit child {targetObject.name})");
                    FreezeTarget(parent);
                }
            }
        }
    }
    
    private void FreezeTarget(GameObject target)
    {
        FrozenEntity frozenEntity = target.GetComponent<FrozenEntity>();
        
        if (frozenEntity == null)
        {
            Debug.Log($"[FreezePotion] Adding FrozenEntity to {target.name}");
            frozenEntity = target.AddComponent<FrozenEntity>();
        }
        else
        {
            Debug.Log($"[FreezePotion] {target.name} already has FrozenEntity (frozen: {frozenEntity.IsFrozen})");
        }
        
        frozenEntity.Freeze(freezeDuration, freezeOverlaySprite, freezeTint);
    }
}
