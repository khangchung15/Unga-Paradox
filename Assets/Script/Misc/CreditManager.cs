using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class CreditsManager : MonoBehaviour
{
    [Header("Credits Scrollers")]
    [SerializeField] private List<CreditsScroller> scrollers = new List<CreditsScroller>();
    
    [Header("Settings")]
    [SerializeField] private bool autoStart = true;
    [SerializeField] private float delayBetweenScrollers = 0.5f;
    [Tooltip("Start next scroller when current is X% complete (0-1). 1 = wait for full completion")]
    [SerializeField][Range(0f, 1f)] private float startNextAtPercent = 0.6f;
    
    [Header("Events")]
    public UnityEvent onAllCreditsFinished;
    
    private int currentScrollerIndex = -1;
    private bool isRunning = false;
    private List<bool> scrollerStarted = new List<bool>();

    void Start()
    {
        Debug.Log($"[CreditsManager] Starting with {scrollers.Count} scrollers");
        
        foreach (var scroller in scrollers)
        {
            scrollerStarted.Add(false);
            if (scroller != null)
            {
                scroller.onCreditsFinished.AddListener(OnScrollerFinished);
                Debug.Log($"[CreditsManager] Registered scroller: {scroller.gameObject.name}");
            }
        }

        if (autoStart)
        {
            StartCredits();
        }
    }

    void Update()
    {
        if (!isRunning) return;

        int nextIndex = currentScrollerIndex + 1;
        if (nextIndex >= scrollers.Count) return;
        if (scrollerStarted[nextIndex]) return;

        CreditsScroller currentScroller = scrollers[currentScrollerIndex];
        if (currentScroller != null && currentScroller.GetScrollProgress() >= startNextAtPercent)
        {
            scrollerStarted[nextIndex] = true;
            if (delayBetweenScrollers > 0)
            {
                Invoke(nameof(StartNextScrollerDirect), delayBetweenScrollers);
            }
            else
            {
                StartNextScrollerDirect();
            }
        }
    }

    public void StartCredits()
    {
        if (scrollers.Count == 0)
        {
            Debug.LogWarning("[CreditsManager] No scrollers assigned!");
            return;
        }

        Debug.Log("[CreditsManager] Starting credits sequence");
        currentScrollerIndex = 0;
        isRunning = true;
        
        for (int i = 0; i < scrollerStarted.Count; i++)
        {
            scrollerStarted[i] = false;
        }
        
        if (scrollers[0] != null)
        {
            scrollerStarted[0] = true;
            scrollers[0].StartCredits();
            Debug.Log($"[CreditsManager] Started first scroller");
        }
    }

    private void StartNextScrollerDirect()
    {
        int nextIndex = currentScrollerIndex + 1;
        if (nextIndex < scrollers.Count && scrollers[nextIndex] != null)
        {
            Debug.Log($"[CreditsManager] Starting scroller {nextIndex}: {scrollers[nextIndex].gameObject.name}");
            scrollers[nextIndex].StartCredits();
            currentScrollerIndex = nextIndex;
        }
    }

    private void OnScrollerFinished()
    {
        Debug.Log($"[CreditsManager] A scroller finished");
        
        bool allFinished = true;
        foreach (var scroller in scrollers)
        {
            if (scroller != null && !scroller.IsFinished())
            {
                allFinished = false;
                break;
            }
        }

        if (allFinished)
        {
            Debug.Log("[CreditsManager] All scrollers finished!");
            isRunning = false;
            if (onAllCreditsFinished != null)
            {
                onAllCreditsFinished.Invoke();
            }
        }
    }

    public void StopAllCredits()
    {
        Debug.Log("[CreditsManager] Stopping all credits");
        isRunning = false;
        
        foreach (var scroller in scrollers)
        {
            if (scroller != null)
            {
                scroller.StopCredits();
            }
        }
    }

    void OnDestroy()
    {
        foreach (var scroller in scrollers)
        {
            if (scroller != null)
            {
                scroller.onCreditsFinished.RemoveListener(OnScrollerFinished);
            }
        }
    }
}
