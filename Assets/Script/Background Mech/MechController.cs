using UnityEngine;
using Unity.Cinemachine;


public class MechController : MonoBehaviour
{
    public CinemachineImpulseSource impulseSource;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float moveDistance = 10f;
    
    [Header("Attack Settings")]
    public float minAttackDelay = 3f;
    public float maxAttackDelay = 8f;
    
    [Header("References")]
    public Animator animator;
    public Transform laserPivotPoint;
    public GameObject laserObject;
    public Vector3 laserOffset;
    
    [Header("Audio Settings")]
    public AudioClip laserSound;          // Sound to play when laser starts
    public AudioSource audioSource;       // AudioSource component reference
    public float laserVolume = 1f;        // Volume for laser sound (0-1)
    
    private Vector3 startPosition;
    private float targetX;
    private bool movingRight = true;
    private float nextAttackTime;
    private bool isAttacking = false;
    private bool laserActive = false;
    
    void Start()
    {
        if (impulseSource == null)
        {
            impulseSource = GetComponent<CinemachineImpulseSource>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Get or add AudioSource if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Configure AudioSource
        audioSource.playOnAwake = false;
        audioSource.volume = laserVolume;
        
        startPosition = transform.position;
        targetX = startPosition.x + moveDistance;
        SetNextAttackTime();
        
        if (laserObject != null)
        {
            laserObject.SetActive(false);
        }
    }
    
    void Update()
    {
        // Check if it's time to attack
        if (!isAttacking && Time.time >= nextAttackTime)
        {
            StartAttack();
        }
        
        // Only move if not attacking
        if (!isAttacking)
        {
            MoveMech();
            UpdateMovementAnimation();
        }
        
        // Update laser position if active
        if (laserActive && laserObject != null && laserObject.activeInHierarchy)
        {
            UpdateLaserPosition();
        }
    }
    
    void MoveMech()
    {
        if (movingRight)
        {
            transform.position += Vector3.right * moveSpeed * Time.deltaTime;
            
            if (transform.position.x >= targetX)
            {
                movingRight = false;
                targetX = startPosition.x - moveDistance;
                Flip();
            }
        }
        else
        {
            transform.position += Vector3.left * moveSpeed * Time.deltaTime;
            
            if (transform.position.x <= targetX)
            {
                movingRight = true;
                targetX = startPosition.x + moveDistance;
                Flip();
            }
        }
    }
    
    void UpdateMovementAnimation()
    {
        animator.SetBool("isRunning", true);
        animator.SetBool("isIdle", false);
        animator.SetBool("isAttack", false);
    }
    
    void StartAttack()
    {
        isAttacking = true;
        laserActive = false;
        
        animator.SetBool("isRunning", false);
        animator.SetBool("isIdle", false);
        animator.SetBool("isAttack", true);
        
        if (laserObject != null)
        {
            laserObject.SetActive(false);
        }
    }
    
    // ANIMATION EVENT - Call this from the Attack animation at the exact frame you want laser
    public void OnLaserStart()
    {
        laserActive = true;
        
        if (laserObject != null)
        {
            laserObject.SetActive(true);
            UpdateLaserPosition();
            
            Animator laserAnimator = laserObject.GetComponent<Animator>();
            if (laserAnimator != null)
            {
                laserAnimator.Play("LaserAttack", -1, 0f);
                if (impulseSource != null)
                {
                    impulseSource.GenerateImpulse();
                }
            }
        }
        

        // Play laser sound
        PlayLaserSound();
    }
    
    void PlayLaserSound()
    {
        if (laserSound != null && audioSource != null)
        {
            audioSource.volume = laserVolume;
            audioSource.PlayOneShot(laserSound);
        }
        else
        {
            Debug.LogWarning("Laser sound or AudioSource not assigned!");
        }
    }
    
    // ANIMATION EVENT - Call this at the end of the Attack animation
    public void OnAttackEnd()
    {
        isAttacking = false;
        laserActive = false;
        animator.SetBool("isAttack", false);
        animator.SetBool("isIdle", false);
        animator.SetBool("isRunning", true);
        
        if (laserObject != null)
        {
            laserObject.SetActive(false);
        }
        
        SetNextAttackTime();
    }
    
    void UpdateLaserPosition()
    {
        if (laserPivotPoint != null && laserObject != null)
        {
            laserObject.transform.position = laserPivotPoint.position + laserOffset;
            laserObject.transform.rotation = Quaternion.identity;
            
            Vector3 laserScale = laserObject.transform.localScale;
            laserScale.x = Mathf.Abs(laserScale.x) * (movingRight ? 1 : -1);
            laserObject.transform.localScale = laserScale;
        }
    }
    
    void SetNextAttackTime()
    {
        nextAttackTime = Time.time + Random.Range(minAttackDelay, maxAttackDelay);
    }
    
    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    
    void OnDisable()
    {
        if (laserObject != null)
        {
            laserObject.SetActive(false);
        }
    }
    
    // Public methods to control audio settings at runtime
    public void SetLaserVolume(float volume)
    {
        laserVolume = Mathf.Clamp01(volume);
        if (audioSource != null)
        {
            audioSource.volume = laserVolume;
        }
    }
    
    public void SetLaserSound(AudioClip newLaserSound)
    {
        laserSound = newLaserSound;
    }
}