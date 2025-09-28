using UnityEngine;
using UnityEngine.InputSystem; // new Input System

public class InventorySwitcher : MonoBehaviour
{
    private GameObject[] inventorySlots;
    private int activeSlot = 0;

    void Start()
    {
        inventorySlots = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            inventorySlots[i] = transform.GetChild(i).gameObject;

        // Hide all Active highlights first
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            Transform activeChild = inventorySlots[i].transform.Find("Active");
            if (activeChild != null)
                activeChild.gameObject.SetActive(false);
        }

        UpdateActiveSlot(activeSlot);
    }

    void Update()
    {
        var keyboard = Keyboard.current;

        if (keyboard.digit1Key.wasPressedThisFrame) SwitchSlot(0);
        if (keyboard.digit2Key.wasPressedThisFrame) SwitchSlot(1);
        if (keyboard.digit3Key.wasPressedThisFrame) SwitchSlot(2);
        if (keyboard.digit4Key.wasPressedThisFrame) SwitchSlot(3);
        if (keyboard.digit5Key.wasPressedThisFrame) SwitchSlot(4);
    }

    void SwitchSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySlots.Length) return;
        activeSlot = slotIndex;
        UpdateActiveSlot(activeSlot);
    }

    void UpdateActiveSlot(int slotIndex)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            Transform activeChild = inventorySlots[i].transform.Find("Active");
            if (activeChild != null)
                activeChild.gameObject.SetActive(i == slotIndex);
        }
    }
}
