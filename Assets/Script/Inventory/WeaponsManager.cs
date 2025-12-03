using System.Collections.Generic;
using UnityEngine;

public class WeaponsManager : MonoBehaviour
{
    [Header("Weapons")]
    public List<WeaponInfo> weapons;
    public SerializableDictionary<string, WeaponInfo> weaponDictionary;
    public static WeaponsManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        if (GameObject.Find("Scientist") == null)
            DontDestroyOnLoad(this.gameObject);

        weaponDictionary = new SerializableDictionary<string, WeaponInfo>();
        foreach (WeaponInfo weapon in weapons)
        {
            weaponDictionary.Add(weapon.id, weapon);
        }
    }
}
