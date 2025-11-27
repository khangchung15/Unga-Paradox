using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BossHealthFuture : MonoBehaviour
{
    [SerializeField] private int startingHealth;
    [SerializeField] private GameObject deathVFXPrefab;
    [SerializeField] private BossHealthBar healthBar;
    [SerializeField] private BossShooter shots;
    public AudioClip hurtSound1;
    public AudioClip hurtSound2;
    public AudioClip deathSound;
    public AudioClip tpSound;
    public AudioClip[] hurtSounds;
    private Vector2 startingPositon;
    private Rigidbody2D rb2d;

    private Knockback knockback;
    public int currentHealth;
    public int maxHealth;
    private AudioSource audioSource;
    private bool alreadySecondStage = false;
    private bool isDead = false;
    
    public UnityEvent onDeath;
    
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        knockback = GetComponent<Knockback>();
        knockback = GetComponent<Knockback>();
        audioSource = GetComponent<AudioSource>();
        shots = GetComponent<BossShooter>();
        rb2d = GetComponent<Rigidbody2D>();
    }
    
    private void Start()
    {
        currentHealth = startingHealth;
        maxHealth = startingHealth;
        startingPositon = transform.position;
        hurtSounds = new AudioClip[] { hurtSound1, hurtSound2 };
        
        Debug.Log($"[BossHealth] Boss initialized with {currentHealth}/{maxHealth} HP");
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0)
        {
            DetectDeath();
        }
        currentHealth = (damageAmount >= currentHealth) ? 0 : currentHealth - damageAmount;
        
        if (healthBar != null)
        {
            healthBar.SetValue(currentHealth);
        }
        
        Debug.Log($"[BossHealth] Boss took {damageAmount} damage. Current HP: {currentHealth}/{maxHealth}");
        
        if (currentHealth > 0)
        {
            audioSource.PlayOneShot(hurtSounds[Random.Range(0, hurtSounds.Length)]);
        }

        if (currentHealth <= startingHealth / 2 && alreadySecondStage == false)
        {
            shots.burstCount = 10;
            shots.projectileMoveSpeed = 4;
            shots.shootCooldown = 3;
            shots.angleSpread = 359;
            shots.projectilesPerBurst = 100;
            shots.stagger = false;
            gameObject.transform.position = startingPositon;
            alreadySecondStage = true;
            audioSource.PlayOneShot(tpSound);
            Debug.Log("[BossHealth] Boss entered second stage!");
        }
    }
    
    public void Heal(float healAmount)
    {
        if (isDead) return;
        
        currentHealth += (int)healAmount;
        
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        
        if (healthBar != null)
        {
            healthBar.SetValue(currentHealth);
        }
        
        Debug.Log($"[BossHealth] Boss healed for {healAmount}. Current HP: {currentHealth}/{maxHealth}");
    }

    private void DetectDeath()
    {
        audioSource.PlayOneShot(deathSound);

        if (isDead) return;
        isDead = true;
        
        Debug.Log("[BossHealth] Boss died!");
        
        if (shots != null)
            shots.enabled = false;
        
        if (animator != null)
            animator.ResetTrigger("Dead");
            animator.SetTrigger("Dead");

        if (rb2d) {
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
            rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        
        if (deathVFXPrefab != null)
        {
            Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
        }

        if (healthBar != null) Destroy(healthBar.gameObject);

        GameObject Camera = GameObject.FindGameObjectWithTag("MainCamera");
        Camera.GetComponent<AudioSource>().Stop();

        if (deathVFXPrefab != null)
        {
            Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
        }
        
        onDeath.Invoke();

        Destroy(gameObject,deathSound.length);
    }
}
