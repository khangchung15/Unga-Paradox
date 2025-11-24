using UnityEngine;

public class LocalActiveInventory : MonoBehaviour
{
    private int activeSlotIndexNum = 0;
    private PlayerControls playerControls;

    private void Awake() 
    {
        playerControls = new PlayerControls();
    }

    private void Start() 
    {
        playerControls.Inventory.Keyboard.performed += ctx => ToggleActiveSlot((int)ctx.ReadValue<float>());
        ToggleActiveHighlight(0);
    }

    private void OnEnable() 
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void ToggleActiveSlot(int numValue) 
    {
        ToggleActiveHighlight(numValue - 1);
    }

    private void ToggleActiveHighlight(int indexNum) 
    {
        activeSlotIndexNum = indexNum;

        foreach (Transform inventorySlot in this.transform)
        {
            inventorySlot.GetChild(0).gameObject.SetActive(false);
        }

        this.transform.GetChild(indexNum).GetChild(0).gameObject.SetActive(true);
        ChangeActiveWeapon();
    }

    private void ChangeActiveWeapon() 
    {
        if (LocalActiveWeapon.Instance == null)
        {
            Debug.LogWarning("LocalActiveWeapon.Instance is null. Make sure LocalActiveWeapon component is on ActiveWeaponSlot.");
            return;
        }
        
        if (LocalActiveWeapon.Instance.CurrentActiveWeapon != null)
        {
            Destroy(LocalActiveWeapon.Instance.CurrentActiveWeapon.gameObject);
        }

        InventorySlot inventorySlot = transform.GetChild(activeSlotIndexNum).GetComponentInChildren<InventorySlot>();
        
        if (inventorySlot == null)
        {
            LocalActiveWeapon.Instance.WeaponNull();
            return;
        }

        WeaponInfo weaponInfo = inventorySlot.GetWeaponInfo();
        
        if (weaponInfo == null)
        {
            LocalActiveWeapon.Instance.WeaponNull();
            return;
        }

        if (weaponInfo.weaponPrefab == null)
        {
            Debug.LogWarning($"Weapon prefab is null for {weaponInfo.weaponName}");
            LocalActiveWeapon.Instance.WeaponNull();
            return;
        }

        GameObject newWeapon = Instantiate(weaponInfo.weaponPrefab, LocalActiveWeapon.Instance.transform.position, Quaternion.identity);
        newWeapon.transform.parent = LocalActiveWeapon.Instance.transform;
        LocalActiveWeapon.Instance.NewWeapon(newWeapon.GetComponent<MonoBehaviour>());
    }
}
