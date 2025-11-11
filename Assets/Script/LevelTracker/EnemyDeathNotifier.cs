using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyDeathNotifier : MonoBehaviour
{
    [SerializeField] RoomController room;   // assign OR auto-find parent
    EnemyHealth health;
    bool announced;

    void Awake()
    {
        health = GetComponent<EnemyHealth>();
        if (!room) room = GetComponentInParent<RoomController>(); // <-- key change
        if (room) room.RegisterEnemy();
    }

    void Update()
    {
        if (!announced && health != null && health.IsDead())
        {
            announced = true;
            if (room) room.NotifyEnemyDied();
            // Optionally Destroy after death SFX:
            // Destroy(gameObject, 0.35f);
        }
    }
}
