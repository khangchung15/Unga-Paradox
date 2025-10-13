using System.Threading;
using UnityEngine;

public class GrenadeEffect : MonoBehaviour
{
    public float fuseTime = 3.0f;
    public GameObject attackEffect;
    private float timer = 0.0f;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > fuseTime)
        {
            // Grenade explodes
            GameObject attack = Instantiate(attackEffect, transform.position, Quaternion.Euler(0, 0, 0));
            Destroy(gameObject);
        }
    }
}
