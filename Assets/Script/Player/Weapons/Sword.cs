using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] private GameObject slashAnimPrefab;
    [SerializeField] private Transform slashAnimSpawnPoint;
    private PlayerControls playerControls;
    private Animator myAnimator;
    private PlayerController playerController;
    private ActiveWeapon activeWeapon;
    private GameObject slashAnim;

    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
        activeWeapon = GetComponentInParent<ActiveWeapon>();
        myAnimator = GetComponent<Animator>();
        playerControls = new PlayerControls();
    }
    private void OnEnable()
    {
        playerControls.Enable();
    }
    void Start()
    {
        playerControls.Player.Attack.started += _ => Attack();
    }

    private void Update() { MouseFollowWithOffset(); }

    private void Attack() { myAnimator.SetTrigger("Attack"); }

    private void MouseFollowWithOffset()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 playerScreenPoint = Camera.main.WorldToScreenPoint(playerController.transform.position);
        Vector2 direction = mousePos - playerScreenPoint;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (mousePos.x < playerScreenPoint.x)
        {
            activeWeapon.transform.rotation = Quaternion.Euler(180, 0, -angle);
        }
        else { activeWeapon.transform.rotation = Quaternion.Euler(0, 0, angle); }
    }
}