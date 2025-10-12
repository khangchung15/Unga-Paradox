using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseController : MonoBehaviour
{
    public static bool IsGamePaused { get; private set; } = false;
    public static void SetPause(bool pause)
    {
        IsGamePaused = pause;
    }
}
