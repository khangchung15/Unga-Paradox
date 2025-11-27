using System.Collections;
using UnityEngine;

public class BossShooter : MonoBehaviour, IBoss
{
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] public float projectileMoveSpeed;
    [SerializeField] private float startingDistance = 0.1f;
    [SerializeField] private Transform target;                        

    [Header("Burst Shape")]
    [SerializeField] public int projectilesPerBurst;
    [Tooltip("Reminder: The first and last projectiles will fire at the same position if this is 359.")]
    [SerializeField][Range(0,359)] public float angleSpread;
    [Tooltip("Shoot projectiles one at a time instead of all at once.")]
    [SerializeField] public bool stagger;
    [SerializeField] public bool oscillate;
    [SerializeField] public int burstCount;
    
    [Header("Timing")]
    [SerializeField] public float timeBetweenBursts;
    [SerializeField] public float shootCooldown = 1f;
    
    [Header("Cleanup")]
    [SerializeField] private Transform projectilesParent;
    [SerializeField] private string projectileTag = "EnemyProjectile"; 

    private bool isShooting = false;
    private bool _cease = false;           
    private Coroutine _shootRoutine;     

    private void Awake()
    {
        if (target == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
    }
    private void OnValidate()
    {
        oscillate = !stagger ? false : oscillate;

        if(angleSpread == 0 && !stagger) { projectilesPerBurst = 1; }
        
        // Property minimums
        projectilesPerBurst = projectilesPerBurst < 0 ? 0 : projectilesPerBurst;
        burstCount = burstCount < 0 ? 0 : burstCount;
        timeBetweenBursts = timeBetweenBursts < 0.1f ? 0.1f : timeBetweenBursts;
        shootCooldown = shootCooldown < 0.1f ? 0.1f : shootCooldown;
        startingDistance = startingDistance < 0.1f ? 0.1f : startingDistance;
        projectileMoveSpeed = projectileMoveSpeed < 0.1f ? 0.1f : projectileMoveSpeed;
    }

    public void Attack()
    {
        if (!isShooting)
        {
            StartCoroutine(ShootRoutine());
        }
    }

    private IEnumerator ShootRoutine()
    {
        isShooting = true;

        float angleStep, startAngle, currentAngle, endAngle;
        float timeBetweenProjectiles = 0f;

        TargetConeOfInfluence(out angleStep, out startAngle, out currentAngle, out endAngle);

        if (stagger)
        {
            timeBetweenProjectiles = timeBetweenBursts / projectilesPerBurst;
        }

        for (int i = 0; i < burstCount; i++)
        {
            if (!oscillate)
            {
                // Recalculate cone of influence in case player moves
                TargetConeOfInfluence(out angleStep, out startAngle, out currentAngle, out endAngle);
            }

            if(oscillate && (i % 2 == 0))
            {
                // Recalculate on every other burst
                TargetConeOfInfluence(out angleStep, out startAngle, out currentAngle, out endAngle);
            }
            else if(oscillate)
            {
                currentAngle = endAngle;
                endAngle = startAngle;
                startAngle = currentAngle;
                angleStep *= -1;
            }

            // This represents the individual projectiles in a burst
            for (int j = 0; j < projectilesPerBurst; j++)
            {
                Vector2 projectilePos = FindProjectileSpawnPos(currentAngle);

                GameObject newProjectile = Instantiate(projectilePrefab, projectilePos, Quaternion.identity);
                newProjectile.transform.right = newProjectile.transform.position - transform.position;

                if (newProjectile.TryGetComponent(out Projectile projectile))
                {
                    projectile.SetProjectileMoveSpeed(projectileMoveSpeed);
                }

                currentAngle += angleStep;

                if (stagger)
                {
                    yield return new WaitForSeconds(timeBetweenProjectiles);
                }
            }

            currentAngle = startAngle;

            if (!stagger)
            {
                yield return new WaitForSeconds(timeBetweenBursts);
            }
        }

        yield return new WaitForSeconds(shootCooldown);
        isShooting = false;
    }


    private void TargetConeOfInfluence(out float angleStep, out float startAngle, out float currentAngle, out float endAngle)
    {
        Vector2 targetDirection = target.position - transform.position;

        float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;

        angleStep = 0f;
        float halfAngleSpread = 0f;

        startAngle = targetAngle;
        endAngle = targetAngle;
        currentAngle = targetAngle;

        if (angleSpread != 0)
        {
            angleStep = angleSpread / (projectilesPerBurst - 1);
            halfAngleSpread = angleSpread / 2f;

            startAngle = targetAngle - halfAngleSpread;
            endAngle = targetAngle + halfAngleSpread;
            currentAngle = startAngle;
        }
    }

    private Vector2 FindProjectileSpawnPos(float currentAngle)
    {
        float x = transform.position.x + startingDistance * Mathf.Cos(currentAngle * Mathf.Deg2Rad);
        float y = transform.position.y + startingDistance * Mathf.Sin(currentAngle * Mathf.Deg2Rad);

        Vector2 pos = new Vector2(x, y);
        return pos;
    }
}