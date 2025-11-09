using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Playables;

public class LensDistortionSettings : MonoBehaviour
{
    [Header("Volume Reference")]
    [SerializeField] private Volume postProcessingVolume;
    
    [Header("Lens Distortion Settings")]
    [Tooltip("Intensity of the lens distortion effect (-1 to 1)")]
    [SerializeField] [Range(-1f, 1f)] private float distortionIntensity = 0f;
    
    [Tooltip("X multiplier for the distortion (0 to 1)")]
    [SerializeField] [Range(0f, 1f)] private float distortionXMultiplier = 1f;
    
    [Tooltip("Y multiplier for the distortion (0 to 1)")]
    [SerializeField] [Range(0f, 1f)] private float distortionYMultiplier = 1f;
    
    [Tooltip("Center point of the distortion (0-1 range)")]
    [SerializeField] private Vector2 distortionCenter = new Vector2(0.5f, 0.5f);
    
    [Tooltip("Scale of the distortion effect")]
    [SerializeField] [Range(0.01f, 5f)] private float distortionScale = 1f;
    
    [Header("Animation Settings")]
    [Tooltip("Smoothly interpolate to target values (disable for Timeline animation)")]
    [SerializeField] private bool smoothTransition = false;
    
    [Tooltip("Speed of smooth transitions")]
    [SerializeField] private float transitionSpeed = 5f;
    
    // URP Lens Distortion
    private UnityEngine.Rendering.Universal.LensDistortion lensDistortion;
    
    // Target values for smooth transitions
    private float targetIntensity;
    private float targetXMultiplier;
    private float targetYMultiplier;
    private Vector2 targetCenter;
    private float targetScale;
    
    // Current values for smooth transitions
    private float currentIntensity;
    private float currentXMultiplier;
    private float currentYMultiplier;
    private Vector2 currentCenter;
    private float currentScale;
    
    // Track previous values to detect changes from Timeline
    private float prevIntensity;
    private float prevXMultiplier;
    private float prevYMultiplier;
    private Vector2 prevCenter;
    private float prevScale;

    void Start()
    {
        InitializeLensDistortion();
        
        // Initialize all values
        currentIntensity = distortionIntensity;
        targetIntensity = distortionIntensity;
        currentXMultiplier = distortionXMultiplier;
        targetXMultiplier = distortionXMultiplier;
        currentYMultiplier = distortionYMultiplier;
        targetYMultiplier = distortionYMultiplier;
        currentCenter = distortionCenter;
        targetCenter = distortionCenter;
        currentScale = distortionScale;
        targetScale = distortionScale;
        
        prevIntensity = distortionIntensity;
        prevXMultiplier = distortionXMultiplier;
        prevYMultiplier = distortionYMultiplier;
        prevCenter = distortionCenter;
        prevScale = distortionScale;
        
        ApplyLensDistortion();
    }
    
    void Awake()
    {
        InitializeLensDistortion();
    }

    void InitializeLensDistortion()
    {
        if (postProcessingVolume != null && postProcessingVolume.profile != null)
        {
            // Try to get existing Lens Distortion
            if (!postProcessingVolume.profile.TryGet(out lensDistortion))
            {
                // If it doesn't exist, add it
                lensDistortion = postProcessingVolume.profile.Add<UnityEngine.Rendering.Universal.LensDistortion>();
                Debug.Log("LensDistortion effect added to Volume profile");
            }
        }
        else
        {
            Debug.LogError("Post-processing Volume or Profile not assigned!");
        }
    }

    void Update()
    {
        // Check if Timeline or other script changed the serialized values
        bool valuesChanged = !Mathf.Approximately(distortionIntensity, prevIntensity) ||
                            !Mathf.Approximately(distortionXMultiplier, prevXMultiplier) ||
                            !Mathf.Approximately(distortionYMultiplier, prevYMultiplier) ||
                            distortionCenter != prevCenter ||
                            !Mathf.Approximately(distortionScale, prevScale);
        
        if (valuesChanged)
        {
            // Timeline or Inspector changed values
            if (smoothTransition)
            {
                targetIntensity = distortionIntensity;
                targetXMultiplier = distortionXMultiplier;
                targetYMultiplier = distortionYMultiplier;
                targetCenter = distortionCenter;
                targetScale = distortionScale;
            }
            else
            {
                // Apply immediately for Timeline
                ApplyLensDistortion();
            }
            
            // Update previous values
            prevIntensity = distortionIntensity;
            prevXMultiplier = distortionXMultiplier;
            prevYMultiplier = distortionYMultiplier;
            prevCenter = distortionCenter;
            prevScale = distortionScale;
        }
        
        if (smoothTransition)
        {
            // Smoothly interpolate to target values
            currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * transitionSpeed);
            currentXMultiplier = Mathf.Lerp(currentXMultiplier, targetXMultiplier, Time.deltaTime * transitionSpeed);
            currentYMultiplier = Mathf.Lerp(currentYMultiplier, targetYMultiplier, Time.deltaTime * transitionSpeed);
            currentCenter = Vector2.Lerp(currentCenter, targetCenter, Time.deltaTime * transitionSpeed);
            currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * transitionSpeed);
            
            ApplyLensDistortionSmooth();
        }
    }

    void ApplyLensDistortion()
    {
        if (lensDistortion == null) return;
        
        lensDistortion.intensity.value = distortionIntensity;
        lensDistortion.xMultiplier.value = distortionXMultiplier;
        lensDistortion.yMultiplier.value = distortionYMultiplier;
        lensDistortion.center.value = distortionCenter;
        lensDistortion.scale.value = distortionScale;
    }
    
    void ApplyLensDistortionSmooth()
    {
        if (lensDistortion == null) return;
        
        lensDistortion.intensity.value = currentIntensity;
        lensDistortion.xMultiplier.value = currentXMultiplier;
        lensDistortion.yMultiplier.value = currentYMultiplier;
        lensDistortion.center.value = currentCenter;
        lensDistortion.scale.value = currentScale;
    }

    // Public methods for Timeline control and scripting
    public void SetIntensity(float intensity)
    {
        distortionIntensity = Mathf.Clamp(intensity, -1f, 1f);
        targetIntensity = distortionIntensity;
        
        if (!smoothTransition)
        {
            currentIntensity = distortionIntensity;
            ApplyLensDistortion();
        }
    }

    public void SetXMultiplier(float multiplier)
    {
        distortionXMultiplier = Mathf.Clamp01(multiplier);
        targetXMultiplier = distortionXMultiplier;
        
        if (!smoothTransition)
        {
            currentXMultiplier = distortionXMultiplier;
            ApplyLensDistortion();
        }
    }

    public void SetYMultiplier(float multiplier)
    {
        distortionYMultiplier = Mathf.Clamp01(multiplier);
        targetYMultiplier = distortionYMultiplier;
        
        if (!smoothTransition)
        {
            currentYMultiplier = distortionYMultiplier;
            ApplyLensDistortion();
        }
    }

    public void SetCenter(Vector2 center)
    {
        distortionCenter = center;
        targetCenter = distortionCenter;
        
        if (!smoothTransition)
        {
            currentCenter = distortionCenter;
            ApplyLensDistortion();
        }
    }

    public void SetScale(float scale)
    {
        distortionScale = Mathf.Clamp(scale, 0.01f, 5f);
        targetScale = distortionScale;
        
        if (!smoothTransition)
        {
            currentScale = distortionScale;
            ApplyLensDistortion();
        }
    }

    public void SetAllValues(float intensity, float xMult, float yMult, Vector2 center, float scale)
    {
        distortionIntensity = Mathf.Clamp(intensity, -1f, 1f);
        distortionXMultiplier = Mathf.Clamp01(xMult);
        distortionYMultiplier = Mathf.Clamp01(yMult);
        distortionCenter = center;
        distortionScale = Mathf.Clamp(scale, 0.01f, 5f);
        
        targetIntensity = distortionIntensity;
        targetXMultiplier = distortionXMultiplier;
        targetYMultiplier = distortionYMultiplier;
        targetCenter = distortionCenter;
        targetScale = distortionScale;
        
        if (!smoothTransition)
        {
            currentIntensity = distortionIntensity;
            currentXMultiplier = distortionXMultiplier;
            currentYMultiplier = distortionYMultiplier;
            currentCenter = distortionCenter;
            currentScale = distortionScale;
            ApplyLensDistortion();
        }
    }

    public void ResetDistortion()
    {
        SetAllValues(0f, 1f, 1f, new Vector2(0.5f, 0.5f), 1f);
    }

    public void EnableEffect(bool enable)
    {
        if (lensDistortion != null)
        {
            lensDistortion.active = enable;
        }
    }

    // Context menu methods for testing in the editor
    [ContextMenu("Test Barrel Distortion")]
    public void TestBarrelDistortion()
    {
        SetIntensity(-0.5f);
    }

    [ContextMenu("Test Pincushion Distortion")]
    public void TestPincushionDistortion()
    {
        SetIntensity(0.5f);
    }

    [ContextMenu("Reset Distortion")]
    public void TestResetDistortion()
    {
        ResetDistortion();
    }

    void OnValidate()
    {
        // Apply changes in the editor when values are modified
        if (Application.isPlaying)
        {
            targetIntensity = distortionIntensity;
            targetXMultiplier = distortionXMultiplier;
            targetYMultiplier = distortionYMultiplier;
            targetCenter = distortionCenter;
            targetScale = distortionScale;
            
            if (!smoothTransition)
            {
                currentIntensity = distortionIntensity;
                currentXMultiplier = distortionXMultiplier;
                currentYMultiplier = distortionYMultiplier;
                currentCenter = distortionCenter;
                currentScale = distortionScale;
                ApplyLensDistortion();
            }
        }
    }
}