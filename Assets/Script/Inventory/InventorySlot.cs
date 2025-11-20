using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [Header("Weapon Data")]
    [SerializeField] public WeaponInfo weaponInfo;   
    [SerializeField] public Image itemImage;         
    public bool isHotbarSlot = false;


    private void Awake()
    {
        if (itemImage == null)
            itemImage = GetComponent<Image>();
    }

    private void Start()
    {
        RefreshSlot();
    }

    
    public WeaponInfo GetWeaponInfo()
    {
        return weaponInfo;
    }

    public void SetWeaponInfo(WeaponInfo newInfo)
    {
        weaponInfo = newInfo;
        RefreshSlot();
    }

    public bool HasWeapon()
    {
        return weaponInfo != null;
    }

    
    public void SetWeapon(WeaponInfo newInfo)
    {
        SetWeaponInfo(newInfo);
    }

    public bool HasItem => HasWeapon();

    
    private void RefreshSlot()
    {
        if (itemImage == null)
            return;

        if (weaponInfo == null)
        {
            itemImage.enabled = false;
            itemImage.sprite = null;
        }
        else
        {
            itemImage.enabled = true;
            itemImage.sprite = weaponInfo.weaponSprite;
        }
    }
}
