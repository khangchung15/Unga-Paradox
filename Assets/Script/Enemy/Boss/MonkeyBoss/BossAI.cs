using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private MonoBehaviour enemyType;
    [SerializeField] private EnemyPathfinding enemyPathfinding;      
    [SerializeField] private Transform target;                        
    
    [Header("Tuning")]
    [SerializeField] private float roamChangeDirectionCooldown = 2f;
    [SerializeField] private float attackRange = 0f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private bool stopMovingWhileAttacking;

    private enum State { Roaming, Attacking }
    private State state;

    private Vector2 roamPosition;
    private float timeRoaming = 0f;
    private bool canAttack = true;

    void Awake()
    {
        if (!enemyPathfinding) enemyPathfinding = GetComponent<EnemyPathfinding>();
        state = State.Roaming;

        if (target == null)
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null) target = playerGO.transform;
        }
    }

    void Start()
    {
        roamPosition = GetRoamingPosition();
    }

    void Update()
    {
        MovementStateControl();
    }

    private void MovementStateControl()
    {
        switch (state)
        {
            case State.Roaming:   Roaming();   break;
            case State.Attacking: Attacking(); break;
        }
    }

    private void Roaming()
    {
        timeRoaming += Time.deltaTime;

        enemyPathfinding.MoveTo(roamPosition);

        if (target != null && Vector2.Distance(transform.position, target.position) <= attackRange)
        {
            state = State.Attacking;
        }

        if (timeRoaming > roamChangeDirectionCooldown)
        {
            roamPosition = GetRoamingPosition();
        }
    }

    private void Attacking()
    {
        if (target == null)
        {
            state = State.Roaming;
            return;
        }

        if (Vector2.Distance(transform.position, target.position) > attackRange)
        {
            state = State.Roaming;
            return;
        }

        if (attackRange != 0 && canAttack)
        {
            canAttack = false;

            (enemyType as IBoss)?.Attack(); 

            if (stopMovingWhileAttacking) enemyPathfinding.StopMoving();
            else enemyPathfinding.MoveTo(roamPosition);

            StartCoroutine(AttackCooldownRoutine());
        }
    }

    private IEnumerator AttackCooldownRoutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private Vector2 GetRoamingPosition()
    {
        timeRoaming = 0f;
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }
}
