using System.Collections.Generic;
using UnityEngine;

public class WeaponsManager : MonoBehaviour
{
    [Header("Weapons")]
    public List<WeaponInfo> weapons;
    public SerializableDictionary<string, WeaponInfo> weaponDictionary;

    private void Awake()
    {
        weaponDictionary = new SerializableDictionary<string, WeaponInfo>();
        foreach (WeaponInfo weapon in weapons)
        {
            weaponDictionary.Add(weapon.id, weapon);
        }
    }
}
