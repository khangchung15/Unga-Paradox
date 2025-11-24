using UnityEngine;

public class TeleportSwap : MonoBehaviour, IWeapon
{
    [SerializeField] private WeaponInfo weaponInfo;
    
    [Header("Teleport Settings")]
    [SerializeField] private string bossTag = "Boss";
    [SerializeField] private GameObject teleportEffectPrefab;
    [SerializeField] private AudioClip teleportSound;
    
    private AudioSource audioSource;
    private Transform playerTransform;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }
    
    public WeaponInfo GetWeaponInfo()
    {
        return weaponInfo;
    }
    
    public void Attack()
    {
        GameObject boss = GameObject.FindGameObjectWithTag(bossTag);
        
        if (boss == null)
        {
            Debug.LogWarning("Boss not found! Make sure the Boss GameObject has the 'Boss' tag.");
            return;
        }
        
        if (playerTransform == null)
        {
            Debug.LogWarning("Player transform not found!");
            return;
        }
        
        SwapPositions(playerTransform, boss.transform);
        
        if (teleportSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(teleportSound);
        }
    }
    
    private void SwapPositions(Transform player, Transform boss)
    {
        Vector3 playerPos = player.position;
        Vector3 bossPos = boss.position;
        
        if (teleportEffectPrefab != null)
        {
            Instantiate(teleportEffectPrefab, playerPos, Quaternion.identity);
            Instantiate(teleportEffectPrefab, bossPos, Quaternion.identity);
        }
        
        player.position = bossPos;
        boss.position = playerPos;
        
        Debug.Log("Teleported! Player and Boss swapped positions.");
    }
}
