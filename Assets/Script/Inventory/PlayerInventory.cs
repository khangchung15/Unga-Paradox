using UnityEngine;
using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameInventoryPanel;  
    public Transform itemContainer;        

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.I;

    private List<InventorySlot> backpackSlots = new List<InventorySlot>();
    private bool isOpen = false;

    private void Start()
    {
        
        foreach (Transform t in itemContainer)
        {
            InventorySlot slot = t.GetComponent<InventorySlot>();
            if (slot != null)
                backpackSlots.Add(slot);
        }

        
        if (gameInventoryPanel != null)
            gameInventoryPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            ToggleInventory();
    }

    private void ToggleInventory()
    {
        if (gameInventoryPanel == null) return;

        isOpen = !isOpen;
        gameInventoryPanel.SetActive(isOpen);
    }

    
    public bool AddItem(WeaponInfo info)
    {
        foreach (var slot in backpackSlots)
        {
            if (!slot.HasWeapon())
            {
                slot.SetWeaponInfo(info);
                return true;
            }
        }

        
        return false;
    }
}
