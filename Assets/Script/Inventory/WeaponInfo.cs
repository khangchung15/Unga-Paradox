using UnityEngine;

[CreateAssetMenu(menuName = "New Weapon")]
public class WeaponInfo : ScriptableObject
{
    [Header("Weapon ID Settings")]
    public string id;

    [ContextMenu("Generate ID")]
    private void GenerateGuid()
    {
        id = System.Guid.NewGuid().ToString();
    }

    [Header("Weapon Settings")]
    public string weaponName;          
    public GameObject weaponPrefab;    
    public float weaponCooldown;

    [Header("UI Settings")]
    public Sprite weaponSprite;

    [Header("World Pickup")]
    public GameObject pickupPrefab;
    
    [Header("Audio Settings")]
    public AudioClip equipSound;
}
