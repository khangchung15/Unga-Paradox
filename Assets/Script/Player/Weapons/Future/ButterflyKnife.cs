using System.Collections;
using UnityEngine;

public class ButterflyKnife : MonoBehaviour, IWeapon
{
    [SerializeField] private WeaponInfo weaponInfo;
    
    [Header("Animated Knife - Left Click")]
    [SerializeField] private GameObject animatedKnifePrefab;
    [SerializeField] private string overheadSlashAnimationName = "OverheadSlash";
    [SerializeField] private float minSpawnOffsetY = 2f;
    [SerializeField] private float maxSpawnOffsetY = 4f;
    [SerializeField] private float flyUpSpeed = 8f;
    [SerializeField] private float hoverDuration = 0.5f;
    
    [Header("Multi-Knife Throw - Right Click")]
    [SerializeField] private int multiKnifeCount = 7;
    [SerializeField] private float multiKnifeMinX = -2f;
    [SerializeField] private float multiKnifeMaxX = 2f;
    [SerializeField] private float multiKnifeMinY = 1f;
    [SerializeField] private float multiKnifeMaxY = 5f;

    [Header("Throw Settings")]
    [SerializeField] private float throwSpeed = 10f;
    [SerializeField] private float throwRange = 5f;
    [SerializeField] private int bossThrowDamage = 20;
    [SerializeField] private int playerBackfireDamage = 15;
    
    [Header("References")]
    [SerializeField] private string bossTag = "Boss";
    [SerializeField] private AudioClip attackSound;
    
    private AudioSource audioSource;
    private Transform bossTransform;
    private Transform playerTransform;
    private BossShield bossShield;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }
    
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        GameObject bossObject = GameObject.FindGameObjectWithTag(bossTag);
        if (bossObject != null)
        {
            bossTransform = bossObject.transform;
            bossShield = bossObject.GetComponentInChildren<BossShield>();
        }
    }
    
    public WeaponInfo GetWeaponInfo()
    {
        return weaponInfo;
    }
    
    public void Attack()
    {
        PerformLeftClickAttack();
    }
    
    public void SecondaryAttack()
    {
        PerformRightClickAttack();
    }
    
    private void PerformLeftClickAttack()
    {
        if (animatedKnifePrefab == null)
        {
            Debug.LogWarning("[ButterflyKnife] Animated Knife Prefab not assigned!");
            return;
        }
        
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("[ButterflyKnife] Player not found!");
                return;
            }
        }
        
        if (bossTransform == null)
        {
            GameObject bossObject = GameObject.FindGameObjectWithTag(bossTag);
            if (bossObject != null)
            {
                bossTransform = bossObject.transform;
                bossShield = bossObject.GetComponentInChildren<BossShield>();
            }
        }
        
        float randomYOffset = Random.Range(minSpawnOffsetY, maxSpawnOffsetY);
        Vector3 targetPosition = playerTransform.position + Vector3.up * randomYOffset;
        
        GameObject knifeInstance = Instantiate(animatedKnifePrefab, playerTransform.position, Quaternion.identity);
        
        AnimatedKnife animatedKnifeScript = knifeInstance.GetComponent<AnimatedKnife>();
        if (animatedKnifeScript != null)
        {
            animatedKnifeScript.Initialize(
                playerTransform,
                bossTransform,
                bossShield,
                targetPosition,
                flyUpSpeed,
                hoverDuration,
                throwRange,
                throwSpeed,
                bossThrowDamage,
                playerBackfireDamage,
                overheadSlashAnimationName
            );
        }
        
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }
    
    private void PerformRightClickAttack()
    {
        if (animatedKnifePrefab == null)
        {
            Debug.LogWarning("[ButterflyKnife] Animated Knife Prefab not assigned!");
            return;
        }
        
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("[ButterflyKnife] Player not found!");
                return;
            }
        }
        
        if (bossTransform == null)
        {
            GameObject bossObject = GameObject.FindGameObjectWithTag(bossTag);
            if (bossObject != null)
            {
                bossTransform = bossObject.transform;
                bossShield = bossObject.GetComponentInChildren<BossShield>();
            }
        }
        
        for (int i = 0; i < multiKnifeCount; i++)
        {
            float randomX = Random.Range(multiKnifeMinX, multiKnifeMaxX);
            float randomY = Random.Range(multiKnifeMinY, multiKnifeMaxY);
            Vector3 randomOffset = new Vector3(randomX, randomY, 0f);
            Vector3 targetPosition = playerTransform.position + randomOffset;
            
            GameObject knifeInstance = Instantiate(animatedKnifePrefab, playerTransform.position, Quaternion.identity);
            
            AnimatedKnife animatedKnifeScript = knifeInstance.GetComponent<AnimatedKnife>();
            if (animatedKnifeScript != null)
            {
                animatedKnifeScript.Initialize(
                    playerTransform,
                    bossTransform,
                    bossShield,
                    targetPosition,
                    flyUpSpeed,
                    hoverDuration,
                    throwRange,
                    throwSpeed,
                    bossThrowDamage,
                    playerBackfireDamage,
                    overheadSlashAnimationName
                );
            }
        }
        
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }

}
