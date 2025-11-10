using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] float damage;

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>())
        {
            if (collision.gameObject.GetComponent<Health>() == null)
            {
                
            }
            var healthController = collision.gameObject.GetComponent<Health>();
            
            healthController.TakeDamage(damage);
        }
    }
}
