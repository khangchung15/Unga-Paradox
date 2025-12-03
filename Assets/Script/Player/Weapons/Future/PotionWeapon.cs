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
        
        GameObject potion = Instantiate(currentPotion.potionPrefab, playerTransform.position, Quaternion.identity);
        
        Rigidbody2D potionRb = potion.GetComponent<Rigidbody2D>();
        ThrownPotion thrownPotion = potion.GetComponent<ThrownPotion>();
        if (potionRb != null)
        {
            potionRb.AddForce(directionToBoss * throwForceTowardsBoss, ForceMode2D.Impulse);
        }
        
        if (audioSource != null && throwSound != null)
        {
            audioSource.PlayOneShot(throwSound);
        }
        if (potionRb != null && thrownPotion != null)
        {
            Vector2 initialVelocity = directionToBoss * (throwForceTowardsBoss * 0.3f);
            potionRb.linearVelocity = initialVelocity;
            thrownPotion.SetThrowDirection(directionToBoss);
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
        
        GameObject potion = Instantiate(currentPotion.potionPrefab, playerTransform.position, Quaternion.identity);
        
        Rigidbody2D potionRb = potion.GetComponent<Rigidbody2D>();
        ThrownPotion thrownPotion = potion.GetComponent<ThrownPotion>();
        if (potionRb != null)
        {
            potionRb.AddForce(Vector2.up * throwForceUpward, ForceMode2D.Impulse);
        }
        
        if (audioSource != null && throwSound != null)
        {
            audioSource.PlayOneShot(throwSound);
        }
        if (potionRb != null && thrownPotion != null)
        {
            Vector2 initialVelocity = Vector2.up * (throwForceUpward * 0.3f);
            potionRb.linearVelocity = initialVelocity;
            thrownPotion.SetThrowDirection(Vector2.up);
        }
        
        SelectRandomPotion();
    }
}
