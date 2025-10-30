using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class ProjectileDamageSource : MonoBehaviour
{
    [SerializeField] private int damageAmount = 25;
    [SerializeField] private AudioClip damageClip = null;
    [SerializeField] private float destroyTime = 2.0f;
    private AudioSource audioSource = null;
    //[SerializeField] private int knockAmount = 10;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        destroyTime -= Time.deltaTime;
        if (destroyTime <= 0)
        {
            Destroy(gameObject);
        }
    }

    // Triggers when the attack hits an enemy
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Enemy takes damage
        if (other.gameObject.GetComponent<EnemyHealth>() || other.gameObject.GetComponent<BossHealth>())
        {
            // Probably gotta come back around to clean this up later.
            if (other.gameObject.GetComponent<EnemyHealth>())
            {
                EnemyHealth enemyHealth = other.gameObject.GetComponent<EnemyHealth>();
                enemyHealth.TakeDamage(damageAmount);
            }

            if (other.gameObject.GetComponent<BossHealth>())
            {
                BossHealth bossHealth = other.gameObject.GetComponent<BossHealth>();
                bossHealth.TakeDamage(damageAmount);
            }
            //Knockback knockback = other.gameObject.GetComponent<Knockback>();
            //knockback.GetKnockedBack(transform, 10);
        }

        // Player takes damage
        if (other.gameObject.GetComponent<Health>())
        {
            Debug.Log("Health Component Found");
            Health playerHealth = other.gameObject.GetComponent<Health>();
            playerHealth.TakeDamage(damageAmount);
        }

        if (audioSource != null && damageClip != null)
        {
            audioSource.PlayOneShot(damageClip);
        }

        Destroy(gameObject, 0.1f);
    }
}
