using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BossHealth : MonoBehaviour
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

    private Knockback knockback;
    public int currentHealth;
    public int maxHealth;
    private AudioSource audioSource;
    private bool alreadySecondStage = false;
    private bool isDead = false;
    
    public UnityEvent onDeath;

    private void Awake()
    {
        knockback = GetComponent<Knockback>();
        knockback = GetComponent<Knockback>();
        audioSource = GetComponent<AudioSource>();
        shots = GetComponent<BossShooter>();
    }

    private void Start()
    {
        currentHealth = startingHealth;
        startingPositon = transform.position;
        hurtSounds = new AudioClip[] { hurtSound1, hurtSound2 };
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0)
        {
            DetectDeath();
        }
        currentHealth = (damageAmount >= currentHealth) ? 0 : currentHealth - damageAmount;
        healthBar.SetValue(currentHealth);
        
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
        }
    }

    private void DetectDeath()
    {
        audioSource.PlayOneShot(deathSound);

        if (isDead) return;
        isDead = true;

        // Spawn VFX
        if (deathVFXPrefab != null)
        {
            Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
        }

        if (healthBar != null) Destroy(healthBar.gameObject);

        // Stop Music when he dies
        GameObject Camera = GameObject.FindGameObjectWithTag("MainCamera");
        Camera.GetComponent<AudioSource>().Stop();
        
        if (deathVFXPrefab != null)
            Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
        
        onDeath.Invoke();
        
        Destroy(gameObject,deathSound.length);
    }
}