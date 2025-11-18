using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon Info")]
    public WeaponInfo weaponInfo;  

    [Header("Pickup Settings")]
    public float pickupRange = 2f;
    public KeyCode pickupKey = KeyCode.E;
    public GameObject pickupPromptPrefab;

    private GameObject promptInstance;
    private bool playerInRange = false;

    private Transform player;
    private ActiveInventory playerInventory;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerInventory = FindObjectOfType<ActiveInventory>();

        if (playerInventory == null)
            Debug.LogWarning("[WeaponPickup] Could not find ActiveInventory!");

        
        if (pickupPromptPrefab != null)
        {
            promptInstance = Instantiate(
                pickupPromptPrefab,
                transform.position + Vector3.up * 1.2f,
                Quaternion.identity,
                transform
            );
            promptInstance.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(pickupKey))
            PickupWeapon();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerInRange = true;
            if (promptInstance != null)
                promptInstance.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptInstance != null)
                promptInstance.SetActive(false);
        }
    }

    private void PickupWeapon()
    {
        if (weaponInfo == null)
        {
            Debug.LogWarning("[WeaponPickup] Cannot pick up weapon — weaponInfo missing!");
            return;
        }

        
        if (ActiveWeapon.Instance.CurrentActiveWeapon != null)
        {
            MonoBehaviour oldWeaponMB = ActiveWeapon.Instance.CurrentActiveWeapon;
            IWeapon oldWepInterface = oldWeaponMB as IWeapon;

            if (oldWepInterface != null)
            {
                WeaponInfo oldWeaponInfo = oldWepInterface.GetWeaponInfo();

                if (oldWeaponInfo != null && oldWeaponInfo.pickupPrefab != null)
                {
                    
                    GameObject dropped = Instantiate(oldWeaponInfo.pickupPrefab);
                    dropped.transform.position = player.position + (Vector3)(player.right * 0.7f);

                    
                    Rigidbody2D rb = dropped.GetComponent<Rigidbody2D>();
                    if (rb != null)
                        rb.AddForce(player.right * 2f, ForceMode2D.Impulse);

                    Debug.Log($"[WeaponPickup] Dropped old weapon: {oldWeaponInfo.weaponName}");
                }
            }


            ActiveWeapon.Instance.ClearWeapon();
        }


        if (playerInventory != null)
            playerInventory.ReplaceActiveSlot(weaponInfo);

        
        GameObject newWeaponObj = Instantiate(
            weaponInfo.weaponPrefab,
            ActiveWeapon.Instance.transform.position,
            Quaternion.identity
        );

        newWeaponObj.transform.SetParent(ActiveWeapon.Instance.transform);
        ActiveWeapon.Instance.NewWeapon(newWeaponObj.GetComponent<MonoBehaviour>());

      
        Destroy(gameObject);
    }
}
