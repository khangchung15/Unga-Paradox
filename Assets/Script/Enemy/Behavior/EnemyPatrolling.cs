using System;
using System.Collections;
using UnityEngine;

public class General_Enemy_Patrolling : MonoBehaviour
{
    [Header("Patrol Settings")]
    [Tooltip("Input for manually set up the waypoints for NPC")]
    public Vector2[] patrolPoints;

    [Tooltip("Movement speed of the NPC.")]
    public float moveSpeed = 2;

    [Tooltip("Distance to consider a waypoint reached.")]
    public float waypointThreshold = 0.1f;

    [Tooltip("Minimum Rest Time between Patrol Points")]
    public float minRestTime = 0.5f;

    [Tooltip("Maximum Rest Time between Patrol Points")]
    public float maxRestTime = 2f;

    private Vector2 target;

    private Rigidbody2D rb;
    private Animator anim;
    private int currentPatrolIndex = 0;
    private bool isResting;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        StartCoroutine(SetPatrolPoint()); // start the patrol routine
    }

    void Update()
    {
        if (isResting)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        Vector2 direction = ((Vector3)target - transform.position).normalized;
        
        //if(direction.x < 0 && transform.localScale.x > 0 || direction.x > 0 && transform.localScale.x < 0)
        //{
        //    transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        //}

        if (direction.x > 0 )
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x * - 1), transform.localScale.y, transform.localScale.z);
        }
        else if (direction.x < 0 )
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
            
        rb.linearVelocity = direction.normalized * moveSpeed;

        if (Vector2.Distance(transform.position, target) < .1f) // check when a waypoint is reached
        {
            StartCoroutine(SetPatrolPoint()); // similar to async/await in JS, but for Unity C#
           // Debug.Log(isResting);
        }
    }

    IEnumerator SetPatrolPoint()
    {
        isResting = true;
        anim.Play("Idle"); // play idle animation when resting
        // this allows ONLY the npc to be resting without stopping the game
        yield return new WaitForSeconds(UnityEngine.Random.Range(minRestTime, maxRestTime)); // wait for a random time between minRestTime and maxRestTime
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length; // this will never exceed the bounds of the array since it will always go back to 0, when it reaches the length of patrolPoints
        target = patrolPoints[currentPatrolIndex]; // set the new patrol point
        isResting = false;
        anim.Play("Walk"); // play walk animation when moving
    }
    // Draw lines to visualize patrol points and path in the editor
    void OnDrawGizmos()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Gizmos.color = Color.red;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Gizmos.DrawSphere((Vector3)patrolPoints[i], 0.15f);
            int nextIndex = (i + 1) % patrolPoints.Length;
            Gizmos.DrawLine((Vector3)patrolPoints[i], (Vector3)patrolPoints[nextIndex]);
        }
    }
}