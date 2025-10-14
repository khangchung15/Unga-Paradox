using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveWeapon : Singleton<ActiveWeapon>
{
    [SerializeField] private MonoBehaviour currentActiveWeapon;

    private PlayerControls playerControls;

    private bool attackButtonDown, isAttacking = false;

    protected override void Awake()
    {
        playerControls = new PlayerControls();
        base.Awake();
    }

    public PlayerControls GetPlayerControls()
    {
        return playerControls;
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    
    private void Start()
    {
        playerControls.Player.Attack.started += _ => StartAttacking();
        playerControls.Player.Attack.canceled += _ => StopAttacking();
    }

    private void Update()
    {
        Attack();
    }

    public void ToggleIsAttacking(bool value)
    {
        isAttacking = value;
    }

    private void StartAttacking()
    {
        attackButtonDown = true;
    }

    private void StopAttacking()
    {
        attackButtonDown = false;
    }

    private void Attack()
    {
        if (attackButtonDown && !isAttacking)
        {
            isAttacking = true;
            (currentActiveWeapon as IWeapon).Attack();
        }
    }
}
