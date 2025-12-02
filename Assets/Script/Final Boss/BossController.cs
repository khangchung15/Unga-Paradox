using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private BossShield bossShield;
    
    [Header("Death Beam Shooters")]
    [SerializeField] private DeathBeamShooter[] deathBeamShooters;
    
    [Header("Hover Settings")]
    [SerializeField] private float hoverYPosition = 5f;
    [SerializeField] private float hoverSpeed = 2f;
    [SerializeField] private float hoverRangeX = 3f;
    [SerializeField] private float hoverMoveSpeed = 1.5f;
    
    [Header("Fall Settings")]
    [SerializeField] private float fallGravityScale = 3f;
    [SerializeField] private string groundLayerName = "Ground";
    [SerializeField] private float delayBeforeStoppingAttackingBeam = 0.5f;
    
    [Header("Recovery Settings")]
    [SerializeField] private float flyUpSpeed = 3f;
    
    [Header("Stuck Detection")]
    [SerializeField] private float stuckCheckInterval = 0.5f;
    [SerializeField] private float stuckDistanceThreshold = 0.1f;
    [SerializeField] private string bossSpawnPointTag = "BossSpawnPoint";
    
    [Header("Animation Names")]
    [SerializeField] private string idleAnimationName = "Idle";
    [SerializeField] private string attackAnimationName = "Attack";
    [SerializeField] private string fallAnimationName = "Fall";
    [SerializeField] private string recoverAnimationName = "Recover";
    
    private BossState currentState = BossState.Hovering;
    private float hoverDirection = 1f;
    private Vector3 startPosition;
    private bool isOnGround = false;
    private int groundLayer;
    private DeathBeamShooter currentAttackingShooter;
    
    private Vector3 lastPositionCheck;
    private float stuckCheckTimer = 0f;
    private Transform playerTransform;
    private Transform bossSpawnPoint;
    
    public enum BossState
    {
        Hovering,
        Attacking,
        Falling,
        OnGround,
        Recovering,
        FlyingUp
    }
    
    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        if (animator == null)
            animator = GetComponent<Animator>();
        if (bossShield == null)
            bossShield = GetComponentInChildren<BossShield>();
        
        groundLayer = LayerMask.NameToLayer(groundLayerName);
        
        if (deathBeamShooters == null || deathBeamShooters.Length == 0)
        {
            deathBeamShooters = FindObjectsOfType<DeathBeamShooter>();
        }
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        GameObject spawnPoint = GameObject.FindGameObjectWithTag(bossSpawnPointTag);
        if (spawnPoint != null)
        {
            bossSpawnPoint = spawnPoint.transform;
        }
    }
    
    private void Start()
    {
        startPosition = transform.position;
        hoverYPosition = transform.position.y;
        lastPositionCheck = transform.position;
        
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;
        
        if (bossShield != null)
        {
            bossShield.OnShieldBroken += HandleShieldBroken;
            bossShield.OnShieldRegenerateComplete += HandleShieldRegenerated;
        }
        
        SetState(BossState.Hovering);
    }
    
    private void Update()
    {
        switch (currentState)
        {
            case BossState.Hovering:
                UpdateHovering();
                CheckIfStuck();
                break;
            case BossState.FlyingUp:
                UpdateFlyingUp();
                break;
        }
        
        FacePlayer();
    }
    
    private void UpdateHovering()
    {
        Vector3 targetPosition = transform.position;
        targetPosition.x += hoverDirection * hoverMoveSpeed * Time.deltaTime;
        
        if (Mathf.Abs(targetPosition.x - startPosition.x) > hoverRangeX)
        {
            hoverDirection *= -1f;
        }
        
        targetPosition.y = Mathf.Lerp(transform.position.y, hoverYPosition, hoverSpeed * Time.deltaTime);
        
        transform.position = targetPosition;
    }
    
    private void UpdateFlyingUp()
    {
        Vector3 targetPosition = transform.position;
        targetPosition.y = Mathf.MoveTowards(transform.position.y, hoverYPosition, flyUpSpeed * Time.deltaTime);
        transform.position = targetPosition;
        
        if (Mathf.Abs(transform.position.y - hoverYPosition) < 0.1f)
        {
            SetState(BossState.Hovering);
        }
    }
    
    private void FacePlayer()
    {
        if (playerTransform == null) return;
        
        float directionToPlayer = playerTransform.position.x - transform.position.x;
        
        if (directionToPlayer > 0.1f)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else if (directionToPlayer < -0.1f)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }
    
    private void CheckIfStuck()
    {
        stuckCheckTimer += Time.deltaTime;
        
        if (stuckCheckTimer >= stuckCheckInterval)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPositionCheck);
            
            if (distanceMoved < stuckDistanceThreshold)
            {
                TeleportToSpawnPoint();
            }
            
            lastPositionCheck = transform.position;
            stuckCheckTimer = 0f;
        }
    }
    
    private void TeleportToSpawnPoint()
    {
        if (bossSpawnPoint != null)
        {
            transform.position = bossSpawnPoint.position;
            startPosition = bossSpawnPoint.position;
            rb.linearVelocity = Vector2.zero;
        }
    }
    
    private void SetState(BossState newState)
    {
        currentState = newState;
        
        switch (newState)
        {
            case BossState.Hovering:
                animator.speed = 1f;
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0;
                rb.linearVelocity = Vector2.zero;
                animator.Play(idleAnimationName);
                EnableDeathBeamShooters(true);
                currentAttackingShooter = null;
                stuckCheckTimer = 0f;
                lastPositionCheck = transform.position;
                break;
                
            case BossState.Attacking:
                animator.Play(attackAnimationName);
                break;
                
            case BossState.Falling:
                animator.speed = 1f;
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = fallGravityScale;
                animator.Play(fallAnimationName);
                isOnGround = false;
                EnableDeathBeamShooters(false);
                StartCoroutine(StopDeathBeamsAfterDelay());
                break;
                
            case BossState.OnGround:
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
                StartCoroutine(ShowRecoverFirstFrame());
                break;
                
            case BossState.Recovering:
                animator.speed = 1f;
                animator.Play(recoverAnimationName);
                StartCoroutine(WaitForRecoveryAnimation());
                break;
                
            case BossState.FlyingUp:
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0;
                rb.linearVelocity = Vector2.zero;
                animator.Play(idleAnimationName);
                break;
        }
    }
    
    private IEnumerator ShowRecoverFirstFrame()
    {
        yield return null;
        
        if (currentState == BossState.OnGround)
        {
            animator.Play(recoverAnimationName, 0, 0f);
            yield return null;
            animator.speed = 0f;
        }
    }
    
    private void HandleShieldBroken(DeathBeamShooter shooter)
    {
        if (currentState == BossState.Hovering || currentState == BossState.Attacking)
        {
            currentAttackingShooter = shooter;
            SetState(BossState.Falling);
        }
    }
    
    private void HandleShieldRegenerated()
    {
        if (currentState == BossState.OnGround)
        {
            SetState(BossState.Recovering);
        }
    }
    
    private IEnumerator StopDeathBeamsAfterDelay()
    {
        ForceStopAllDeathBeamsExcept(currentAttackingShooter);
        
        yield return new WaitForSeconds(delayBeforeStoppingAttackingBeam);
        
        if (currentAttackingShooter != null)
        {
            currentAttackingShooter.ForceStop();
            currentAttackingShooter = null;
        }
    }
    
    private IEnumerator WaitForRecoveryAnimation()
    {
        AnimationClip recoverClip = GetAnimationClip(recoverAnimationName);
        if (recoverClip != null)
        {
            yield return new WaitForSeconds(recoverClip.length);
        }
        
        SetState(BossState.FlyingUp);
    }
    
    public void PlayAttackAnimation()
    {
        if (currentState == BossState.Hovering)
        {
            SetState(BossState.Attacking);
        }
    }
    
    public void ReturnToIdle()
    {
        if (currentState == BossState.Attacking)
        {
            SetState(BossState.Hovering);
        }
    }
    
    public bool CanAttack()
    {
        return currentState == BossState.Hovering || currentState == BossState.Attacking;
    }
    
    private void EnableDeathBeamShooters(bool enable)
    {
        if (deathBeamShooters == null) return;
        
        foreach (DeathBeamShooter shooter in deathBeamShooters)
        {
            if (shooter != null)
            {
                shooter.enabled = enable;
            }
        }
    }
    
    private void ForceStopAllDeathBeams()
    {
        if (deathBeamShooters == null) return;
        
        foreach (DeathBeamShooter shooter in deathBeamShooters)
        {
            if (shooter != null)
            {
                shooter.ForceStop();
            }
        }
    }
    
    private void ForceStopAllDeathBeamsExcept(DeathBeamShooter exceptShooter)
    {
        if (deathBeamShooters == null) return;
        
        foreach (DeathBeamShooter shooter in deathBeamShooters)
        {
            if (shooter != null && shooter != exceptShooter)
            {
                shooter.ForceStop();
            }
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == BossState.Falling && collision.gameObject.layer == groundLayer)
        {
            isOnGround = true;
            SetState(BossState.OnGround);
        }
    }
    
    private AnimationClip GetAnimationClip(string clipName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return null;
            
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip;
        }
        return null;
    }
    
    private void OnDestroy()
    {
        if (bossShield != null)
        {
            bossShield.OnShieldBroken -= HandleShieldBroken;
            bossShield.OnShieldRegenerateComplete -= HandleShieldRegenerated;
        }
    }
}
