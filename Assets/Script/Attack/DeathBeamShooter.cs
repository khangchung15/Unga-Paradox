using UnityEngine;

public class DeathBeamShooter : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private string targetTag = "Player";
    
    [Header("Shooting Settings")]
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float shootDuration = 0.5f;
    [SerializeField] private float beamRange = 20f;
    [SerializeField] private float damage = 10f;
    
    [Header("Beam Visual")]
    [SerializeField] private GameObject beamObject;
    
    [Header("Audio")]
    [SerializeField] private AudioClip shootSound;
    
    private AudioSource audioSource;
    private float shootTimer;
    private bool isShooting;
    private float shootingTimer;
    private RotateToTarget rotateToTarget;
    private Animator beamAnimator;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        rotateToTarget = GetComponent<RotateToTarget>();
        
        if (rotateToTarget == null)
        {
            Debug.LogError("RotateToTarget component not found on " + gameObject.name);
        }
        
        if (beamObject != null)
        {
            beamAnimator = beamObject.GetComponent<Animator>();
            if (beamAnimator == null)
            {
                Debug.LogWarning("Animator component not found on beam object: " + beamObject.name);
            }
        }
    }
    
    private void Start()
    {
        if (target == null)
        {
            GameObject targetObject = GameObject.FindGameObjectWithTag(targetTag);
            if (targetObject != null)
            {
                target = targetObject.transform;
                
                if (rotateToTarget != null)
                {
                    rotateToTarget.target = target;
                }
            }
            else
            {
                Debug.LogWarning("Target with tag '" + targetTag + "' not found");
            }
        }
        
        shootTimer = shootInterval;
        
        if (beamObject != null)
        {
            beamObject.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (target == null) return;
        
        if (!isShooting)
        {
            shootTimer -= Time.deltaTime;
            
            if (shootTimer <= 0f)
            {
                StartShooting();
            }
        }
        else
        {
            shootingTimer -= Time.deltaTime;
            
            if (shootingTimer <= 0f)
            {
                StopShooting();
            }
            else
            {
                PerformBeamDamage();
            }
        }
    }
    
    private void StartShooting()
    {
        isShooting = true;
        shootingTimer = shootDuration;
        
        if (beamObject != null)
        {
            beamObject.SetActive(true);
            
            if (beamAnimator != null)
            {
                beamAnimator.Play("DeathBeam", 0, 0f);
            }
        }
        
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
    }
    
    private void StopShooting()
    {
        isShooting = false;
        shootTimer = shootInterval;
        
        if (beamObject != null)
        {
            beamObject.SetActive(false);
        }
    }
    
    private void PerformBeamDamage()
    {
        Vector2 direction = (target.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, beamRange);
        
        if (hit.collider != null)
        {
            Health playerHealth = hit.collider.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage * Time.deltaTime);
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = isShooting ? Color.red : Color.yellow;
            Gizmos.DrawLine(transform.position, target.position);
        }
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, beamRange);
    }
}
