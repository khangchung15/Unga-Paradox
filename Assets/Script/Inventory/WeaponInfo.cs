using UnityEngine;

[CreateAssetMenu(menuName = "New Weapon")]
public class WeaponInfo : ScriptableObject
{
    [Header("Weapon Settings")]
    public string weaponName;          
    public GameObject weaponPrefab;    
    public float weaponCooldown;

    [Header("UI Settings")]
    public Sprite weaponSprite;        
}
