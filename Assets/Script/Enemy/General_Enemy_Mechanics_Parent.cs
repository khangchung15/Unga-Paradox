using UnityEngine;
using Pathfinding;

// this script is to be placed on the parent gameObject of the enemy, to relay all events to the child scripts, and manage them in here.
public class General_Enemy_Mechanics_Parent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private General_Enemy_Detection detectionScript;
    private General_Enemy_Wandering wanderingScript;
    private General_Enemy_Chasing_Test chasingScript;
    private General_Enemy_Collision collisionScript;
    private Seeker seeker;
    
    private void Awake()
    {
        detectionScript = GetComponentInChildren<General_Enemy_Detection>();
        wanderingScript = GetComponentInChildren<General_Enemy_Wandering>();
        chasingScript = GetComponentInChildren<General_Enemy_Chasing_Test>();
        collisionScript = GetComponentInChildren<General_Enemy_Collision>();
        seeker = GetComponent<Seeker>();

        if (detectionScript == null) throw new MissingComponentException("General_Enemy_Detection missing!");
        if (wanderingScript == null) throw new MissingComponentException("General_Enemy_Wandering missing!");
        if (chasingScript == null) throw new MissingComponentException("General_Enemy_Chasing_Test missing!");
        if (seeker == null) throw new MissingComponentException("Seeker missing!");
        if (collisionScript == null) Debug.LogWarning("General_Enemy_Collision script missing");

    }
    void Start()
    {
        if (chasingScript != null)
        {
            chasingScript.enabled = false; // disable chasing at start
            
        }
        seeker.drawGizmos = false; // disable path gizmos at start

    }

    // Update is called once per frame
    void Update()
    {

        if (detectionScript.hasLineOfSight)
        {
            seeker.drawGizmos = true;
            // when the enemy sees you, it stops wandering and runs after you
            if (wanderingScript.enabled)
            {
                wanderingScript.enabled = false;
               
            }
            if (!chasingScript.enabled)
            {
                chasingScript.enabled = true;
                
            }
           
        }
        else if (!detectionScript.hasLineOfSight)
        {
            seeker.drawGizmos = false;
            if (chasingScript.enabled)
            {
                chasingScript.enabled = false;
            }
            if (!wanderingScript.enabled)
            {
                wanderingScript.enabled = true;
                
            }
        }
    }
    void OnCollisionEnter2D(Collision2D collision) // handle collision
    {    
        if (collisionScript != null)
        {
            collisionScript.OnParentCollision(collision);
        }
            
    }
}
