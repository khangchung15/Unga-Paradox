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
    [SerializeField] private Sprite defeatedSprite;

    [Header("Objects to Disable on Death")]
    [Tooltip("GameObjects that will be disabled when boss dies (e.g., player weapons, inventory UI)")]
    [SerializeField] private GameObject[] objectsToDisableOnDeath;

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
    private SpriteRenderer spriteRenderer;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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
        
        if (hurtSound1 != null && hurtSound2 != null)
        {
            hurtSounds = new AudioClip[] { hurtSound1, hurtSound2 };
        }
        
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead || currentHealth <= 0)
        {
            DetectDeath();
            return;
        }
        
        currentHealth = (damageAmount >= currentHealth) ? 0 : currentHealth - damageAmount;
        
        if (healthBar != null)
        {
            healthBar.SetValue(currentHealth);
        }
        
        
        if (currentHealth > 0)
        {
            if (audioSource != null && hurtSounds != null && hurtSounds.Length > 0)
            {
                audioSource.PlayOneShot(hurtSounds[Random.Range(0, hurtSounds.Length)]);
            }
        }

        if (currentHealth <= startingHealth / 2 && alreadySecondStage == false)
        {
            if (shots != null)
            {
                shots.burstCount = 10;
                shots.projectileMoveSpeed = 4;
                shots.shootCooldown = 3;
                shots.angleSpread = 359;
                shots.projectilesPerBurst = 100;
                shots.stagger = false;
            }
            
            gameObject.transform.position = startingPositon;
            alreadySecondStage = true;
            
            if (audioSource != null && tpSound != null)
            {
                audioSource.PlayOneShot(tpSound);
            }
            
        }
        
        if (currentHealth <= 0)
        {
            DetectDeath();
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
    }

    private void DetectDeath()
    {
        if (isDead) return;
        isDead = true;
        
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        if (shots != null)
            shots.enabled = false;
        
        GameObject bossDeathSpawn = GameObject.Find("Boss Death Spawn");
        if (bossDeathSpawn != null)
        {
            transform.position = bossDeathSpawn.transform.position;
            
            if (rb2d != null)
            {
                rb2d.linearVelocity = Vector2.zero;
                rb2d.angularVelocity = 0f;
                rb2d.bodyType = RigidbodyType2D.Kinematic;
                rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            
            BossController bossController = GetComponent<BossController>();
            if (bossController != null)
            {
                bossController.enabled = false;
            }
            
            foreach (GameObject obj in objectsToDisableOnDeath)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }

            DeathBeamShooter[] beamShooters = FindObjectsOfType<DeathBeamShooter>();
            foreach (DeathBeamShooter shooter in beamShooters)
            {
                if (shooter != null)
                {
                    shooter.ForceStop();
                    shooter.enabled = false;
                }
            }
            
            if (animator != null)
            {
                animator.Play("Recover", 0, 0f);
                animator.speed = 0f;
            }
            
            SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in spriteRenderers)
            {
                if (sr.gameObject.name == "Shield")
                {
                    sr.enabled = false;
                }
            }
            
            if (spriteRenderer != null && defeatedSprite != null)
            {
                spriteRenderer.sprite = defeatedSprite;
            }
            
            BossDeathSequence deathSequence = bossDeathSpawn.GetComponent<BossDeathSequence>();
            if (deathSequence != null)
            {
                deathSequence.StartDeathSequence();
            }
        }
        
        if (healthBar != null) 
            Destroy(healthBar.gameObject);

        GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCamera != null)
        {
            AudioSource cameraAudio = mainCamera.GetComponent<AudioSource>();
            if (cameraAudio != null)
            {
                cameraAudio.Stop();
            }
        }
        
        onDeath.Invoke();
    }

}
