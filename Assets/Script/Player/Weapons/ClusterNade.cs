using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClusterNade : MonoBehaviour, IWeapon
{
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    [SerializeField] private WeaponInfo weaponInfo;

    [Header("Attack Settings")]
    [Tooltip("Sound when throwing the grenade")]
    public AudioClip attackSound;
    [Tooltip("Sound when grenade splits")]
    public AudioClip clusterExplodeSound;
    [Tooltip("Throw distance from player")]
    public float attackDistance = 1.0f;

    [Header("Cluster Settings")]
    [Tooltip("Prefab of the smaller grenades spawned on explosion")]
    public GameObject clusterProjectile;
    [Tooltip("Speed of the thrown grenade")]
    public float throwVelocity = 7.0f;
    [Tooltip("Speed of smaller cluster grenades")]
    public float clusterVelocity = 5.0f;
    [Tooltip("Number of smaller grenades to spawn")]
    public int clusterCount = 5;
    [Tooltip("Time before the grenade explodes into clusters")]
    public float splitDelay = 1.5f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        MouseFollowWithOffset();
    }

    public WeaponInfo GetWeaponInfo()
    {
        return weaponInfo;
    }

    private void MouseFollowWithOffset()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 playerScreenPoint = Camera.main.WorldToScreenPoint(PlayerController.Instance.transform.position);
        Vector2 direction = mousePos - playerScreenPoint;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (mousePos.x < playerScreenPoint.x)
            ActiveWeapon.Instance.transform.rotation = Quaternion.Euler(180, 0, -angle);
        else
            ActiveWeapon.Instance.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void Attack()
    {
        spriteRenderer.enabled = false;

        // Get mouse position in world
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // Direction from player to mouse
        Vector2 direction = (mouseWorldPos - PlayerController.Instance.transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Vector3 spawnPos = PlayerController.Instance.transform.position + (Vector3)direction * attackDistance;

        // Instantiate a copy of this prefab as the thrown grenade
        GameObject thrownNade = Instantiate(gameObject, spawnPos, Quaternion.Euler(0, 0, angle));

        // Give it velocity
        Rigidbody2D rb = thrownNade.GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * throwVelocity;

        // Disable weapon behavior on spawned grenade
        ClusterNade weaponScript = thrownNade.GetComponent<ClusterNade>();
        weaponScript.enabled = false;

        // Add projectile fuse behavior
        ClusterNadeProjectile clusterScript = thrownNade.AddComponent<ClusterNadeProjectile>();
        clusterScript.clusterPartPrefab = clusterProjectile;
        clusterScript.clusterCount = clusterCount;
        clusterScript.clusterVelocity = clusterVelocity;
        clusterScript.fuseTime = splitDelay;
        clusterScript.explodeSound = clusterExplodeSound;

        // Play throw sound
        if (audioSource && attackSound)
            audioSource.PlayOneShot(attackSound);
    }
}
