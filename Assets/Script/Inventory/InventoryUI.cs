using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public List<InventorySlot> allSlots = new List<InventorySlot>();

    private InventorySlot selectedSlot = null;
    private Image draggedIcon;
    private Canvas parentCanvas;

    private void Start()
    {
        parentCanvas = GetComponentInParent<Canvas>();

        
        allSlots.Clear();
        allSlots.AddRange(GetComponentsInChildren<InventorySlot>());
        allSlots.AddRange(ActiveInventory.Instance.slots);

        Debug.Log("Inventory Slots found: " + allSlots.Count);

        
        GameObject dragIconObj = new GameObject("DraggedIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        dragIconObj.transform.SetParent(parentCanvas.transform);
        draggedIcon = dragIconObj.GetComponent<Image>();
        draggedIcon.raycastTarget = false;
        draggedIcon.enabled = false;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TrySelectSlot();

        if (selectedSlot != null)
            DragItemIcon();

        if (Input.GetMouseButtonUp(0))
            TryDropItem();
    }

    void TrySelectSlot()
    {
        InventorySlot hovered = GetSlotUnderMouse();
        if (hovered == null || !hovered.HasItem)
            return;

        selectedSlot = hovered;

        draggedIcon.enabled = true;
        draggedIcon.sprite = selectedSlot.weaponInfo.weaponSprite;
    }

    void DragItemIcon()
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            Input.mousePosition,
            parentCanvas.worldCamera,
            out pos);

        draggedIcon.rectTransform.anchoredPosition = pos;
    }

    void TryDropItem()
    {
        if (selectedSlot == null)
        {
            draggedIcon.enabled = false;
            return;
        }

        InventorySlot hovered = GetSlotUnderMouse();

        
        if (hovered != null && hovered != selectedSlot)
        {
            WeaponInfo temp = hovered.weaponInfo;
            hovered.SetWeapon(selectedSlot.weaponInfo);
            selectedSlot.SetWeapon(temp);

            
            int activeIndex = ActiveInventory.Instance.activeSlotIndexNum;
            InventorySlot activeSlot = ActiveInventory.Instance.slots[activeIndex];

            if (hovered == activeSlot || selectedSlot == activeSlot)
            {
                ActiveInventory.Instance.ForceEquipSlot(activeIndex);
            }
        }
        else
        {
            
            if (selectedSlot.HasItem)
                DropItemToWorld(selectedSlot.weaponInfo);

            selectedSlot.SetWeapon(null);

            
            int activeIndex = ActiveInventory.Instance.activeSlotIndexNum;
            InventorySlot activeSlot = ActiveInventory.Instance.slots[activeIndex];

            if (selectedSlot == activeSlot)
            {
                ActiveInventory.Instance.ForceEquipSlot(activeIndex);
            }
        }

        
        selectedSlot = null;
        draggedIcon.enabled = false;
    }

    void DropItemToWorld(WeaponInfo weapon)
    {
        if (weapon.pickupPrefab == null)
        {
            Debug.LogWarning("No pickup prefab assigned for " + weapon.weaponName);
            return;
        }

        Vector3 mouseScreen = Input.mousePosition;

        
        mouseScreen.z = Mathf.Abs(Camera.main.transform.position.z);

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mouseScreen);
        worldPos.z = 0;

        Instantiate(weapon.pickupPrefab, worldPos, Quaternion.identity);

        Debug.Log($"Dropped {weapon.weaponName} at {worldPos}");
    }

    void FixHotbarConsistency(InventorySlot a, InventorySlot b)
    {
        
        if (a.isHotbarSlot && !a.HasItem)
        {
            if (ActiveInventory.Instance.activeSlotIndexNum == a.transform.GetSiblingIndex())
            {
                
                ActiveWeapon.Instance.WeaponNull();
            }

            ActiveInventory.Instance.slots[a.transform.GetSiblingIndex()].weaponInfo = null;
        }

        
        if (b.isHotbarSlot)
        {
            ActiveInventory.Instance.slots[b.transform.GetSiblingIndex()].weaponInfo = b.weaponInfo;
        }
    }

    InventorySlot GetSlotUnderMouse()
    {
        Vector2 mousePos = Input.mousePosition;

        foreach (InventorySlot slot in allSlots)
        {
            if (slot.itemImage == null) continue;

            RectTransform rt = slot.itemImage.rectTransform;

            if (RectTransformUtility.RectangleContainsScreenPoint(rt, mousePos))
                return slot;
        }

        return null;
    }
}
