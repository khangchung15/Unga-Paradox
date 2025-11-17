using UnityEngine;

public class RoomHazard : MonoBehaviour
{
    [Header("Optional: falling rock spawner in this room")]
    [SerializeField] RockRainRandom rockRain;

    [Header("Toggle these when the room is locked")]
    [SerializeField] Collider2D[] collidersToEnable;
    [SerializeField] Behaviour[] behavioursToEnable; // e.g. TrapDamage, TrapDamageEvents, custom scripts

    // Convenience: auto-fill from children if left empty
    void OnValidate()
    {
        if (collidersToEnable == null || collidersToEnable.Length == 0)
            collidersToEnable = GetComponentsInChildren<Collider2D>(true);

        if (behavioursToEnable == null || behavioursToEnable.Length == 0)
            behavioursToEnable = GetComponentsInChildren<Behaviour>(true);

        if (!rockRain)
            rockRain = GetComponentInChildren<RockRainRandom>(true);
    }

    /// <summary> Enable/disable hazards for this room. </summary>
    public void Enable(bool on)
    {
        if (rockRain) rockRain.EnableRain(on);

        if (collidersToEnable != null)
            foreach (var c in collidersToEnable) if (c) c.enabled = on;

        if (behavioursToEnable != null)
            foreach (var b in behavioursToEnable) if (b) b.enabled = on;
    }
}
