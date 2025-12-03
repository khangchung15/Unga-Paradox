using System.Collections.Generic;
using UnityEngine;

public class DeathBeamDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damagePerSecond = 10f;
    [SerializeField] private string targetTag = "Player";
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private HashSet<Collider2D> targetsInBeam = new HashSet<Collider2D>();
    
    private void OnEnable()
    {
        targetsInBeam.Clear();
    }
    
    private void OnDisable()
    {
        targetsInBeam.Clear();
    }
    
    private void Update()
    {
        if (targetsInBeam.Count == 0) return;
        
        List<Collider2D> toRemove = new List<Collider2D>();
        
        foreach (Collider2D target in targetsInBeam)
        {
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                toRemove.Add(target);
                continue;
            }
            
            HealthFuture playerHealthFuture = target.GetComponent<HealthFuture>();
            if (playerHealthFuture != null)
            {
                float damageThisFrame = damagePerSecond * Time.deltaTime;
                playerHealthFuture.TakeDamage(damageThisFrame);
                
                if (showDebugLogs)
                {
                    Debug.Log($"Beam dealt {damageThisFrame} damage to {target.name} (HP: {playerHealthFuture.currentHealth}/{playerHealthFuture.maxHealth})");
                }
                continue;
            }
            
            Health playerHealth = target.GetComponent<Health>();
            if (playerHealth != null)
            {
                float damageThisFrame = damagePerSecond * Time.deltaTime;
                playerHealth.TakeDamage(damageThisFrame);
                
                if (showDebugLogs)
                {
                    Debug.Log($"Beam dealt {damageThisFrame} damage to {target.name}");
                }
            }
        }
        
        foreach (Collider2D target in toRemove)
        {
            targetsInBeam.Remove(target);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(targetTag)) return;
        
        targetsInBeam.Add(collision);
        
        if (showDebugLogs)
        {
            Debug.Log($"Beam started hitting {collision.name}");
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (targetsInBeam.Contains(collision))
        {
            targetsInBeam.Remove(collision);
            
            if (showDebugLogs)
            {
                Debug.Log($"Beam stopped hitting {collision.name}");
            }
        }
    }
}
