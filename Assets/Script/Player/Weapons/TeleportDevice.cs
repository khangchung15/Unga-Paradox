using UnityEngine;
using UnityEngine.InputSystem;

public class TeleportDevice : MonoBehaviour, IWeapon
{
    [SerializeField] private WeaponInfo weaponInfo;

    [Header("Teleport Settings")]
    [Tooltip("Maximum teleport distance")]
    public float maxTeleportDistance = 10f;
    [Tooltip("Teleport sound effect")]
    public AudioClip teleportSound;
    [Tooltip("Teleport particle effect prefab (optional)")]
    public GameObject teleportEffectPrefab;
    [Tooltip("Layers that block teleportation")]
    public LayerMask obstacleLayers;

    private AudioSource audioSource;
    private Transform playerTransform;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        GetPlayerTransform();
    }

    private void Update()
    {
        if (playerTransform != null)
        {
            MouseFollowWithOffset();
        }
    }

    public WeaponInfo GetWeaponInfo()
    {
        return weaponInfo;
    }

    private void GetPlayerTransform()
    {
        if (PlayerController.Instance != null)
        {
            playerTransform = PlayerController.Instance.transform;
        }
        else if (ScientistController.Instance != null)
        {
            playerTransform = ScientistController.Instance.transform;
        }
        else
        {
            Debug.LogError("TeleportDevice: No player controller found!");
        }
    }

    private void MouseFollowWithOffset()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 playerScreenPoint = Camera.main.WorldToScreenPoint(playerTransform.position);
        Vector2 direction = mousePos - playerScreenPoint;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        if (mousePos.x < playerScreenPoint.x)
        {
            ActiveWeapon.Instance.transform.rotation = Quaternion.Euler(180, 0, -angle);
        }
        else
        {
            ActiveWeapon.Instance.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public void Attack()
    {
        if (playerTransform == null)
        {
            GetPlayerTransform();
            if (playerTransform == null) return;
        }

        Vector2 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        Vector3 playerPos = playerTransform.position;
        Vector3 targetPos = mouseWorldPos;
        
        float distance = Vector3.Distance(playerPos, targetPos);
        if (distance > maxTeleportDistance)
        {
            Vector3 direction = (targetPos - playerPos).normalized;
            targetPos = playerPos + direction * maxTeleportDistance;
        }

        RaycastHit2D hit = Physics2D.Linecast(playerPos, targetPos, obstacleLayers);
        
        if (hit.collider != null)
        {
            Vector3 safePosition = hit.point - (Vector2)((targetPos - playerPos).normalized * 0.5f);
            targetPos = safePosition;
        }

        if (teleportEffectPrefab != null)
        {
            Instantiate(teleportEffectPrefab, playerPos, Quaternion.identity);
        }

        playerTransform.position = targetPos;

        if (teleportEffectPrefab != null)
        {
            Instantiate(teleportEffectPrefab, targetPos, Quaternion.identity);
        }

        if (audioSource != null && teleportSound != null)
        {
            audioSource.PlayOneShot(teleportSound);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform player = null;
        if (PlayerController.Instance != null)
            player = PlayerController.Instance.transform;
        else if (ScientistController.Instance != null)
            player = ScientistController.Instance.transform;

        if (player == null) return;
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(player.position, maxTeleportDistance);
    }
}
