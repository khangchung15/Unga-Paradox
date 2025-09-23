using UnityEngine;


// This is the enemy class that every enemy will need to implement.
public class Enemy : MonoBehaviour
{
    public int health;
    public int baseAttack;
    public float timeBetweenAttacks;
    public float attackSpeed;

    [HideInInspector]
    public Transform player;

    public float speed;

    public virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    
    // Pass in values to damage the enemy
    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
