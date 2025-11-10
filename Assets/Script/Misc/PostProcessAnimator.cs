using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class PostProcessAnimator : MonoBehaviour
{
    [SerializeField] private Volume postProcessingVolume;
    [SerializeField] private bool disable;

    [Header("Post Processing Profiles")]
    [SerializeField] private VolumeProfile postProfileMain;
    [SerializeField] private VolumeProfile postProfileSecondary;

    [Header("Heartbeat Settings")]
    [SerializeField] private float heartbeatInterval = 1f;
    [SerializeField] private float heartbeatDuration = 0.2f;
    
    private Coroutine heartbeatCoroutine;
    private bool isHeartbeatActive = false;

    void Start()
    {
        SwitchToMainProfile();
        if (!disable)
        {
            StartHeartbeatLoop();
        }
    }

    void StartHeartbeatLoop()
    {
        if (heartbeatCoroutine != null)
            StopCoroutine(heartbeatCoroutine);
        
        heartbeatCoroutine = StartCoroutine(HeartbeatRoutine());
    }

    IEnumerator HeartbeatRoutine()
    {
        while (!disable)
        {
            yield return new WaitForSeconds(heartbeatInterval);
            
            // Switch to heartbeat profile
            SwitchToSecondaryProfile();
            isHeartbeatActive = true;
            
            yield return new WaitForSeconds(heartbeatDuration);
            
            // Switch back to main profile
            SwitchToMainProfile();
            isHeartbeatActive = false;
        }
    }

    // Manual trigger
    public void TriggerHeartbeat()
    {
        if (!isHeartbeatActive)
        {
            StartCoroutine(SingleHeartbeat());
        }
    }

    IEnumerator SingleHeartbeat()
    {
        isHeartbeatActive = true;
        SwitchToSecondaryProfile();
        yield return new WaitForSeconds(heartbeatDuration);
        SwitchToMainProfile();
        isHeartbeatActive = false;
    }

    public void SwitchToMainProfile()
    {
        if (postProcessingVolume != null && postProfileMain != null)
        {
            postProcessingVolume.profile = postProfileMain;
        }
    }

    public void SwitchToSecondaryProfile()
    {
        if (postProcessingVolume != null && postProfileSecondary != null)
        {
            postProcessingVolume.profile = postProfileSecondary;
        }
    }

    void OnDisable()
    {
        if (heartbeatCoroutine != null)
        {
            StopCoroutine(heartbeatCoroutine);
        }
        SwitchToMainProfile();
    }
}