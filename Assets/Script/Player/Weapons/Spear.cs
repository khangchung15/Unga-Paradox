using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Spear : MonoBehaviour, IWeapon
{
    private SpriteRenderer spriteRenderer;

    [SerializeField] private WeaponInfo weaponInfo;

    [Header("Attack Settings")]
    [Tooltip("Attack sound")]
    public AudioClip attackSound;
    [Tooltip("Attack distance")]
    public float attackDistance = 1.0f;

    [Header("Ranged Attack Settings")]
    [Tooltip("Attack Velocity")]
    public GameObject attackProjectile;
    public float attackVelocity = 6.0f;

    private AudioSource audioSource;

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

    private void spriteReturn()
    {
        spriteRenderer.enabled = true;
    }

    private void MouseFollowWithOffset()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 playerScreenPoint = Camera.main.WorldToScreenPoint(PlayerController.Instance.transform.position);
        Vector2 direction = mousePos - playerScreenPoint;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        ActiveWeapon.Instance.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void Attack()
    {
        spriteRenderer.enabled = false;

        // Get mouse position in world
        Vector2 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        // Direction from player to mouse
        Vector2 direction = (mouseWorldPos - PlayerController.Instance.transform.position).normalized;

        // Angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Spawn position = player position + offset in mouse direction
        Vector3 spawnPos = PlayerController.Instance.transform.position + (Vector3)direction * attackDistance;

        // Instantiate projectile
        GameObject projectile = Instantiate(attackProjectile, spawnPos, Quaternion.Euler(0, 0, angle));
        projectile.GetComponent<Rigidbody2D>().linearVelocity = direction * attackVelocity;

        // Play attack sound
        if (audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        Invoke("spriteReturn", 0.75f);
    }
}
