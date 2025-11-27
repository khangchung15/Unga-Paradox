using UnityEngine;

public class PotionWeapon : MonoBehaviour, IWeapon
{
    [SerializeField] private WeaponInfo weaponInfo;
    
    [Header("Potion Prefabs")]
    [SerializeField] private PotionData[] potionTypes;
    
    [Header("Display")]
    [SerializeField] private SpriteRenderer potionSpriteRenderer;
    
    [Header("Throw Settings")]
    [SerializeField] private float throwForceTowardsBoss = 15f;
    [SerializeField] private float throwForceUpward = 10f;
    [SerializeField] private float spawnOffsetDistance = 1f;
    
    [Header("References")]
    [SerializeField] private string bossTag = "Boss";
    [SerializeField] private AudioClip throwSound;
    
    private Transform playerTransform;
    private Transform bossTransform;
    private AudioSource audioSource;
    private PotionData currentPotion;
    
    [System.Serializable]
    public class PotionData
    {
        public string potionName;
        public Sprite potionSprite;
        public GameObject potionPrefab;
    }
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        if (potionSpriteRenderer == null)
        {
            potionSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
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
        }
        
        SelectRandomPotion();
    }
    
    private void OnEnable()
    {
        SelectRandomPotion();
    }
    
    private void SelectRandomPotion()
    {
        if (potionTypes == null || potionTypes.Length == 0)
        {
            return;
        }
        
        currentPotion = potionTypes[Random.Range(0, potionTypes.Length)];
        
        if (potionSpriteRenderer != null && currentPotion.potionSprite != null)
        {
            potionSpriteRenderer.sprite = currentPotion.potionSprite;
            potionSpriteRenderer.sortingLayerName = "Ground";
            potionSpriteRenderer.sortingOrder = 7;
        }
        
        Debug.Log($"[PotionWeapon] Selected potion: {currentPotion.potionName}");
    }
    
    public WeaponInfo GetWeaponInfo()
    {
        return weaponInfo;
    }
    
    public void Attack()
    {
        ThrowAtBoss();
    }
    
    public void SecondaryAttack()
    {
        ThrowUpward();
    }
    
    private void ThrowAtBoss()
    {
        if (currentPotion == null || currentPotion.potionPrefab == null)
        {
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
                return;
            }
        }
        
        if (bossTransform == null)
        {
            GameObject bossObject = GameObject.FindGameObjectWithTag(bossTag);
            if (bossObject != null)
            {
                bossTransform = bossObject.transform;
            }
        }
        
        Vector2 directionToBoss = bossTransform != null ? 
            (bossTransform.position - playerTransform.position).normalized : 
            Vector2.right;
        
        Vector3 spawnPosition = playerTransform.position + (Vector3)directionToBoss * spawnOffsetDistance;
        GameObject potion = Instantiate(currentPotion.potionPrefab, spawnPosition, Quaternion.identity);
        
        Rigidbody2D potionRb = potion.GetComponent<Rigidbody2D>();
        if (potionRb != null)
        {
            potionRb.AddForce(directionToBoss * throwForceTowardsBoss, ForceMode2D.Impulse);
            Debug.Log($"[PotionWeapon] Threw {currentPotion.potionName} at boss with force {throwForceTowardsBoss}");
        }
        
        if (audioSource != null && throwSound != null)
        {
            audioSource.PlayOneShot(throwSound);
        }
        
        SelectRandomPotion();
    }
    
    private void ThrowUpward()
    {
        if (currentPotion == null || currentPotion.potionPrefab == null)
        {
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
                return;
            }
        }
        
        Vector3 spawnPosition = playerTransform.position + Vector3.up * spawnOffsetDistance;
        GameObject potion = Instantiate(currentPotion.potionPrefab, spawnPosition, Quaternion.identity);
        
        Rigidbody2D potionRb = potion.GetComponent<Rigidbody2D>();
        if (potionRb != null)
        {
            potionRb.AddForce(Vector2.up * throwForceUpward, ForceMode2D.Impulse);
            Debug.Log($"[PotionWeapon] Threw {currentPotion.potionName} upward with force {throwForceUpward}");
        }
        
        if (audioSource != null && throwSound != null)
        {
            audioSource.PlayOneShot(throwSound);
        }
        
        SelectRandomPotion();
    }
}
