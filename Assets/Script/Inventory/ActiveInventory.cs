using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveInventory : MonoBehaviour
{
    public static ActiveInventory Instance;

    [Header("Hotbar Slots (Auto-filled)")]
    public List<InventorySlot> slots = new List<InventorySlot>();

    public int activeSlotIndexNum = 0;
    private PlayerControls playerControls;

    private void Awake()
    {
        Instance = this;
        playerControls = new PlayerControls();

        
        slots.Clear();
        slots.AddRange(GetComponentsInChildren<InventorySlot>());
    }

    private void Start()
    {
        Instance = this;

        slots.Clear();
        slots.AddRange(GetComponentsInChildren<InventorySlot>());

        
        foreach (InventorySlot s in slots)
            s.isHotbarSlot = true;

        playerControls.Inventory.Keyboard.performed += ctx => ToggleActiveSlot((int)ctx.ReadValue<float>());
        ToggleActiveHighlight(0);
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

  

    private void ToggleActiveSlot(int numValue)
    {
        ToggleActiveHighlight(numValue - 1);
    }

    private void ToggleActiveHighlight(int indexNum)
    {
        activeSlotIndexNum = indexNum;

       
        foreach (Transform inventorySlot in transform)
        {
            inventorySlot.GetChild(0).gameObject.SetActive(false);
        }

        
        transform.GetChild(indexNum).GetChild(0).gameObject.SetActive(true);

        ChangeActiveWeapon();
    }

 

    private void ChangeActiveWeapon()
    {

        ActiveWeapon.Instance.ClearWeapon();

        InventorySlot slot = GetCurrentSlot();

        if (slot == null || slot.weaponInfo == null)
        {
            ActiveWeapon.Instance.WeaponNull();
            return;
        }

        GameObject weaponToSpawn = slot.weaponInfo.weaponPrefab;

        GameObject newWeapon = Instantiate(
            weaponToSpawn,
            ActiveWeapon.Instance.transform.position,
            Quaternion.identity);

        newWeapon.transform.parent = ActiveWeapon.Instance.transform;

        ActiveWeapon.Instance.NewWeapon(newWeapon.GetComponent<MonoBehaviour>());
    }

    
    public void ReplaceSlot(int slotIndex, WeaponInfo newWeapon)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count)
        {
            Debug.LogWarning($"ReplaceSlot INVALID index: {slotIndex}");
            return;
        }

        InventorySlot slot = slots[slotIndex];
        slot.SetWeaponInfo(newWeapon);

       
        if (slotIndex == activeSlotIndexNum)
        {
            ChangeActiveWeapon();
        }
    }

    public void ForceEquipSlot(int index)
    {
        activeSlotIndexNum = index;
        ToggleActiveHighlight(index);
        ChangeActiveWeapon();   
    }

    public void ReplaceActiveSlot(WeaponInfo newWeaponInfo)
    {
       
        ReplaceSlot(activeSlotIndexNum, newWeaponInfo);
    }

    public void ClearSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count)
            return;

        slots[slotIndex].SetWeaponInfo(null);

        
        if (slotIndex == activeSlotIndexNum)
        {
            ActiveWeapon.Instance.WeaponNull();
        }
    }

    
    public InventorySlot GetCurrentSlot()
    {
        if (activeSlotIndexNum < 0 || activeSlotIndexNum >= slots.Count)
            return null;

        return slots[activeSlotIndexNum];
    }
}
