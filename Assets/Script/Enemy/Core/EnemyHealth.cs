using Pathfinding;
using UnityEngine;
using System.Collections;   

public class EnemyHealth : MonoBehaviour
{
    protected int startingHealth = 100;
    protected int currentHealth;
    //public AudioClip deathSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;

    private AudioSource audioSource;
    private Rigidbody2D rb;
    //private Flash flash;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float flashDuration = 0.2f;
    public int StartingHealth
    {
        get => startingHealth;
        set => startingHealth = value;
    }
    private void Start()
    {
        currentHealth = StartingHealth;
        audioSource = GetComponent<AudioSource>();

        
        rb = GetComponent<Rigidbody2D>();
        //flash = GetComponent<Flash>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    // Damage the enemy
    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth > 0)
        {
            if (audioSource != null && hurtSound != null)
            {
                audioSource.pitch = UnityEngine.Random.Range(0.7f, 1.3f);
                audioSource.PlayOneShot(hurtSound);
            }

            if (spriteRenderer != null)
                StartCoroutine(FlashWhite());
        }

        if (currentHealth <= 0)
        {
            DetectDeath();
        }
        Debug.Log(currentHealth);
        //StartCoroutine(flash.FlashRoutine());
    }

    // Check if the enemy is dead
    public virtual void DetectDeath()
    {
        if (audioSource != null && deathSound != null)
        {
            audioSource.pitch = UnityEngine.Random.Range(0.7f, 1.3f);
            audioSource.PlayOneShot(deathSound, 0.3f);
        }
    }

    public void GetCurrentHealth()
    {

    }

    public bool IsDead()
    { 
        return currentHealth <= 0;
    }
    private IEnumerator FlashWhite()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

}
