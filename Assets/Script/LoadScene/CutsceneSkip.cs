using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class CutsceneSkip : MonoBehaviour
{
    [Header("Skip Settings")]
    public PlayableDirector timelineDirector;
    public GameObject skipPrompt;
    public float skipDelay = 1f;
    
    [Header("Skip Mode")]
    public bool loadNewScene = false;
    public string sceneToLoad;
    
    [Header("Optional: Objects to Enable/Disable")]
    public GameObject[] objectsToEnableAfterSkip;
    public GameObject[] objectsToDisableAfterSkip;
    
    [Header("Input Settings")]
    public bool useNewInputSystem = true;
    
    private float timer = 0f;
    private bool canSkip = false;
    private bool hasSkipped = false;
    
    void Start()
    {
        timer = 0f;
        canSkip = false;
        hasSkipped = false;
        
        // Auto-find timeline if not assigned
        if (timelineDirector == null)
        {
            timelineDirector = FindObjectOfType<PlayableDirector>();
        }
        
        if (skipPrompt != null)
        {
            skipPrompt.SetActive(true);
        }
    }
    
    void Update()
    {
        if (hasSkipped) return;
        
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
    
    bool CheckSkipInput()
    {
        if (useNewInputSystem)
        {
            #if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            var gamepad = UnityEngine.InputSystem.Gamepad.current;
            
            return (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || 
                                        keyboard.escapeKey.wasPressedThisFrame)) ||
                   (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame);
            #else
            return Input.GetKeyDown(KeyCode.Space) || 
                   Input.GetKeyDown(KeyCode.Escape);
            #endif
        }
        else
        {
            return Input.GetKeyDown(KeyCode.Space) || 
                   Input.GetKeyDown(KeyCode.Escape);
        }
    }
    
    public void SkipCutscene()
    {
        if (hasSkipped) return;
        hasSkipped = true;
        
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
            timelineDirector.gameObject.SetActive(false);
        }
        
        // Hide skip prompt
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
        if (canSkip)
        {
            SkipCutscene();
        }
    }
}