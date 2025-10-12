using System.Runtime.CompilerServices;
using UnityEngine;

public class DamageSource : MonoBehaviour
{
    [SerializeField] private int damageAmount = 25;
    //[SerializeField] private int knockAmount = 10;

    // Triggers when the attack hits an enemy
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Enemy takes damage
        if (other.gameObject.GetComponent<EnemyHealth>())
        {
            EnemyHealth enemyHealth = other.gameObject.GetComponent<EnemyHealth>();
            //Knockback knockback = other.gameObject.GetComponent<Knockback>();
            enemyHealth.TakeDamage(damageAmount);
            //knockback.GetKnockedBack(transform, 10);
        }

        // Player takes damage
        if (other.gameObject.GetComponent<Health>())
        {
            Debug.Log("Health Component Found");
            Health playerHealth = other.gameObject.GetComponent<Health>();
            playerHealth.TakeDamage(damageAmount);
        }
    }
}
