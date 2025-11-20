using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDataPersistence
{
    [SerializeField] private int slotIndex;
    [SerializeField] private WeaponInfo weaponInfo;
    private string weaponID;
    private WeaponsManager weapons;

    private void Awake()
    {
        weapons = FindAnyObjectByType<WeaponsManager>();
    }

    public void LoadData(GameData data)
    {
        data.weaponHotbar.TryGetValue(slotIndex, out weaponID);


        if (weaponID != weaponInfo.id && weaponID != null)
        {
            weapons.weaponDictionary.TryGetValue(weaponID, out weaponInfo);
            Transform item = transform.Find("Item");
            if (item != null)
            {
                item.GetComponent<Image>().sprite = weaponInfo.weaponSprite;
            }
        }
    }

    public void SaveData(ref GameData data)
    {
        if (data.weaponHotbar.ContainsKey(slotIndex))
        {
            data.weaponHotbar.Remove(slotIndex);
        }

        data.weaponHotbar.Add(slotIndex, weaponInfo.id);
    }

    public WeaponInfo GetWeaponInfo()
    {
        return weaponInfo;
    }

    public void SetWeaponInfo(WeaponInfo info)
    {
        weaponInfo = info;
    }
}
