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
        
        foreach (Collider2D hitCollider in hitColliders)
        {
            GameObject targetObject = hitCollider.gameObject;
            
            if (targetObject.CompareTag("Player") || targetObject.CompareTag("Boss") || targetObject.CompareTag("Enemy"))
            {
                FreezeTarget(targetObject);
            }
            else if (targetObject.transform.parent != null)
            {
                GameObject parent = targetObject.transform.parent.gameObject;
                if (parent.CompareTag("Player") || parent.CompareTag("Boss") || parent.CompareTag("Enemy"))
                {
                    FreezeTarget(parent);
                }
            }
        }
    }
    
    private void FreezeTarget(GameObject target)
    {
        if (target.CompareTag("Boss"))
        {
            BossShield bossShield = target.GetComponentInChildren<BossShield>();
            if (bossShield != null && bossShield.IsShieldActive)
            {
                return;
            }
            
        }
        
        FrozenEntity frozenEntity = target.GetComponent<FrozenEntity>();
        
        if (frozenEntity == null)
        {
            frozenEntity = target.AddComponent<FrozenEntity>();
        }
        
        frozenEntity.Freeze(freezeDuration, freezeOverlaySprite, freezeTint);
    }
}
