using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    protected EnemyDetection enemyDetection;
    protected EnemyMovement enemyMovement;
    protected EnemyStateMachine stateMachine;
    protected Rigidbody2D rb;
    protected EnemyWandering enemyWandering;
    protected EnemyChasing enemyChasing;
    protected EnemyAttacking enemyAttacking;
    protected EnemyHealth enemyHealth;

    private Coroutine destroyCoroutine;

    // Tracks if the enemy has entered the dead state and had its shutdown logic applied
    private bool isDead = false;

    readonly float initialIdleDuration = 1f; // time to idle before starting wandering

    protected virtual void Awake()
    {
        enemyDetection = GetComponentInChildren<EnemyDetection>();
        rb = GetComponent<Rigidbody2D>();
        stateMachine = GetComponent<EnemyStateMachine>();
        enemyWandering = GetComponentInChildren<EnemyWandering>();
        enemyMovement = GetComponentInChildren<EnemyMovement>();
        enemyChasing = GetComponentInChildren<EnemyChasing>();
        enemyAttacking = GetComponentInChildren<EnemyAttacking>();
        enemyHealth = GetComponent<EnemyHealth>();

        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        if (enemyWandering == null)
            throw new System.Exception("Enemy requires an EnemyWandering component in children.");
        if (enemyMovement == null)
            throw new System.Exception("Enemy requires an EnemyMovement component in children.");
        if (enemyDetection == null)
            throw new System.Exception("Enemy requires an EnemyDetection component in children.");
        if (stateMachine == null)
            throw new System.Exception("Enemy requires an EnemyStateMachine component.");
        if (enemyChasing == null)
            throw new System.Exception("Enemy requires an EnemyChasing component in children.");
        if (enemyAttacking == null)
            throw new System.Exception("Enemy requires an EnemyAttacking component in children.");
        if (enemyHealth == null)
            throw new System.Exception("Enemy requires an EnemyHealth component.");

        enemyWandering.OnReachedWaypoint += HandleReachedDestination;
        enemyWandering.OnIdleComplete += HandleIdleComplete;
        stateMachine.OnStateChanged += HandleStateChanged;
        enemyDetection.OnSightStateChanged += OnSightStateChanged;

        enemyAttacking.OnPlayerEnteredRange += HandlePlayerEnteredAttackRange;
        enemyAttacking.OnPlayerExitedRange += HandlePlayerExitedAttackRange;
    }

    protected virtual void Start()
    {
        // optionally start in Idle/Wandering
    }

    protected virtual void Update()
    {
        // If already handled as dead, skip
        if (isDead) return;

        // Ensure we enter Dead state when health reaches zero
        if (enemyHealth != null && enemyHealth.IsDead() && stateMachine.GetState() != EnemyStateMachine.EnemyState.Dead)
        {
            ForceState(EnemyStateMachine.EnemyState.Dead);
            return;
        }
    }

    // Called whenever the state machine actually changes state
    private void HandleStateChanged(EnemyStateMachine.EnemyState oldState, EnemyStateMachine.EnemyState newState)
    {
        //Debug.Log($"{name} state changed: {oldState} -> {newState}");
        ApplyBehaviorsForState(newState);

        // If we just entered Dead, schedule destruction with blink (only once) and disable producers
        if (newState == EnemyStateMachine.EnemyState.Dead && !isDead)
        {
            isDead = true;

            if (destroyCoroutine == null)
            {
                // total delay before destroy (seconds)
                float totalDelay = 2.5f;
                // last portion used for blinking (seconds)
                float blinkDuration = 0.8f;
                destroyCoroutine = StartCoroutine(DestroyWithBlink(totalDelay, blinkDuration));
            }

            // disable/ unsubscribe systems that may request state changes after death
            if (enemyDetection != null)
            {
                enemyDetection.OnSightStateChanged -= OnSightStateChanged;
                enemyDetection.enabled = false;
            }
            if (enemyAttacking != null)
            {
                enemyAttacking.OnPlayerEnteredRange -= HandlePlayerEnteredAttackRange;
                enemyAttacking.OnPlayerExitedRange -= HandlePlayerExitedAttackRange;
                enemyAttacking.enabled = false;
            }
            if (enemyChasing != null) enemyChasing.enabled = false;
            if (enemyWandering != null) enemyWandering.enabled = false;

            // Prevent further state-change callbacks from this component (optional)
            if (stateMachine != null)
                stateMachine.OnStateChanged -= HandleStateChanged;
        }
    }

    private void ApplyBehaviorsForState(EnemyStateMachine.EnemyState state)
    {
        // If already dead we shouldn't change behavior further
        if (isDead && state != EnemyStateMachine.EnemyState.Dead) return;

        // Always ensure movement component exists
        if (enemyMovement == null) return;

        switch (state)
        {
            case EnemyStateMachine.EnemyState.Wandering:
                // Allow movement, enable wandering and disable chasing
                EnableMovement();
                if (enemyChasing != null)
                {
                    enemyChasing.StopBehavior();
                    enemyChasing.enabled = false;
                }
                if (enemyWandering != null)
                {
                    enemyWandering.enabled = true;
                    enemyWandering.StartBehavior();
                }
                break;

            case EnemyStateMachine.EnemyState.Chasing:
                // Allow movement, enable chasing and disable wandering
                EnableMovement();
                if (enemyWandering != null)
                {
                    enemyWandering.StopBehavior();
                    enemyWandering.enabled = false;
                }
                if (enemyChasing != null)
                {
                    enemyChasing.enabled = true;
                    enemyChasing.StartBehavior();
                }
                break;

            case EnemyStateMachine.EnemyState.BasicAttack:
                // Stop movement and disable movement workers while attack plays
                if (enemyChasing != null)
                {
                    enemyChasing.StopBehavior();
                    enemyChasing.enabled = false;
                }
                if (enemyWandering != null)
                {
                    enemyWandering.StopBehavior();
                    enemyWandering.enabled = false;
                }

                enemyMovement.Stop();
                enemyMovement.enabled = false;

                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }
                break;

            case EnemyStateMachine.EnemyState.Dead:
                // disable behaviors + movement
                if (enemyChasing != null)
                    enemyChasing.enabled = false;
                if (enemyWandering != null)
                    enemyWandering.enabled = false;
                enemyMovement.Stop();
                enemyMovement.enabled = false;
                var cap = GetComponent<CapsuleCollider2D>();
                if (cap != null) CapsuleCollider2D.Destroy(cap);

                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }
                break;

            case EnemyStateMachine.EnemyState.Idle:
            default:
                // idle: stop movement but keep movement component disabled until workers start it
                if (enemyChasing != null)
                {
                    enemyChasing.StopBehavior();
                }
                if (enemyWandering != null)
                {
                    enemyWandering.StartBehavior();
                }
                enemyMovement.Stop();
                break;
        }
    }

    private void EnableMovement()
    {
        if (enemyMovement != null)
        {
            enemyMovement.enabled = true;
            // movement will be started by the worker (chasing/wandering) as appropriate
        }
    }

    private void HandleReachedDestination()
    {
        if (isDead) return;
        stateMachine.ChangeState(EnemyStateMachine.EnemyState.Idle);
    }

    private void HandleIdleComplete()
    {
        if (isDead) return;
        stateMachine.ChangeState(EnemyStateMachine.EnemyState.Wandering);
        if (enemyWandering != null)
            enemyWandering.StartBehavior();
    }

    private void HandlePlayerEnteredAttackRange()
    {
        if (isDead) return;
        // Request attack state (if already attacking this will be a no-op or queued)
        SetState(EnemyStateMachine.EnemyState.BasicAttack);
    }

    private void HandlePlayerExitedAttackRange()
    {
        if (isDead) return;
        // queue up the state for after attack finishes
        if (enemyDetection != null && enemyDetection.hasLineOfSight)
            SetState(EnemyStateMachine.EnemyState.Chasing);
        else
            SetState(EnemyStateMachine.EnemyState.Wandering);
    }

    protected void SetState(EnemyStateMachine.EnemyState newState)
    {
        if (isDead) return; // ignore requests after death
        // Request state change; behavior will be applied when the state actually changes (HandleStateChanged)
        stateMachine.ChangeState(newState);
    }

    public void ForceState(EnemyStateMachine.EnemyState state)
    {
        // Force the state machine to change immediately (bypass BasicAttack queue)
        if (stateMachine != null)
            stateMachine.ForceChangeState(state);
        // Behavior will be applied inside HandleStateChanged when OnStateChanged fires
    }

    private void OnSightStateChanged(EnemyDetection.SightState sightState)
    {
        if (isDead) return;
        // Map detection states to high-level enemy states
        switch (sightState)
        {
            case EnemyDetection.SightState.Player:
                SetState(EnemyStateMachine.EnemyState.Chasing);
                break;

            case EnemyDetection.SightState.Obstacle:
                SetState(EnemyStateMachine.EnemyState.Wandering);
                break;
            case EnemyDetection.SightState.None:
            case EnemyDetection.SightState.OutOfRange:
                SetState(EnemyStateMachine.EnemyState.Wandering);
                break;
            default:
                SetState(EnemyStateMachine.EnemyState.Wandering);
                break;
        }
    }

    private void StartWandering()
    {
        if (isDead) return;
        SetState(EnemyStateMachine.EnemyState.Wandering);
    }

    private IEnumerator DestroyWithBlink(float delay, float blinkDuration)
    {
        float preWait = Mathf.Max(0f, delay - blinkDuration);
        if (preWait > 0f)
            yield return new WaitForSeconds(preWait);

        // collect renderers to toggle
        var renderers = GetComponentsInChildren<SpriteRenderer>();
        float blinkInterval = 0.05f;
        float elapsed = 0f;
        bool visible = true;

        while (elapsed < blinkDuration)
        {
            // toggle visibility
            visible = !visible;
            foreach (var r in renderers)
            {
                if (r != null) r.enabled = visible;
            }

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        // ensure renderers visible before destroy (optional)
        foreach (var r in renderers)
        {
            if (r != null) r.enabled = true;
        }

        // Unsubscribe events to avoid callbacks while destroying
        if (enemyWandering != null)
        {
            enemyWandering.OnReachedWaypoint -= HandleReachedDestination;
            enemyWandering.OnIdleComplete -= HandleIdleComplete;
        }
        if (stateMachine != null)
            stateMachine.OnStateChanged -= HandleStateChanged;
        if (enemyDetection != null)
            enemyDetection.OnSightStateChanged -= OnSightStateChanged;
        if (enemyAttacking != null)
        {
            enemyAttacking.OnPlayerEnteredRange -= HandlePlayerEnteredAttackRange;
            enemyAttacking.OnPlayerExitedRange -= HandlePlayerExitedAttackRange;
        }

        Destroy(gameObject);
    }

    protected virtual void OnDestroy()
    {
        if (enemyDetection != null)
            enemyDetection.OnSightStateChanged -= OnSightStateChanged;
    }
}