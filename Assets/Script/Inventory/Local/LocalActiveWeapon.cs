using UnityEngine;

public class LocalActiveWeapon : MonoBehaviour
{
    public static LocalActiveWeapon Instance { get; private set; }
    public MonoBehaviour CurrentActiveWeapon { get; private set; }

    private PlayerControls playerControls;
    private float timeBetweenAttacks;
    private bool attackButtonDown, isAttacking = false;
    private bool isSecondaryAttacking = false;
    private AudioSource weaponEquipAudioSource;
    private GameObject playerObject;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        playerControls = new PlayerControls();
        
        weaponEquipAudioSource = gameObject.AddComponent<AudioSource>();
        weaponEquipAudioSource.playOnAwake = false;
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
        
        playerObject = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        Attack();
        SecondaryAttack();
    }
    
    private bool IsPlayerFrozen()
    {
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }
        
        if (playerObject != null)
        {
            FrozenEntity frozenEntity = playerObject.GetComponent<FrozenEntity>();
            return frozenEntity != null && frozenEntity.IsFrozen;
        }
        
        return false;
    }

    public void NewWeapon(MonoBehaviour newWeapon)
    {
        CurrentActiveWeapon = newWeapon;
        WeaponInfo weaponInfo = (CurrentActiveWeapon as IWeapon).GetWeaponInfo();
        timeBetweenAttacks = weaponInfo.weaponCooldown;
        
        isAttacking = false;
        isSecondaryAttacking = false;
        
        SetWeaponSortingLayer(newWeapon.gameObject);
        PlayEquipSound(weaponInfo);
    }

    private void PlayEquipSound(WeaponInfo weaponInfo)
    {
        if (weaponInfo.equipSound != null && weaponEquipAudioSource != null)
        {
            if (weaponEquipAudioSource.isPlaying)
            {
                weaponEquipAudioSource.Stop();
            }
            
            weaponEquipAudioSource.PlayOneShot(weaponInfo.equipSound);
            Debug.Log($"[LocalActiveWeapon] Playing equip sound for {weaponInfo.weaponName}");
        }
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
        if (IsPlayerFrozen()) return;
        
        if (attackButtonDown && !isAttacking && CurrentActiveWeapon != null)
        {
            AttackCooldown();
            (CurrentActiveWeapon as IWeapon).Attack();
        }
    }

    private void SecondaryAttack()
    {
        if (IsPlayerFrozen()) return;
        
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
