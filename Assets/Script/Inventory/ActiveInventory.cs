using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveInventory : MonoBehaviour
{
    [Header("Please choose between 0-4")]
    [SerializeField] private int activeSlotIndexNum = 0;

    private PlayerControls playerControls;

    private void Awake() {
        playerControls = new PlayerControls();
    }

    private void Start() {
        playerControls.Inventory.Keyboard.performed += ctx => ToggleActiveSlot((int)ctx.ReadValue<float>());

        StartCoroutine(WaitForLoad());
    }

    private IEnumerator WaitForLoad() 
    {
        yield return new WaitForSeconds(0.001f);
        ToggleActiveHighlight(activeSlotIndexNum);
    }

    private void OnEnable() {
        playerControls.Enable();
    }

    private void ToggleActiveSlot(int numValue) {
        ToggleActiveHighlight(numValue - 1);
    }

    public void ToggleActiveHighlight(int indexNum) {
        activeSlotIndexNum = indexNum;

        foreach (Transform inventorySlot in this.transform)
        {
            inventorySlot.GetChild(0).gameObject.SetActive(false);
        }

        this.transform.GetChild(indexNum).GetChild(0).gameObject.SetActive(true);

        ChangeActiveWeapon();
    }

    private void ChangeActiveWeapon() {
        if (ActiveWeapon.Instance.CurrentActiveWeapon != null)
        {
            Destroy(ActiveWeapon.Instance.CurrentActiveWeapon.gameObject);
        }

        if (!transform.GetChild(activeSlotIndexNum).GetComponentInChildren<InventorySlot>())
        {
            ActiveWeapon.Instance.WeaponNull();
            return;
        }

        GameObject weaponToSpawn = transform.GetChild(activeSlotIndexNum).GetComponentInChildren<InventorySlot>().GetWeaponInfo().weaponPrefab;

        GameObject newWeapon = Instantiate(weaponToSpawn, ActiveWeapon.Instance.transform.position, Quaternion.identity);

        newWeapon.transform.parent = ActiveWeapon.Instance.transform;

        ActiveWeapon.Instance.NewWeapon(newWeapon.GetComponent<MonoBehaviour>());
    }
    public void ReplaceActiveSlot(WeaponInfo newWeaponInfo)
    {
        if (newWeaponInfo == null) return;

        
        if (activeSlotIndexNum < 0 || activeSlotIndexNum >= transform.childCount)
        {
            Debug.LogWarning($"Invalid slot index {activeSlotIndexNum}");
            return;
        }

        
        Transform activeSlot = transform.GetChild(activeSlotIndexNum);
        Debug.Log($"[Inventory] Replacing weapon in slot: {activeSlot.name}");

        
        InventorySlot slot = activeSlot.GetComponent<InventorySlot>();
        if (slot == null)
        {
            Transform item = activeSlot.Find("Item");
            if (item != null)
            {
                slot = item.GetComponent<InventorySlot>();
            }
        }

        if (slot == null)
        {
            Debug.LogWarning($"[Inventory] No InventorySlot found in {activeSlot.name} or its children.");
            return;
        }

        
        slot.SetWeaponInfo(newWeaponInfo);

        
        Transform itemTransform = activeSlot.Find("Item");
        if (itemTransform != null)
        {
            var icon = itemTransform.GetComponent<UnityEngine.UI.Image>();
            if (icon != null)
            {
                if (newWeaponInfo.weaponSprite != null)
                {
                    icon.sprite = newWeaponInfo.weaponSprite;
                    icon.enabled = true;
                    Debug.Log($"[Inventory] Updated icon for {newWeaponInfo.weaponName}");
                }
                else
                {
                    Debug.LogWarning($"[Inventory] No weapon sprite found for {newWeaponInfo.weaponName}");
                }
            }
            else
            {
                Debug.LogWarning($"[Inventory] Item object in {activeSlot.name} has no Image component.");
            }
        }

        Debug.Log($"[Inventory] Slot {activeSlotIndexNum} now holds {newWeaponInfo.weaponName}");
    }
}
