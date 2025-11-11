using UnityEngine;

public class LevelGate : MonoBehaviour
{
    [Header("Colliders")]
    [SerializeField] Collider2D blocker;  // solid wall while locked
    [SerializeField] Collider2D portal;   // trigger when unlocked (optional)

    [Header("Visuals (optional)")]
    [SerializeField] GameObject lockedVisual;
    [SerializeField] GameObject unlockedVisual;
    [SerializeField] Animator anim;       // optional; bool "Open"

    void Reset()
    {
        // Try auto-hook
        blocker = GetComponent<Collider2D>();
    }

    public void SetLocked(bool locked)
    {
        if (blocker) blocker.enabled = locked;        // wall ON when locked
        if (portal)  portal.enabled  = !locked;       // portal ON when unlocked
        if (lockedVisual)   lockedVisual.SetActive(locked);
        if (unlockedVisual) unlockedVisual.SetActive(!locked);
        if (anim) anim.SetBool("Open", !locked);
    }
}
