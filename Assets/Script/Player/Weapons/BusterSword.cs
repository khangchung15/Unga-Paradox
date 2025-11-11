using System.Collections;
using UnityEngine;

public class BusterSword : MonoBehaviour, IWeapon
{
    [SerializeField] private WeaponInfo weaponInfo;

    [Header("Attack Settings")]
    [Tooltip("Attack effect (Also holds damage and visuals)")]
    public GameObject attackEffect;
    [Tooltip("Attack sound (heavy slash or impact)")]
    public AudioClip attackSound;
    [Tooltip("Attack distance from player to spawn effect")]
    public float attackDistance = 1.5f;
    [Tooltip("Cooldown time between swings")]
    public float attackCooldown = 0.8f;

    private AudioSource audioSource;
    private bool canAttack = true;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        MouseFollowWithOffset();
    }

    public WeaponInfo GetWeaponInfo() => weaponInfo;

    private void MouseFollowWithOffset()
    {
        var mousePos = Input.mousePosition;
        var playerScreen = Camera.main.WorldToScreenPoint(PlayerController.Instance.transform.position);
        var dir = (Vector2)(mousePos - playerScreen);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (mousePos.x < playerScreen.x)
            ActiveWeapon.Instance.transform.rotation = Quaternion.Euler(180, 0, -angle);
        else
            ActiveWeapon.Instance.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void Attack()
    {
        if (!canAttack) return;
        canAttack = false;
        StartCoroutine(Cooldown());

        // world mouse position
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        // direction and angle
        Vector2 dir = (mouseWorld - PlayerController.Instance.transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // spawn slash a bit in front
        Vector3 spawnPos = PlayerController.Instance.transform.position + (Vector3)dir * attackDistance;

        var slash = Instantiate(attackEffect, spawnPos, Quaternion.Euler(0, 0, angle));
        slash.transform.SetParent(PlayerController.Instance.transform);

        if (audioSource) audioSource.PlayOneShot(attackSound);

        // Optional: CameraShake.Instance?.Shake(0.2f, 0.3f);
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}
