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
    [SerializeField] private float leapWindup = 0.15f;
    [SerializeField] private float leapLandingLag = 0.15f;
    [SerializeField] private float airborneHitboxTime = 0.20f; 

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

    private bool busy;
    private float cdTimer;

    private void Awake()
    {
        fsm = GetComponent<EnemyStateMachine>();
        anim = GetComponent<EnemyAnimation>();
        detection = GetComponentInChildren<EnemyDetection>();
        rb = GetComponent<Rigidbody2D>();
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;
    }

    private void Update()
    {
        if (!player || !detection || !fsm) return;

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
                StartCoroutine(DoShoot()); // fallback to shoot
        }
        else // nextAttack == Shoot
        {
            if (d > maxLeapDistance && d <= maxShootDistance)
                StartCoroutine(DoShoot());
            else if (d >= minLeapDistance && d <= maxLeapDistance)
                StartCoroutine(DoLeap()); // fallback to leap
        }
    }

    private IEnumerator DoLeap()
    {
        busy = true;
        fsm.ChangeState(EnemyStateMachine.EnemyState.BasicAttack); 
        if (anim != null) anim.RestartAttackAnimation();         

        // windup
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(leapWindup);

        // launch towards player
        if (player != null)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rb.AddForce(dir * leapForce, ForceMode2D.Impulse);
        }

        if (anim != null && airborneHitboxTime > 0f)
            anim.EnableAttackHitboxForSeconds(airborneHitboxTime);

        yield return new WaitForSeconds(leapLandingLag);

        // back to chasing
        fsm.ChangeState(EnemyStateMachine.EnemyState.Chasing);
        fsm.ApplyPendingState();

        nextAttack = AttackType.Shoot;
        cdTimer = globalCooldown;
        busy = false;
    }

    private IEnumerator DoShoot()
    {
        busy = true;
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
