
using UnityEngine;

public class SceneEntryManager : MonoBehaviour
{
    public static string lastEntryPoint = null;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}

