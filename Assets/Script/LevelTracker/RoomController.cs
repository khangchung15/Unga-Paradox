using UnityEngine;

public class RoomController : MonoBehaviour
{
    [SerializeField] LevelGate gate;

    int alive;

    void Start()
    {
        // Lock if any enemies will register at runtime
        if (gate) gate.SetLocked(true);
    }

    // Called by EnemyDeathNotifier.Awake()
    public void RegisterEnemy()
    {
        alive++;
        if (gate) gate.SetLocked(true);
    }

    // Called once by EnemyDeathNotifier when its EnemyHealth reports dead
    public void NotifyEnemyDied()
    {
        alive = Mathf.Max(0, alive - 1);
        if (alive == 0 && gate) gate.SetLocked(false);
    }
}
