using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyDeathNotifier : MonoBehaviour
{
    EnemyHealth health;
    RoomController room;
    bool announced;

    void Awake()
    {
        health = GetComponent<EnemyHealth>();
        room   = FindFirstObjectByType<RoomController>(); // Unity 2022+; use FindObjectOfType if older
        if (room) room.RegisterEnemy();
    }

    void Update()
    {
        if (!announced && health != null && health.IsDead())
        {
            announced = true;
            if (room) room.NotifyEnemyDied();
            // Optional: if your enemy isn't destroyed elsewhere, destroy after SFX time:
            // Destroy(gameObject, 0.35f);
        }
    }
}
