using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System.Collections;

public class CutsceneSkip : MonoBehaviour
{
    [Header("Skip Settings")]
    public PlayableDirector timelineDirector;
    public GameObject skipPrompt;
    public float skipDelay = 1f;
    public string[] allowedCutsceneScenes;
    
    [Header("Skip Mode")]
    public bool loadNewScene = false;
    public string sceneToLoad;
    
    [Header("Optional: Objects to Enable/Disable")]
    public GameObject[] objectsToEnableAfterSkip;
    public GameObject[] objectsToDisableAfterSkip;
    
    [Header("Input Settings")]
    public bool useNewInputSystem = true;

    [Header("Auto Display Settings")]
    public float autoDisplayDuration = 3f;
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.5f;
    public CanvasGroup skipPromptCanvasGroup;
    
    private float timer = 0f;
    private bool canSkip = false;
    private bool hasSkipped = false;
    private bool isCutscenePlaying = false;
    
    void Start()
    {
        if (!IsInAllowedCutsceneScene())
        {
            if (skipPrompt != null)
            {
                skipPrompt.SetActive(false);
            }
            this.enabled = false;
            return;
        }
        
        InitializeCutscene();
    }

    void InitializeCutscene()
    {
        timer = 0f;
        canSkip = false;
        hasSkipped = false;
        
        if (timelineDirector == null)
        {
            timelineDirector = FindObjectOfType<PlayableDirector>();
        }

        // Check if a cutscene is actually playing
        if (timelineDirector != null)
        {
            isCutscenePlaying = timelineDirector.state == PlayState.Playing;
        }

        // ALWAYS show skip prompt when in allowed scene, but control skipping ability
        if (skipPrompt != null)
        {
            skipPrompt.SetActive(true);
            
            if (skipPromptCanvasGroup != null)
            {
                skipPromptCanvasGroup.alpha = 0f;
            }
            
            // Only auto-display if cutscene is playing
            if (isCutscenePlaying)
            {
                StartCoroutine(AutoDisplayPrompt());
            }
            else
            {
                // If no cutscene playing, keep prompt visible but make it clear skipping isn't available
                if (skipPromptCanvasGroup != null)
                {
                    skipPromptCanvasGroup.alpha = 0.0f; // Dimmed to indicate not available
                }
            }
        }
    }

    private IEnumerator AutoDisplayPrompt()
    {
        if (skipPromptCanvasGroup == null) yield break;
        
        float elapsed = 0f;
        
        // Fade in
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            skipPromptCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }
        
        skipPromptCanvasGroup.alpha = 1f;
        
        // Wait for display duration
        yield return new WaitForSeconds(autoDisplayDuration);
        
        // Fade out
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            skipPromptCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }
        
        skipPromptCanvasGroup.alpha = 0f;
    }

    void Update()
    {
        if (hasSkipped) return;
        
        // Only allow skipping if cutscene is playing
        if (!isCutscenePlaying) return;
        
        if (!canSkip)
        {
            timer += Time.deltaTime;
            if (timer >= skipDelay)
            {
                canSkip = true;
            }
            return;
        }
        
        if (CheckSkipInput())
        {
            SkipCutscene();
        }
    }
    
    private bool CheckSkipInput()
    {
        if (useNewInputSystem)
        {
            #if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            
            if (keyboard != null)
            {
                bool spacePressed = keyboard.spaceKey.wasPressedThisFrame;
                
                if (spacePressed)
                {
                    bool isAltPressed = keyboard.leftAltKey.isPressed || keyboard.rightAltKey.isPressed;
                    bool isCtrlPressed = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
                    
                    if (isAltPressed || isCtrlPressed)
                    {
                        return false;
                    }
                    
                    return true;
                }
            }
            
            return false;
            #else
            return Input.GetKeyDown(KeyCode.Space);
            #endif
        }
        else
        {
            return Input.GetKeyDown(KeyCode.Space);
        }
    }

    private bool IsInAllowedCutsceneScene()
    {
        if (allowedCutsceneScenes == null || allowedCutsceneScenes.Length == 0)
        {
            return true;
        }
        
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        foreach (string sceneName in allowedCutsceneScenes)
        {
            if (!string.IsNullOrEmpty(sceneName) && currentSceneName == sceneName)
            {
                return true;
            }
        }
        
        return false;
    }
    
    // Call this method when a cutscene starts
    public void OnCutsceneStart(PlayableDirector director = null)
    {
        if (director != null)
        {
            timelineDirector = director;
        }
        
        isCutscenePlaying = true;
        timer = 0f;
        canSkip = false;
        
        // Show prompt with full opacity when cutscene starts
        if (skipPromptCanvasGroup != null)
        {
            skipPromptCanvasGroup.alpha = 1f;
        }
        
        // Restart auto-display coroutine
        if (skipPrompt != null && skipPromptCanvasGroup != null)
        {
            StopAllCoroutines();
            StartCoroutine(AutoDisplayPrompt());
        }
    }
    
    public void SkipCutscene()
    {
        if (hasSkipped || !isCutscenePlaying) return;
        
        hasSkipped = true;
        isCutscenePlaying = false;
        
        // Mode 1: Load new scene
        if (loadNewScene && !string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
            return;
        }
        
        // Mode 2: Stop timeline in current scene
        if (timelineDirector != null)
        {
            timelineDirector.Stop();
            timelineDirector.time = timelineDirector.duration; // Jump to end
            timelineDirector.Evaluate(); // Update the timeline state
        }
        
        // Hide skip prompt after skipping
        if (skipPrompt != null)
        {
            skipPrompt.SetActive(false);
        }
        
        // Enable/disable objects as needed
        foreach (var obj in objectsToEnableAfterSkip)
        {
            if (obj != null) obj.SetActive(true);
        }
        
        foreach (var obj in objectsToDisableAfterSkip)
        {
            if (obj != null) obj.SetActive(false);
        }
        
        // Disable this script after skipping
        this.enabled = false;
    }
    
    public void SkipButton()
    {
        if (canSkip && isCutscenePlaying)
        {
            SkipCutscene();
        }
    }

    // Optional: Monitor the timeline state
    void OnEnable()
    {
        if (timelineDirector != null)
        {
            timelineDirector.stopped += OnCutsceneFinished;
            timelineDirector.played += OnCutscenePlayed;
        }
    }

    void OnDisable()
    {
        if (timelineDirector != null)
        {
            timelineDirector.stopped -= OnCutsceneFinished;
            timelineDirector.played -= OnCutscenePlayed;
        }
    }

    private void OnCutscenePlayed(PlayableDirector director)
    {
        OnCutsceneStart(director);
    }

    private void OnCutsceneFinished(PlayableDirector director)
    {
        isCutscenePlaying = false;
        // Don't hide prompt completely, just dim it
        if (skipPromptCanvasGroup != null)
        {
            skipPromptCanvasGroup.alpha = 0f;
        }
    }

    // Public method to manually check if skipping is available
    public bool CanSkip()
    {
        return canSkip && isCutscenePlaying;
    }

    // Public method to get cutscene status
    public bool IsCutscenePlaying()
    {
        return isCutscenePlaying;
    }
}