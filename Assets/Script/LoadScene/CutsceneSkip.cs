using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneSkip : MonoBehaviour
{
    [Header("Skip Settings")]
    public string sceneToLoad;
    public GameObject skipPrompt;
    public float skipDelay = 1f;
    
    [Header("Input Settings")]
    public bool useNewInputSystem = true;
    
    private float timer = 0f;
    private bool canSkip = false;

    void Start()
    {
        timer = 0f;
        canSkip = false;
        
        if (skipPrompt != null)
        {
            skipPrompt.SetActive(true);
        }
    }
    
    void Update()
    {
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
            // New Input System check
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            var gamepad = UnityEngine.InputSystem.Gamepad.current;
            
            return (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || 
                                        keyboard.escapeKey.wasPressedThisFrame)) ||
                   (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame);
            #else
            return UnityEngine.Input.GetKeyDown(KeyCode.Space) || 
                   UnityEngine.Input.GetKeyDown(KeyCode.Escape);
            #endif
        }
        else
        {
            // Old Input System
            return UnityEngine.Input.GetKeyDown(KeyCode.Space) || 
                   UnityEngine.Input.GetKeyDown(KeyCode.Escape);
        }
    }
    
    public void SkipCutscene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
    
    public void SkipButton()
    {
        SkipCutscene();
    }
}