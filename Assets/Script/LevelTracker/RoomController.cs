using UnityEngine;

public class RoomController : MonoBehaviour
{
    [Header("What this room controls")]
    [SerializeField] LevelGate[] gates;
    [SerializeField] RoomHazard[] hazards;  // <-- changed

    int alive;

    void Start() { SetLockedAll(true); }

    public void RegisterEnemy() { alive++; SetLockedAll(true); }

    public void NotifyEnemyDied()
    {
        alive = Mathf.Max(0, alive - 1);
        if (alive == 0) { SetLockedAll(false); EnableHazards(false); }
    }

    void SetLockedAll(bool locked)
    {
        if (gates != null) foreach (var g in gates) if (g) g.SetLocked(locked);
        if (locked) EnableHazards(true);
    }

    void EnableHazards(bool on)
    {
        if (hazards != null) foreach (var h in hazards) if (h) h.Enable(on);
    }
}
