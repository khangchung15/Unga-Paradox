using UnityEngine;

// Enemy bullet logic 
public class EnemyBullet : MonoBehaviour
{
    private PlayerController playerScript;
    private Vector2 targetPosition;
    public int damage;
    public float speed;
    private void Start()
    {
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        targetPosition = playerScript.transform.position;
    }

    private void Update()
    {
        if (Vector2.Distance(transform.position, targetPosition) > .1f)
        {
            transform.position = Vector2.MoveTowards(transform.position,targetPosition, speed * Time.deltaTime);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            playerScript.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
