using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    [Header("Chase Settings")]
    public float moveSpeed = 3.0f;
    public float stopDistance = 1.0f;

    [Header("Dialogue Activation")]
    [Tooltip("Should this enemy wait for dialogue to end before chasing?")]
    public bool waitForDialogue = true;
    [Tooltip("NPC that triggers this enemy to start chasing")]
    public NPC triggerNPC;

    [Header("References")]
    public Transform playerTarget;
    public SpriteRenderer enemySprite;

    private bool isChasing = false;

    void Start()
    {
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }

        // Auto-disable if waiting for dialogue
        if (waitForDialogue)
        {
            isChasing = false;
            this.enabled = false; // Disable the script initially
        }
        else
        {
            isChasing = true;
        }

        // Find NPC if not assigned
        if (triggerNPC == null)
        {
            triggerNPC = FindObjectOfType<NPC>();
        }
    }

    void Update()
    {
        if (!isChasing || playerTarget == null || !playerTarget.gameObject.activeInHierarchy)
        {
            return;
        }

        ChasePlayer();
    }

    private void ChasePlayer()
    {
        Vector3 direction = playerTarget.position - transform.position;
        float distance = direction.magnitude;

        if (distance <= stopDistance)
        {
            return;
        }

        direction.Normalize();
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        if (enemySprite != null)
        {
            if (direction.x > 0)
            {
                enemySprite.flipX = true;
            }
            else if (direction.x < 0)
            {
                enemySprite.flipX = false;
            }
        }
    }

    // Call this method when dialogue ends
    public void StartChasing()
    {
        isChasing = true;
        this.enabled = true;
        Debug.Log($"{gameObject.name} started chasing the player!");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
        
        if (playerTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, playerTarget.position);
        }
    }
}