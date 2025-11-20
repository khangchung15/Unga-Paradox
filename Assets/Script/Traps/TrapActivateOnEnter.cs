using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TrapActivateOnEnter : MonoBehaviour
{
    [SerializeField] string targetTag = "Player";
    [SerializeField] float cooldown = 0.6f;

    static readonly int Activate = Animator.StringToHash("Activate");
    Animator anim;
    float nextReady;

    void Awake()
    {
        anim = GetComponentInParent<Animator>();
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(targetTag)) return;
        if (Time.time < nextReady) return;

        Debug.Log("[Trap] Activate fired");
        anim.ResetTrigger(Activate);
        anim.SetTrigger(Activate);   // Idle -> Cycle

        nextReady = Time.time + cooldown;
    }
}
