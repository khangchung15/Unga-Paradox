using Pathfinding;
using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int startingHealth = 100;
    public AudioClip deathSound;
    public AudioClip hurtSound1;
    public AudioClip hurtSound2;
    public AudioClip hurtSound3;
    public AudioClip[] hurtSounds;
    public int currentHealth;

    private AudioSource audioSource;
    private Animator anim;
    private GameObject wandering;
    private GameObject chasing;
    private Rigidbody2D rb;
    private Flash flash;
    
    public UnityEvent onDeath;

    private void Start()
    {
        currentHealth = startingHealth;
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        wandering = GetComponentInChildren<General_Enemy_Wandering>().gameObject;
        chasing = GetComponentInChildren<General_Enemy_Chasing_Test>().gameObject;
        hurtSounds = new AudioClip[] { hurtSound1, hurtSound2, hurtSound3 };
        rb = GetComponent<Rigidbody2D>();
        flash = GetComponent<Flash>();
    }

    // Damage the enemy
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0)
        {
            return;
        }
        
        currentHealth -= damage;
        if (currentHealth > 0)
        {
            //anim.Play("Hurt");
            audioSource.PlayOneShot(hurtSounds[Random.Range(0, hurtSounds.Length)]);
        }
        //Debug.Log(currentHealth);
        StartCoroutine(flash.FlashRoutine());
    }

    // Check if the enemy is dead
    public void DetectDeath()
    {
        if (currentHealth <= 0)
        {
            rb.linearVelocity = Vector2.zero;
            wandering.SetActive(false);
            chasing.SetActive(false);
            anim.Play("Dead");
            audioSource.PlayOneShot(deathSound);
            
            onDeath.Invoke();
            Destroy(gameObject, deathSound.length);
        }
    }
}
