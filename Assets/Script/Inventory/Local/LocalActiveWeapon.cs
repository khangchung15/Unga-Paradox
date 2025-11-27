using UnityEngine;

public class LocalActiveWeapon : MonoBehaviour
{
    public static LocalActiveWeapon Instance { get; private set; }
    public MonoBehaviour CurrentActiveWeapon { get; private set; }

    private PlayerControls playerControls;
    private float timeBetweenAttacks;
    private bool attackButtonDown, isAttacking = false;
    private bool isSecondaryAttacking = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        playerControls = new PlayerControls();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public PlayerControls GetPlayerControls()
    {
        return playerControls;
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Start()
    {
        playerControls.Player.Attack.started += _ => StartAttacking();
        playerControls.Player.Attack.canceled += _ => StopAttacking();
    }

    private void Update()
    {
        Attack();
        SecondaryAttack();
    }

    public void NewWeapon(MonoBehaviour newWeapon)
    {
        CurrentActiveWeapon = newWeapon;
        timeBetweenAttacks = (CurrentActiveWeapon as IWeapon).GetWeaponInfo().weaponCooldown;
        
        isAttacking = false;
        isSecondaryAttacking = false;
        
        SetWeaponSortingLayer(newWeapon.gameObject);
    }

    private void SetWeaponSortingLayer(GameObject weaponObject)
    {
        SpriteRenderer[] renderers = weaponObject.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.sortingLayerName = "Ground";
            renderer.sortingOrder = 7;
        }
    }


    public void WeaponNull()
    {
        CurrentActiveWeapon = null;
    }

    private void AttackCooldown()
    {
        isAttacking = true;
        StopAllCoroutines();
        StartCoroutine(TimeBetweenAttacksRoutine());
    }

    private System.Collections.IEnumerator TimeBetweenAttacksRoutine()
    {
        yield return new WaitForSeconds(timeBetweenAttacks);
        isAttacking = false;
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
        if (attackButtonDown && !isAttacking && CurrentActiveWeapon != null)
        {
            AttackCooldown();
            (CurrentActiveWeapon as IWeapon).Attack();
        }
    }

    private void SecondaryAttack()
    {
        if (Input.GetMouseButtonDown(1) && !isSecondaryAttacking && CurrentActiveWeapon != null)
        {
            isSecondaryAttacking = true;
            
            if (CurrentActiveWeapon is ButterflyKnife butterflyKnife)
            {
                butterflyKnife.SecondaryAttack();
            }
            else if (CurrentActiveWeapon is PotionWeapon potionWeapon)
            {
                potionWeapon.SecondaryAttack();
            }
            
            StartCoroutine(SecondaryAttackCooldownRoutine());
        }
    }


    private System.Collections.IEnumerator SecondaryAttackCooldownRoutine()
    {
        yield return new WaitForSeconds(timeBetweenAttacks);
        isSecondaryAttacking = false;
    }
}
