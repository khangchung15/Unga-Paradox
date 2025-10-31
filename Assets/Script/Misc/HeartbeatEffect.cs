using UnityEngine;
using UnityEngine.Rendering;

public class HeartbeatEffect : MonoBehaviour
{
    [Header("Volume Reference")]
    [SerializeField] private Volume postProcessingVolume;

    [Header("Heartbeat Settings")]
    [SerializeField] private float heartbeatInterval = 1f;
    [SerializeField] private float heartbeatRiseDuration = 0.15f;  // Time to reach peak
    [SerializeField] private float heartbeatFallDuration = 0.15f;  // Time to return to normal
    [SerializeField] private AnimationCurve riseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve fallCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    [Header("Chromatic Aberration Settings")]
    [SerializeField] private float maxChromaticIntensity = 0.5f;
    
    [Header("Other Effects")]
    [SerializeField] private float maxVignetteIntensity = 0.4f;
    [SerializeField] private float maxSaturationShift = -20f;
    
    [Header("Sound Effects")]
    [SerializeField] private AudioClip heartbeatSound;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool playSoundOnHeartbeat = true;
    [SerializeField] private float soundVolume = 1f;
    
    // URP uses these types
    private UnityEngine.Rendering.Universal.ChromaticAberration chromaticAberration;
    private UnityEngine.Rendering.Universal.Vignette vignette;
    private UnityEngine.Rendering.Universal.ColorAdjustments colorAdjustments;
    
    private float timer;
    private HeartbeatState currentState = HeartbeatState.Idle;
    private float stateTimer = 0f;
    private bool hasPlayedSoundThisCycle = false;

    private enum HeartbeatState
    {
        Idle,
        Rising,
        Falling
    }

    void Start()
    {
        // Get references to the URP post-processing effects
        if (postProcessingVolume != null && postProcessingVolume.profile != null)
        {
            postProcessingVolume.profile.TryGet(out chromaticAberration);
            postProcessingVolume.profile.TryGet(out vignette);
            postProcessingVolume.profile.TryGet(out colorAdjustments);
        }

        // If no audio source is assigned, try to get one on this GameObject
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        // If still no audio source, create one
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }

        ResetEffects();
    }

    void Update()
    {
        timer += Time.deltaTime;
        
        // Start new heartbeat when interval is reached
        if (timer >= heartbeatInterval && currentState == HeartbeatState.Idle)
        {
            StartRising();
            timer = 0f;
        }
        
        // Handle the current state
        switch (currentState)
        {
            case HeartbeatState.Rising:
                stateTimer += Time.deltaTime;
                float riseProgress = Mathf.Clamp01(stateTimer / heartbeatRiseDuration);
                AnimateRise(riseProgress);
                
                if (riseProgress >= 1f)
                {
                    StartFalling();
                }
                break;
                
            case HeartbeatState.Falling:
                stateTimer += Time.deltaTime;
                float fallProgress = Mathf.Clamp01(stateTimer / heartbeatFallDuration);
                AnimateFall(fallProgress);
                
                if (fallProgress >= 1f)
                {
                    EndHeartbeat();
                }
                break;
        }
    }

    void StartRising()
    {
        currentState = HeartbeatState.Rising;
        stateTimer = 0f;
        hasPlayedSoundThisCycle = false;
        
        // Play sound at the start of heartbeat
        PlayHeartbeatSound();
    }

    void StartFalling()
    {
        currentState = HeartbeatState.Falling;
        stateTimer = 0f;
    }

    void EndHeartbeat()
    {
        currentState = HeartbeatState.Idle;
        stateTimer = 0f;
        hasPlayedSoundThisCycle = false;
        ResetEffects();
    }

    void PlayHeartbeatSound()
    {
        if (playSoundOnHeartbeat && heartbeatSound != null && audioSource != null && !hasPlayedSoundThisCycle)
        {
            audioSource.PlayOneShot(heartbeatSound, soundVolume);
            hasPlayedSoundThisCycle = true;
        }
    }

    void AnimateRise(float progress)
    {
        // Use the rise curve for smooth easing in
        float curveValue = riseCurve.Evaluate(progress);
        
        // Animate effects smoothly rising
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = curveValue * maxChromaticIntensity;
        }
        
        if (vignette != null)
        {
            vignette.intensity.value = curveValue * maxVignetteIntensity;
        }
        
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = curveValue * maxSaturationShift;
        }
    }

    void AnimateFall(float progress)
    {
        // Use the fall curve for smooth easing out
        float curveValue = fallCurve.Evaluate(progress);
        
        // Animate effects smoothly falling
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = curveValue * maxChromaticIntensity;
        }
        
        if (vignette != null)
        {
            vignette.intensity.value = curveValue * maxVignetteIntensity;
        }
        
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = curveValue * maxSaturationShift;
        }
    }

    void ResetEffects()
    {
        if (chromaticAberration != null) chromaticAberration.intensity.value = 0f;
        if (vignette != null) vignette.intensity.value = 0f;
        if (colorAdjustments != null) colorAdjustments.saturation.value = 0f;
    }

    [ContextMenu("Test Heartbeat")]
    public void TestHeartbeat()
    {
        if (currentState == HeartbeatState.Idle)
        {
            StartRising();
        }
    }
    
    [ContextMenu("Test Heartbeat Sound")]
    public void TestHeartbeatSound()
    {
        PlayHeartbeatSound();
    }
    
    // Public methods to control sound
    public void SetHeartbeatSound(AudioClip newSound)
    {
        heartbeatSound = newSound;
    }
    
    public void SetSoundVolume(float volume)
    {
        soundVolume = Mathf.Clamp01(volume);
    }
    
    public void EnableSound(bool enable)
    {
        playSoundOnHeartbeat = enable;
    }
}