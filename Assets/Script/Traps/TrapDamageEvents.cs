using UnityEngine;

public class TrapDamageEvents : MonoBehaviour
{
    TrapDamage d;
    void Awake() => d = GetComponent<TrapDamage>();

    // called by Animation Events
    public void StartDamage() { if (d) d.isDangerous = true; }
    public void StopDamage()  { if (d) d.isDangerous = false; }
}
