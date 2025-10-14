using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Sword : MonoBehaviour, IWeapon
{
    [SerializeField] private WeaponInfo weaponInfo;

    [Header("Attack Settings")]
    [Tooltip("Attack effect (Also holds damage)")]
    public GameObject attackEffect;
    [Tooltip("Attack sound")]
    public AudioClip attackSound;
    [Tooltip("Attack distance")]
    public float attackDistance = 1.0f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
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

        // Instantiate attack effect
        GameObject attack = Instantiate(attackEffect, spawnPos, Quaternion.Euler(0, 0, angle));
        attack.transform.SetParent(PlayerController.Instance.transform);

        // Play attack sound
        if (audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }
}
