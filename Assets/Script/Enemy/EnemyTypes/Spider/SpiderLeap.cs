using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyStateMachine))]
[RequireComponent(typeof(Rigidbody2D))]
public class SpiderCombatController : MonoBehaviour
{
    public enum AttackType { Leap, Shoot }

    [Header("Alternation")]
    [SerializeField] private AttackType nextAttack = AttackType.Leap; 
    [SerializeField] private float globalCooldown = 1.0f;

    [Header("Leap Settings")]
    [SerializeField] private float minLeapDistance = 1.5f;
    [SerializeField] private float maxLeapDistance = 5.0f;
    [SerializeField] private float leapForce = 12f;
    [SerializeField] private float leapTravelTime = 0.25f;
    [SerializeField] private float leapWindup = 0.15f;
    [SerializeField] private float leapLandingLag = 0.15f;
    [SerializeField] private float airborneHitboxTime = 0.20f; 
    [SerializeField] private float airborneHitboxDelay = 0.05f;
    [SerializeField] private Color leapWindupColor = Color.yellow;
    [SerializeField] private SpriteRenderer leapIndicatorSprite;
    [SerializeField] private Color indicatorIdleColor = Color.white;
    [SerializeField] private Color indicatorFlashColor = Color.red;
    [SerializeField] private float indicatorFlashInterval = 0.05f;
    [SerializeField] private SpriteRenderer bodySprite;
    [SerializeField] private Color defaultBodyColor = Color.white;
    [SerializeField] private Color leapFlashColor = Color.red;
    [SerializeField] private float leapFlashInterval = 0.05f;

    [Header("Ranged Settings")]
    [SerializeField] private float maxShootDistance = 10.0f;
    [SerializeField] private float shootWindup = 0.10f;
    [SerializeField] private Transform firePoint;         
    [SerializeField] private SpiderProjectile projectilePrefab; 

    private EnemyStateMachine fsm;
    private EnemyAnimation anim;
    private EnemyDetection detection;
    private Rigidbody2D rb;
    private Transform player;

    private EnemyHealth enemyHealth;
    private bool isDead;
    private Coroutine flashRoutine;

    private bool busy;
    private float cdTimer;

    private void Awake()
    {
        fsm = GetComponent<EnemyStateMachine>();
        anim = GetComponent<EnemyAnimation>();
        detection = GetComponentInChildren<EnemyDetection>();
        rb = GetComponent<Rigidbody2D>();
        enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth != null && enemyHealth.onDeath != null)
        {
            enemyHealth.onDeath.AddListener(OnDeath);
        }
        if (bodySprite != null)
        {
            defaultBodyColor = bodySprite.color;
        }
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;
    }

    private void OnDestroy()
    {
        if (enemyHealth != null && enemyHealth.onDeath != null)
        {
            enemyHealth.onDeath.RemoveListener(OnDeath);
        }
    }

    private void OnDeath()
    {
        isDead = true;
        busy = true;
        StopAllCoroutines();
        if (bodySprite != null)
        {
            bodySprite.color = defaultBodyColor;
        }
        flashRoutine = null;
    }

    private void Update()
    {
        if (!player || !detection || !fsm) return;
        if (isDead) return;

        if (cdTimer > 0f) cdTimer -= Time.deltaTime;
        if (busy || cdTimer > 0f) return;

        if (!detection.hasLineOfSight) return;

        float d = Vector2.Distance(transform.position, player.position);
        var state = fsm.GetState();
        bool canActFrom =
            state == EnemyStateMachine.EnemyState.Chasing ||
            state == EnemyStateMachine.EnemyState.Wandering ||
            state == EnemyStateMachine.EnemyState.Idle;

        if (!canActFrom) return;

        if (nextAttack == AttackType.Leap)
        {
            if (d >= minLeapDistance && d <= maxLeapDistance)
                StartCoroutine(DoLeap());
            else if (d <= maxShootDistance)
                StartCoroutine(DoShoot());
        }
        else 
        {
            if (d > maxLeapDistance && d <= maxShootDistance)
                StartCoroutine(DoShoot());
            else if (d >= minLeapDistance && d <= maxLeapDistance)
                StartCoroutine(DoLeap());
        }
    }

    private IEnumerator DoLeap()
    {
        busy = true;
        if (isDead) { busy = false; yield break; }
        fsm.ChangeState(EnemyStateMachine.EnemyState.BasicAttack); 
        if (anim != null) anim.RestartAttackAnimation();

        // visual telegraph: tint the spider during windup
        if (bodySprite != null)
        {
            bodySprite.color = leapWindupColor;
        }

        // windup
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(leapWindup);

        // snapshot the target direction at leap start so the spider commits to a jump
        Vector2 leapDir = Vector2.zero;
        if (player != null)
        {
            leapDir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        }

        rb.linearVelocity = leapDir * leapForce;

        if (airborneHitboxTime > 0f)
        {
            if (airborneHitboxDelay > 0f)
                yield return new WaitForSeconds(airborneHitboxDelay);

            if (!isDead)
            {
                if (anim != null)
                    anim.EnableAttackHitboxForSeconds(airborneHitboxTime);

                if (bodySprite != null)
                {
                    if (flashRoutine != null)
                        StopCoroutine(flashRoutine);
                    flashRoutine = StartCoroutine(FlashBodyForSeconds(airborneHitboxTime));
                }

                if (leapIndicatorSprite != null)
                    StartCoroutine(FlashIndicatorForSeconds(airborneHitboxTime));
            }
        }

        yield return new WaitForSeconds(leapTravelTime);
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(leapLandingLag);

        if (bodySprite != null && flashRoutine == null)
        {
            bodySprite.color = defaultBodyColor;
        }

        fsm.ChangeState(EnemyStateMachine.EnemyState.Chasing);
        fsm.ApplyPendingState();

        nextAttack = AttackType.Shoot;
        cdTimer = globalCooldown;
        busy = false;
    }

    private IEnumerator FlashIndicatorForSeconds(float duration)
    {
        if (leapIndicatorSprite == null)
            yield break;

        float elapsed = 0f;
        bool toggle = false;

        while (elapsed < duration)
        {
            leapIndicatorSprite.color = toggle ? indicatorFlashColor : indicatorIdleColor;
            toggle = !toggle;

            yield return new WaitForSeconds(indicatorFlashInterval);
            elapsed += indicatorFlashInterval;
        }

        // ensure we end on the idle color
        leapIndicatorSprite.color = indicatorIdleColor;
    }

    private IEnumerator FlashBodyForSeconds(float duration)
    {
        if (bodySprite == null)
            yield break;

        float elapsed = 0f;
        bool toggle = false;

        while (elapsed < duration)
        {
            bodySprite.color = toggle ? leapFlashColor : defaultBodyColor;
            toggle = !toggle;

            yield return new WaitForSeconds(leapFlashInterval);
            elapsed += leapFlashInterval;
        }

        bodySprite.color = defaultBodyColor;
        flashRoutine = null;
    }

    private IEnumerator DoShoot()
    {
        busy = true;
        if (isDead) { busy = false; yield break; }
        fsm.ChangeState(EnemyStateMachine.EnemyState.BasicAttack);
        if (anim != null) anim.RestartAttackAnimation(); 
        yield return new WaitForSeconds(shootWindup);

        if (firePoint != null && projectilePrefab != null && player != null)
        {
            Vector2 dir = (player.position - firePoint.position).normalized;
            var proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            proj.Launch(dir);
        }

        fsm.ChangeState(EnemyStateMachine.EnemyState.Chasing);
        fsm.ApplyPendingState();

        nextAttack = AttackType.Leap;
        cdTimer = globalCooldown;
        busy = false;
    }
}
