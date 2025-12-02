using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    [Header("Chase Settings")]
    public float moveSpeed = 3.0f;
    public float stopDistance = 1.0f;
    public float attackRange = 1.5f;

    [Header("Dialogue Activation")]
    [Tooltip("Should this enemy wait for dialogue to end before chasing?")]
    public bool waitForDialogue = true;
    [Tooltip("NPC that triggers this enemy to start chasing")]
    public NPC triggerNPC;

    [Header("References")]
    public Transform playerTarget;
    public SpriteRenderer enemySprite;
    public Animator animator;

    [Header("Attack Settings")]
    public float attackCooldown = 1.5f;
    public float attackDamage = 10f;
    public AttackIndicator attackIndicator;

    private bool isChasing = false;
    private bool isAttacking = false;
    private float lastAttackTime;
    private float attackChargeTime;
    private bool isChargingAttack = false;

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

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (attackIndicator == null)
        {
            attackIndicator = FindObjectOfType<AttackIndicator>();
        }

        if (waitForDialogue)
        {
            isChasing = false;
            this.enabled = false;
        }
        else
        {
            isChasing = true;
        }

        if (triggerNPC == null)
        {
            triggerNPC = FindObjectOfType<NPC>();
        }

        if (attackIndicator != null)
        {
            attackIndicator.Hide();
        }
    }

    void Update()
    {
        if (!isChasing || playerTarget == null || !playerTarget.gameObject.activeInHierarchy)
        {
            if (animator != null)
            {
                animator.SetBool("isMoving", false);
                animator.SetBool("isAttacking", false);
            }

            if (attackIndicator != null && isChargingAttack)
            {
                isChargingAttack = false;
                attackIndicator.Hide();
            }

            return;
        }

        float distance = Vector3.Distance(transform.position, playerTarget.position);

        if (distance <= attackRange)
        {
            AttackPlayer();
        }
        else if (distance <= stopDistance)
        {
            if (animator != null)
            {
                animator.SetBool("isMoving", false);
                animator.SetBool("isAttacking", false);
            }

            if (isChargingAttack)
            {
                isChargingAttack = false;
                if (attackIndicator != null)
                {
                    attackIndicator.Hide();
                }
            }
        }
        else
        {
            ChasePlayer();

            if (isChargingAttack)
            {
                isChargingAttack = false;
                if (attackIndicator != null)
                {
                    attackIndicator.Hide();
                }
            }
        }

        if (isChargingAttack)
        {
            float chargeProgress = (Time.time - attackChargeTime) / attackCooldown;
            if (attackIndicator != null)
            {
                attackIndicator.SetFillAmount(chargeProgress);
            }
        }
    }

    private void ChasePlayer()
    {
        Vector3 direction = playerTarget.position - transform.position;
        direction.Normalize();
        transform.position += direction * moveSpeed * Time.deltaTime;

        if (animator != null)
        {
            animator.SetBool("isMoving", true);
            animator.SetBool("isAttacking", false);
        }

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

    private void AttackPlayer()
    {
        if (animator != null)
        {
            animator.SetBool("isMoving", false);
        }

        if (!isChargingAttack && Time.time >= lastAttackTime + attackCooldown)
        {
            isChargingAttack = true;
            attackChargeTime = Time.time;

            if (attackIndicator != null)
            {
                attackIndicator.Show();
                attackIndicator.ResetIndicator();
            }
        }

        if (isChargingAttack && Time.time >= attackChargeTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            isChargingAttack = false;

            if (attackIndicator != null)
            {
                attackIndicator.Hide();
            }

            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        HealthFuture playerHealthFuture = playerTarget.GetComponent<HealthFuture>();
        if (playerHealthFuture != null)
        {
            playerHealthFuture.TakeDamage(attackDamage);
            return;
        }

        Health playerHealth = playerTarget.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }

    public void StartChasing()
    {
        isChasing = true;
        this.enabled = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (playerTarget != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, playerTarget.position);
        }
    }
}
