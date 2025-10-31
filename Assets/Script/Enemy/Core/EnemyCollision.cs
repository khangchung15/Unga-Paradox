using UnityEngine;
using System.Collections;

public class EnemyCollision : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    [Tooltip("Seconds to temporarily ignore physics collisions after an enemy-enemy contact to avoid pushing.")]
    [SerializeField] private float tempIgnoreDuration = 0.15f;

    private EnemyWandering wanderingScript;
    private EnemyStateMachine stateMachine;
    private Rigidbody2D myRb;
    private EnemyMovement enemyMovement;

    void Awake()
    {   
        wanderingScript = GetComponentInChildren<EnemyWandering>(); // go to the parent, then actually get the component in the parent's children lmao
        stateMachine = GetComponent<EnemyStateMachine>();
        myRb = GetComponent<Rigidbody2D>();
        enemyMovement = GetComponentInChildren<EnemyMovement>();
        if (wanderingScript == null) 
            throw new MissingComponentException("EnemyWandering children script missing!");    
        if (stateMachine == null)
            throw new MissingComponentException("EnemyStateMachine script missing!");
        if (myRb == null)
            throw new MissingComponentException("Rigidbody2D component missing!");

    }

    private void OnCollisionEnter2D(Collision2D collision) // needs to update collision detection
    {
        var other = collision.collider.gameObject;

        // If other is an enemy or player only handle those cases
        if (!other.CompareTag("Enemy") && !other.CompareTag("Player"))
            return;

        // If either side is dead, ignore collisions between them so characters can walk through dead body

        if (other.CompareTag("Enemy"))
        {
            Debug.Log(HelperFuncs.GetOwnerName(transform) + " Collided With Enemy. Preventing push and choosing new wander point.");

            // zero both rigidbodies' velocities to stop any pushing effect
            TryZeroRigidbodyVelocity(myRb);
            TryZeroRigidbodyVelocity(other.GetComponent<Rigidbody2D>());

            // temporarily ignore collisions between the two so they won't be pushed while AI repositions
            StartTempIgnoreCollisionWith(other, tempIgnoreDuration);

            // restart wander behaviour
            if (EnemyStateMachine.EnemyState.Wandering == stateMachine.GetState())
            {
                wanderingScript.StopBehavior(); // stops coroutines on this component (keeps behavior consistent with previous code)
                wanderingScript.StartBehavior();

                return;
            }
            return;
        }
        
        if (other.CompareTag("Player"))
        {
            Debug.Log(HelperFuncs.GetOwnerName(transform) + " Collided With Player. Preventing push.");

            // zero both rigidbodies' velocities to stop any pushing effect
            TryZeroRigidbodyVelocity(myRb);
            TryZeroRigidbodyVelocity(other.GetComponent<Rigidbody2D>());
            // temporarily ignore collisions between the two so they won't be pushed while AI repositions
            StartTempIgnoreCollisionWith(other, tempIgnoreDuration);
        }

        if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            TryZeroRigidbodyVelocity(myRb);
            enemyMovement.Stop();
            if (EnemyStateMachine.EnemyState.Wandering == stateMachine.GetState())
            {

                wanderingScript.StopBehavior(); // stops coroutines on this component (keeps behavior consistent with previous code)
                wanderingScript.StartBehavior();

                return;
            }
        }   
    }

    private void TryZeroRigidbodyVelocity(Rigidbody2D rb)
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void StartTempIgnoreCollisionWith(GameObject other, float duration)
    {
        if (other == null) return;

        var myColliders = transform.GetComponents<Collider2D>();
        var otherColliders = other.GetComponents<Collider2D>();

        foreach (var a in myColliders)
        {
            foreach (var b in otherColliders)
            {
                if (a == null || b == null) continue;
                StartCoroutine(TempIgnorePair(a, b, duration));
            }
        }
    }

    private IEnumerator TempIgnorePair(Collider2D a, Collider2D b, float duration)
    {
        if (a == null || b == null) yield break;

        Physics2D.IgnoreCollision(a, b, true);
        yield return new WaitForSeconds(duration);

        // re-enable only if both colliders still exist
        if (a != null && b != null)
            Physics2D.IgnoreCollision(a, b, false);
    }
}
