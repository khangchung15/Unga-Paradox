using UnityEngine;


public class LightFlicker : MonoBehaviour
{
    [Header("Light Reference")]
    public UnityEngine.Rendering.Universal.Light2D targetLight;
    
    [Header("Flicker Settings")]
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.5f;
    public float flickerSpeed = 10f;
    public float flickerRandomness = 0.2f;
    
    [Header("Random Color Flicker (Optional)")]
    public bool enableColorFlicker = false;
    public Color[] randomColors;
    
    private float baseIntensity;
    private Color baseColor;
    private float randomSeed;
    
    void Start()
    {
        // If no light is assigned, try to get it from the same GameObject
        if (targetLight == null)
        {
            targetLight = GetComponent<UnityEngine.Rendering.Universal.Light2D>();
        }
        
        if (targetLight != null)
        {
            baseIntensity = targetLight.intensity;
            baseColor = targetLight.color;
        }
        else
        {
            Debug.LogError("No Light2D component found! Please assign a Light2D component.");
            enabled = false;
            return;
        }
        
        // Generate a random seed for unique flicker patterns
        randomSeed = Random.Range(0f, 100f);
    }
    
    void Update()
    {
        if (targetLight == null) return;
        
        // Calculate flicker using Perlin noise for smooth randomness
        float noise = Mathf.PerlinNoise(randomSeed, Time.time * flickerSpeed);
        
        // Add additional randomness for more natural flicker
        float randomVariation = Random.Range(-flickerRandomness, flickerRandomness);
        
        // Calculate final intensity
        float intensityMultiplier = Mathf.Lerp(minIntensity, maxIntensity, noise);
        targetLight.intensity = baseIntensity * (intensityMultiplier + randomVariation);
        
        // Optional color flickering
        if (enableColorFlicker && randomColors.Length > 0)
        {
            // Occasionally change color for fire-like effect
            if (Random.Range(0f, 1f) < 0.05f) // 5% chance per frame
            {
                Color randomColor = randomColors[Random.Range(0, randomColors.Length)];
                targetLight.color = Color.Lerp(baseColor, randomColor, 0.3f);
            }
            else
            {
                // Smoothly return to base color
                targetLight.color = Color.Lerp(targetLight.color, baseColor, Time.deltaTime * 2f);
            }
        }
    }
    
    // Method to reset to original settings
    public void ResetLight()
    {
        if (targetLight != null)
        {
            targetLight.intensity = baseIntensity;
            targetLight.color = baseColor;
        }
    }
    
    // Method for a dramatic flicker (useful for lightning or explosions)
    public void TriggerDramaticFlicker(float duration = 0.5f)
    {
        StartCoroutine(DramaticFlickerRoutine(duration));
    }
    
    private System.Collections.IEnumerator DramaticFlickerRoutine(float duration)
    {
        float elapsed = 0f;
        float originalMin = minIntensity;
        float originalMax = maxIntensity;
        
        // Increase flicker range for dramatic effect
        minIntensity = 0f;
        maxIntensity = 3f;
        flickerSpeed *= 3f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Restore original settings
        minIntensity = originalMin;
        maxIntensity = originalMax;
        flickerSpeed /= 3f;
    }
}