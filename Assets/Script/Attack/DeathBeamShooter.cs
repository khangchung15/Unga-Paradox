using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class DeathBeamShooter : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private string targetTag = "Player";
    
    [Header("Shooting Settings")]
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float shootDuration = 0.5f;
    
    [Header("Beam Visual")]
    [SerializeField] private GameObject beamObject;
    
    [Header("Audio")]
    [SerializeField] private AudioClip shootSound;
    
    [Header("Attack Indicator")]
    [SerializeField] private AttackIndicatorManager attackIndicatorManager;
    [SerializeField] private float chargeTime = 1.5f;
    
    [Header("Rotation Lock")]
    [SerializeField] private float rotationLockBeforeShoot = 0.5f;

    [Header("Boss Reference")]
    [SerializeField] private BossController bossController;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    [Header("Camera Shake")]
    [SerializeField] private CinemachineImpulseSource impulseSource;

    
    private AudioSource audioSource;
    private float shootTimer;
    private bool isShooting;
    private float shootingTimer;
    private DeathBeamRotateToTarget rotateToTarget;
    private Animator beamAnimator;
    
    private float chargeTimer;
    private bool isCharging;
    private AttackIndicator currentIndicator;
    private bool isRotationLocked = false;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
        
        rotateToTarget = GetComponent<DeathBeamRotateToTarget>();
        
        if (beamObject != null)
        {
            beamAnimator = beamObject.GetComponent<Animator>();
        }
        
        if (attackIndicatorManager == null)
        {
            attackIndicatorManager = FindObjectOfType<AttackIndicatorManager>();
        }
        
        if (impulseSource == null)
        {
            impulseSource = GetComponent<CinemachineImpulseSource>();
        }
    }

    
    private void Start()
    {
        if (bossController == null)
        {
            bossController = FindObjectOfType<BossController>();
        }

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
        }
        
        shootTimer = shootInterval;
        
        if (beamObject != null)
        {
            beamObject.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy) return;
        
        if (bossController != null && !bossController.CanAttack())
        {
            if (isCharging || isShooting)
            {
                CancelCharging();
                StopShooting();
            }
            return;
        }
        
        if (!isCharging && !isShooting)
        {
            shootTimer -= Time.deltaTime;
            
            if (shootTimer <= 0f)
            {
                StartCharging();
            }
        }
        else if (isCharging)
        {
            if (currentIndicator == null)
            {
                CancelCharging();
                return;
            }
            
            chargeTimer += Time.deltaTime;
            float chargeProgress = chargeTimer / chargeTime;
            
            float lockTime = chargeTime - rotationLockBeforeShoot;
            if (!isRotationLocked && chargeTimer >= lockTime)
            {
                LockRotation();
            }
            
            if (currentIndicator != null)
            {
                currentIndicator.SetFillAmount(chargeProgress);
            }
            
            if (chargeTimer >= chargeTime)
            {
                FinishCharging();
            }
        }
        else if (isShooting)
        {
            shootingTimer -= Time.deltaTime;
            
            if (shootingTimer <= 0f)
            {
                StopShooting();
            }
        }
    }
    
    private void StartCharging()
    {
        if (attackIndicatorManager != null)
        {
            currentIndicator = attackIndicatorManager.RequestIndicator(this);
            
            if (currentIndicator == null)
            {
                shootTimer = 0.5f;
                return;
            }
        }
        
        isCharging = true;
        chargeTimer = 0f;
        
        if (bossController != null)
        {
            bossController.PlayAttackAnimation();
        }
        
    }

    
    private void CancelCharging()
    {
        if (!isCharging) return;
        
        isCharging = false;
        chargeTimer = 0f;
        shootTimer = shootInterval;
        
        UnlockRotation();
        
        if (attackIndicatorManager != null && currentIndicator != null)
        {
            attackIndicatorManager.ReleaseIndicator(this);
            currentIndicator = null;
        }
    }

    
    private void FinishCharging()
    {
        isCharging = false;
        
        if (attackIndicatorManager != null && currentIndicator != null)
        {
            attackIndicatorManager.ReleaseIndicator(this);
            currentIndicator = null;
        }
        
        StartShooting();
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
        
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse();
        }
        
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
    }

    
    private void StopShooting()
    {
        if (!isShooting) return;
        
        isShooting = false;
        shootTimer = shootInterval;
        
        if (beamObject != null)
        {
            beamObject.SetActive(false);
        }
        
        UnlockRotation();
        
        if (bossController != null)
        {
            bossController.ReturnToIdle();
        }
    }

    
    private void LockRotation()
    {
        if (rotateToTarget != null && !isRotationLocked)
        {
            rotateToTarget.LockRotation();
            isRotationLocked = true;
            
        }
    }
    
    private void UnlockRotation()
    {
        if (rotateToTarget != null && isRotationLocked)
        {
            rotateToTarget.UnlockRotation();
            isRotationLocked = false;
        }
    }
    
    private void OnDestroy()
    {
        if (attackIndicatorManager != null && currentIndicator != null)
        {
            attackIndicatorManager.ReleaseIndicator(this);
        }
    }
    
    private void OnDrawGizmos()
    {
        if (target != null && isShooting)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
    public void ForceStop()
    {
        if (isCharging)
        {
            CancelCharging();
        }
        
        if (isShooting)
        {
            isShooting = false;
            shootTimer = shootInterval;
            
            if (beamObject != null)
            {
                beamObject.SetActive(false);
            }
            
            UnlockRotation();
        }
    }

    public bool IsShooting()
    {
        return isShooting;
    }

}
