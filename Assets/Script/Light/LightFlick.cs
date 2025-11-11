using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class LightFlick : MonoBehaviour
{
    public float minIntensity = 1.1f;
    public float maxIntensity = 1.3f;
    public float speed = 3f;

    [SerializeField] private Light2D target;

    void Awake()
    {
        if (!target) target = GetComponent<Light2D>();
        if (!target) target = GetComponentInChildren<Light2D>();
        if (!target) target = GetComponentInParent<Light2D>();
    }

    void Update()
    {
        if (!target) return;

        float noise = Mathf.PerlinNoise(Time.time * speed, 0f);
        target.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}
