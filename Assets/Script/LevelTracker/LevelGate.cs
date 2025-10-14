using UnityEngine;

public class LevelGate : MonoBehaviour
{
    [SerializeField] Collider2D gateCollider;   // scene-change collider
    [SerializeField] GameObject lockedVisual;   // optional group (your rocks)
    [SerializeField] GameObject unlockedVisual; // optional

    [SerializeField] bool useTriggerMode = false; // FALSE = use collisions (your current LevelChanger)

    void Reset()
    {
        gateCollider = GetComponent<Collider2D>();
    }

    public void SetLocked(bool locked)
    {
        if (gateCollider)
        {
            gateCollider.isTrigger = useTriggerMode; // false -> collisions, true -> triggers
            gateCollider.enabled   = !locked;        // only active when UNLOCKED
        }
        if (lockedVisual)   lockedVisual.SetActive(locked);
        if (unlockedVisual) unlockedVisual.SetActive(!locked);
    }
}
