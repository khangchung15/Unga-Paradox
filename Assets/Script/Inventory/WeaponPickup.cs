using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon Info")]
    public WeaponInfo weaponInfo;  

    [Header("Pickup Settings")]
    public float pickupRange = 2f;
    public KeyCode pickupKey = KeyCode.E;
    public GameObject pickupPromptPrefab;

    [HideInInspector] public GameObject promptInstance;
    [HideInInspector] public bool playerInRange = false;

    private Transform player;
    private ActiveInventory playerInventory;

    private void Start()
    {
        
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerInventory = FindObjectOfType<ActiveInventory>();

        if (playerInventory != null)
            Debug.Log("[WeaponPickup] Found ActiveInventory: " + playerInventory.name);
        else
            Debug.LogWarning("[WeaponPickup] Could not find ActiveInventory in scene!");

        
        if (pickupPromptPrefab != null)
        {
            promptInstance = Instantiate(
                pickupPromptPrefab,
                transform.position + Vector3.up * 1.2f,
                Quaternion.identity
            );
            promptInstance.transform.SetParent(transform);
            promptInstance.SetActive(false); 
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            if (promptInstance != null)
                promptInstance.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptInstance != null)
                promptInstance.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(pickupKey))
        {
            PickupWeapon();
        }
    }

    private void PickupWeapon()
    {
        if (weaponInfo == null)
        {
            Debug.LogWarning("WeaponPickup: missing WeaponInfo!");
            return;
        }

        
        if (ActiveWeapon.Instance.CurrentActiveWeapon != null)
        {
            MonoBehaviour currentWeapon = ActiveWeapon.Instance.CurrentActiveWeapon;
            IWeapon currentWeaponInterface = currentWeapon as IWeapon;

            if (currentWeaponInterface != null)
            {
                WeaponInfo oldWeaponInfo = currentWeaponInterface.GetWeaponInfo();

                if (oldWeaponInfo != null)
                {
                    
                    GameObject dropped = Instantiate(gameObject);
                    dropped.transform.position = player.position + player.right * 0.7f;

                    
                    WeaponPickup pickupScript = dropped.GetComponent<WeaponPickup>();
                    pickupScript.weaponInfo = oldWeaponInfo;
                    pickupScript.playerInRange = false;

                    if (pickupScript.promptInstance != null)
                        pickupScript.promptInstance.SetActive(false);

                    
                    SpriteRenderer sr = dropped.GetComponent<SpriteRenderer>();
                    if (sr != null && oldWeaponInfo.weaponSprite != null)
                        sr.sprite = oldWeaponInfo.weaponSprite;

                   
                    Rigidbody2D rb = dropped.GetComponent<Rigidbody2D>();
                    if (rb != null)
                        rb.AddForce(player.right * 2f, ForceMode2D.Impulse);

                    Debug.Log($"Dropped old weapon: {oldWeaponInfo.weaponName}");
                }
            }

            
            Destroy(currentWeapon.gameObject);
        }

        
        if (playerInventory != null)
            playerInventory.ReplaceActiveSlot(weaponInfo);

        
        GameObject newWeapon = Instantiate(
            weaponInfo.weaponPrefab,
            ActiveWeapon.Instance.transform.position,
            Quaternion.identity
        );

        newWeapon.transform.SetParent(ActiveWeapon.Instance.transform);
        ActiveWeapon.Instance.NewWeapon(newWeapon.GetComponent<MonoBehaviour>());

        
        Destroy(gameObject);
    }
}
