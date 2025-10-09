using Pathfinding;
using UnityEngine;

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

    private void Start()
    {
        currentHealth = startingHealth;
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        wandering = GetComponentInChildren<EnemyWandering>().gameObject;
        chasing = GetComponentInChildren<EnemyChasing>().gameObject;
        hurtSounds = new AudioClip[] { hurtSound1, hurtSound2, hurtSound3 };
        rb = GetComponent<Rigidbody2D>();
        flash = GetComponent<Flash>();
    }

    // Damage the enemy
    public void TakeDamage(int damage)
    {
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
            Destroy(gameObject, deathSound.length);
        }
    }
}
