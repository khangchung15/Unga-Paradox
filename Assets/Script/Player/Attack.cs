using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Attack : MonoBehaviour
{
    private PlayerControls controls;    // input actions
    private float timer = 0.0f;         // timer for attack cooldown
    private bool playOnce = false;      // bool for attack cooldown
    private Transform parentTransform;
    private ActiveWeapon activeWeapon;

    [Header("Attack Settings")]
    [Tooltip("True for melee, false for ranged")]
    public bool isMelee;
    [Tooltip("Attack effect (Also holds damage)")]
    public GameObject attackEffect;
    [Tooltip("Attack sound")]
    public AudioClip attackSound;
    [Tooltip("Attack distance")]
    public float attackDistance = 1.0f;
    [Tooltip("Attack cooldown")]
    public float attackCooldown = 1.0f;

    [Header ("Ranged Attack Settings")]
    [Tooltip("Attack Velocity")]
    public GameObject attackProjectile;
    public float attackVelocity = 1.0f;

    private AudioSource audioSource;

    private void Awake()
    {
        controls = new PlayerControls();
        audioSource = GetComponent<AudioSource>();
        parentTransform = transform.parent;
        activeWeapon = GetComponentInParent<ActiveWeapon>();
    }

    // Enable attack
    private void OnEnable()
    {
        controls.Player.Enable();
        if (isMelee == true)
            controls.Player.Attack.performed += onAttack;
        else if (isMelee == false)
            controls.Player.Attack.performed += onAttackRanged;
    }

    // Disable attack
    private void OnDisable()
    {
        if (isMelee == true)
            controls.Player.Attack.performed -= onAttack;
        else if (isMelee == false)
            controls.Player.Attack.performed -= onAttackRanged;
        controls.Player.Disable();
    }

    private void Update()
    {
        MouseFollowWithOffset();
        // Attack Cooldown
        if (timer > attackCooldown)
        {
            if (!playOnce)
            {
                //Debug.Log("Attack Enable " + timer);
                controls.Player.Attack.Enable();
                playOnce = true;
            }
        }
        if (timer < attackCooldown)
        {
            if (playOnce)
            {
                //Debug.Log("Attack Disable " + timer);
                controls.Player.Attack.Disable();
                playOnce = false;
            }
        }
        timer += Time.deltaTime;
    }

    private void MouseFollowWithOffset()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 playerScreenPoint = Camera.main.WorldToScreenPoint(parentTransform.position);
        Vector2 direction = mousePos - playerScreenPoint;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (mousePos.x < playerScreenPoint.x)
        {
            activeWeapon.transform.rotation = Quaternion.Euler(180, 0, -angle);
        }
        else 
        {
            activeWeapon.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void onAttack(InputAction.CallbackContext context)
    {
        // Reset timer for cooldown
        timer = 0.0f;

        // Get mouse position in world
        Vector2 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        // Direction from player to mouse
        Vector2 direction = (mouseWorldPos - parentTransform.position).normalized;

        // Angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Spawn position = player position + offset in mouse direction
        Vector3 spawnPos = parentTransform.position + (Vector3)direction * attackDistance;

        // Instantiate attack effect
        GameObject attack = Instantiate(attackEffect, spawnPos, Quaternion.Euler(0, 0, angle));
        attack.transform.SetParent(transform);

        // Play attack sound
        if (audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }

    private void onAttackRanged(InputAction.CallbackContext context)
    {
        // Reset timer for cooldown
        timer = 0.0f;

        // Get mouse position in world
        Vector2 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        // Direction from player to mouse
        Vector2 direction = (mouseWorldPos - parentTransform.position).normalized;

        // Angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Spawn position = player position + offset in mouse direction
        Vector3 spawnPos = parentTransform.position + (Vector3)direction * attackDistance;

        // Instantiate projectile
        GameObject projectile = Instantiate(attackProjectile, spawnPos, Quaternion.Euler(0, 0, angle));
        projectile.GetComponent<Rigidbody2D>().linearVelocity = direction * attackVelocity;

        // Play attack sound
        if (audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }
}
