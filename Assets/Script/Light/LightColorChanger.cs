using UnityEngine;

public class LightColorChanger : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // The target GameObject to detect
    
    [Header("Light Settings")]
    public float detectionRange = 10.0f; // Range at which color changes
    public Color normalColor = Color.white; // Default light color
    public Color detectedColor = Color.red; // Color when target is in range
    
    [Header("Color Transition")]
    public float colorChangeSpeed = 2.0f; // How fast color transitions
    public bool keepColorAfterExit = false; // If true, color stays changed even after target leaves range
    
    [Header("Intensity Settings")]
    public bool changeIntensity = true; // Enable/disable intensity changes
    public float normalIntensity = 1.0f; // Default light intensity
    public float detectedIntensity = 2.0f; // Intensity when target is in range
    public float intensityChangeSpeed = 2.0f; // How fast intensity transitions
    
    [Header("Reset Options")]
    public bool resetOnDisable = true; // Reset to normal when component is disabled
    public KeyCode manualResetKey = KeyCode.None; // Optional key to manually reset color
    
    private Component spotlight;
    private bool targetInRange = false;
    private bool colorLocked = false; // Track if color is locked after detection
    private System.Reflection.PropertyInfo colorProperty;
    private System.Reflection.PropertyInfo intensityProperty;
    
    // Public properties to control behavior from other scripts
    public bool IsColorLocked => colorLocked;
    public bool IsTargetInRange => targetInRange;
    
    void Start()
    {
        InitializeLightComponents();
    }
    
    void InitializeLightComponents()
    {
        // Try to get the Light2D component using reflection
        spotlight = GetComponent("Light2D");
        
        if (spotlight != null)
        {
            // Get the color property using reflection
            colorProperty = spotlight.GetType().GetProperty("color");
            intensityProperty = spotlight.GetType().GetProperty("intensity");
            
            if (colorProperty != null)
            {
                colorProperty.SetValue(spotlight, normalColor);
            }
            else
            {
                Debug.LogWarning("Color property not found on Light2D component!");
                spotlight = null;
            }
            
            if (intensityProperty != null && changeIntensity)
            {
                intensityProperty.SetValue(spotlight, normalIntensity);
            }
        }
        else
        {
            Debug.LogWarning("Light2D component not found on this GameObject! Make sure you have the 2D Renderer package installed.");
        }
    }
    
    void Update()
    {
        if (target == null || spotlight == null)
            return;
            
        // Handle manual reset
        if (manualResetKey != KeyCode.None && Input.GetKeyDown(manualResetKey))
        {
            ResetToNormal();
        }
            
        // Calculate distance to target
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        // Check if target is in range
        bool wasInRange = targetInRange;
        targetInRange = distanceToTarget <= detectionRange;
        
        // Handle detection logic
        if (targetInRange && !colorLocked)
        {
            // Target entered range - lock color if needed
            if (!wasInRange)
            {
                OnTargetEnterRange();
                if (keepColorAfterExit)
                {
                    colorLocked = true;
                }
            }
        }
        else if (!targetInRange && !colorLocked)
        {
            // Target left range - reset if not locked
            if (wasInRange)
            {
                OnTargetExitRange();
            }
        }
        
        // Handle color and intensity transitions
        HandleLightTransition();
    }
    
    void HandleLightTransition()
    {
        if (colorProperty == null) return;
        
        // Determine target values based on state
        Color targetColor = (targetInRange || colorLocked) ? detectedColor : normalColor;
        float targetIntensity = (targetInRange || colorLocked) ? detectedIntensity : normalIntensity;
        
        // Get current values
        Color currentColor = (Color)colorProperty.GetValue(spotlight);
        float currentIntensity = intensityProperty != null ? (float)intensityProperty.GetValue(spotlight) : normalIntensity;
        
        // Smoothly transition colors
        Color newColor = Color.Lerp(currentColor, targetColor, colorChangeSpeed * Time.deltaTime);
        colorProperty.SetValue(spotlight, newColor);
        
        // Smoothly transition intensity if enabled
        if (intensityProperty != null && changeIntensity)
        {
            float newIntensity = Mathf.Lerp(currentIntensity, targetIntensity, intensityChangeSpeed * Time.deltaTime);
            intensityProperty.SetValue(spotlight, newIntensity);
        }
    }
    
    // Public methods to control the light externally
    public void ResetToNormal()
    {
        colorLocked = false;
        if (colorProperty != null)
        {
            colorProperty.SetValue(spotlight, normalColor);
        }
        if (intensityProperty != null && changeIntensity)
        {
            intensityProperty.SetValue(spotlight, normalIntensity);
        }
        Debug.Log("Light reset to normal color and intensity");
    }
    
    public void LockToDetectedColor()
    {
        colorLocked = true;
        if (colorProperty != null)
        {
            colorProperty.SetValue(spotlight, detectedColor);
        }
        if (intensityProperty != null && changeIntensity)
        {
            intensityProperty.SetValue(spotlight, detectedIntensity);
        }
        Debug.Log("Light locked to detected color and intensity");
    }
    
    public void UnlockColor()
    {
        colorLocked = false;
        Debug.Log("Light color unlocked");
    }
    
    public void SetColors(Color newNormalColor, Color newDetectedColor)
    {
        normalColor = newNormalColor;
        detectedColor = newDetectedColor;
    }
    
    public void SetIntensities(float newNormalIntensity, float newDetectedIntensity)
    {
        normalIntensity = newNormalIntensity;
        detectedIntensity = newDetectedIntensity;
    }
    
    // Event methods that you can extend
    void OnTargetEnterRange()
    {
        Debug.Log("Target entered spotlight range!");
        // You can add sound effects, particle systems, or other reactions here
    }
    
    void OnTargetExitRange()
    {
        Debug.Log("Target left spotlight range!");
        // You can add sound effects, particle systems, or other reactions here
    }
    
    void OnDisable()
    {
        if (resetOnDisable)
        {
            ResetToNormal();
        }
    }
    
    // Visualize detection range in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = (targetInRange || colorLocked) ? detectedColor : normalColor;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        if ((targetInRange || colorLocked) && target != null)
        {
            Gizmos.color = detectedColor;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}