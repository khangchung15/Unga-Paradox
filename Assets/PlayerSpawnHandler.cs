using UnityEngine;

public class PlayerSpawnHandler : MonoBehaviour
{
    void Start()
    {
        // If we came from a transition, use that spawn point
        if (!string.IsNullOrEmpty(SceneEntryManager.lastEntryPoint))
        {
            Transform spawn = GameObject.Find(SceneEntryManager.lastEntryPoint)?.transform;

            if (spawn != null)
            {
                transform.position = spawn.position;
                SceneEntryManager.lastEntryPoint = null;   // clear after use
                return;
            }
        }

        // Otherwise use default spawn if it exists
        Transform defaultSpawn = GameObject.Find("DefaultSpawn")?.transform;
        if (defaultSpawn != null)
        {
            transform.position = defaultSpawn.position;
        }
    }
}
