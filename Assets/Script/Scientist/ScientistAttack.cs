using UnityEngine;
using UnityEngine.InputSystem;

public class ScientistAttack : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 15f;
    [SerializeField] private float projectileLifetime = 3f;
    
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private int projectileDamage = 10;
    
    [Header("Audio")]
    [SerializeField] private AudioClip shootSound;
    
    [Header("Input")]
    [SerializeField] private InputAction attackAction;
    
    private AudioSource audioSource;
    private float lastAttackTime = -999f;
    private ScientistController scientistController;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        scientistController = GetComponent<ScientistController>();
    }
    
    private void OnEnable()
    {
        attackAction.Enable();
        attackAction.performed += OnAttackPerformed;
    }
    
    private void OnDisable()
    {
        attackAction.performed -= OnAttackPerformed;
        attackAction.Disable();
    }
    
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        TryAttack();
    }
    
    private void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown)
            return;
        
        if (scientistController != null && scientistController.state == ScientistController.PlayerState.Dead)
            return;
        
        PerformAttack();
        lastAttackTime = Time.time;
    }
    
    private void PerformAttack()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("No projectile prefab assigned to ScientistAttack!");
            return;
        }
        
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        
        bool facingRight = scientistController != null && 
                          scientistController.facing == ScientistController.PlayerDirection.Right;
        
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        float angle = facingRight ? 0f : 180f;
        
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.Euler(0, 0, angle));
        
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * projectileSpeed;
        }
        
        SideScrollerProjectile projectileScript = projectile.GetComponent<SideScrollerProjectile>();
        if (projectileScript != null)
        {
            projectileScript.SetDamage(projectileDamage);
        }
        
        Destroy(projectile, projectileLifetime);
        
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
    }
}
