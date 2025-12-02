using UnityEngine;

public class LocalInventorySlot : MonoBehaviour
{
    [SerializeField] private WeaponInfo weaponInfo;

    public WeaponInfo GetWeaponInfo()
    {
        return weaponInfo;
    }

    public void SetWeaponInfo(WeaponInfo info)
    {
        weaponInfo = info;
    }
}