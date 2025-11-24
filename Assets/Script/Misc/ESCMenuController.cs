using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class EscMenuController : MonoBehaviour
{
    [Header("Menu References")]
    [Tooltip("The pause menu UI panel")]
    public GameObject pauseMenuPanel;
    
    [Header("Button References")]
    [Tooltip("Resume button to continue gameplay")]
    public Button resumeButton;
    
    [Tooltip("Restart button to reload current scene")]
    public Button restartButton;
    
    [Tooltip("Main Menu button to return to menu")]
    public Button mainMenuButton;
    
    [Header("Text References")]
    [Tooltip("Pause/Game Over text")]
    public TMP_Text pauseText;
    
    [Header("Settings")]
    [Tooltip("Name of the main menu scene")]
    public string mainMenuSceneName = "Menu";
    
    [Tooltip("Should the menu work independently or use UIManager if available")]
    public bool useUIManagerIfAvailable = true;

    [Tooltip("Pause all audio when game is paused")]
    public bool pauseAudio = true;
    
    [Tooltip("Default pause message")]
    public string pauseMessage = "PAUSED";

    [Header("Input Action")]
    [Tooltip("Input action for toggling pause (typically ESC key)")]
    public InputAction pauseAction;
    
    private bool isPaused = false;
    private UIManager uiManager;
    
    private const string ESCAPE_KEY_PATH = "<Keyboard>/escape";
    private const string GAMEPAD_START_PATH = "<Gamepad>/start";

    private void Awake()
    {
        if (pauseAction == null || pauseAction.bindings.Count == 0)
        {
            SetupDefaultPauseAction();
        }
        
        if (useUIManagerIfAvailable)
        {
            uiManager = FindObjectOfType<UIManager>();
        }
        
        SetupButtons();
    }

    private void SetupDefaultPauseAction()
    {
        pauseAction = new InputAction(name: "Pause", type: InputActionType.Button);
        pauseAction.AddBinding(ESCAPE_KEY_PATH);
        pauseAction.AddBinding(GAMEPAD_START_PATH);
    }

    private void OnEnable()
    {
        if (pauseAction != null)
        {
            pauseAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.Disable();
        }
    }

    private void Start()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        
        isPaused = false;
        SetupButtons();
    }

    private void Update()
    {
        if (pauseAction != null && pauseAction.triggered)
        {
            if (!GameManagerFuture.IsGameOver())
            {
                TogglePause();
            }
        }
    }

    private void SetupButtons()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(OnResumeClicked);
            Debug.Log("Resume button listener added. Button interactable: " + resumeButton.interactable);
        }
        else
        {
            Debug.LogError("Resume button is not assigned to EscMenuController!");
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartClicked);
            Debug.Log("Restart button listener added. Button interactable: " + restartButton.interactable);
        }
        else
        {
            Debug.LogError("Restart button is not assigned to EscMenuController!");
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            Debug.Log("Main Menu button listener added. Button interactable: " + mainMenuButton.interactable);
        }
        else
        {
            Debug.LogError("Main Menu button is not assigned to EscMenuController!");
        }
    }

    public void TogglePause()
    {
        if (GameManagerFuture.IsGameOver())
        {
            return;
        }

        if (uiManager != null && useUIManagerIfAvailable)
        {
            uiManager.TogglePause();
            return;
        }
        
        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        if (GameManagerFuture.IsGameOver())
        {
            return;
        }

        isPaused = true;
        Time.timeScale = 0f;
        
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
        
        if (resumeButton != null)
        {
            resumeButton.gameObject.SetActive(true);
        }
        
        if (pauseText != null)
        {
            pauseText.text = pauseMessage;
        }
        
        if (pauseAudio)
        {
            AudioListener.pause = true;
        }
        
        PauseController.SetPause(true);
    }

    public void ShowPauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
    }

    public void Resume()
    {
        if (GameManagerFuture.IsGameOver())
        {
            return;
        }

        isPaused = false;
        Time.timeScale = 1f;
        
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        
        if (pauseAudio)
        {
            AudioListener.pause = false;
        }
        
        PauseController.SetPause(false);
        Debug.Log("Game resumed - Audio unfrozen: " + pauseAudio);
    }

    private void OnResumeClicked()
    {
        Debug.Log("Resume button clicked");
        Resume();
    }

    private void OnRestartClicked()
    {
        Debug.Log("Restart button clicked");
        RestartLevel();
    }

    private void OnMainMenuClicked()
    {
        Debug.Log("Main Menu button clicked");
        BackToMainMenu();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        
        if (pauseAudio)
        {
            AudioListener.pause = false;
        }
        
        PauseController.SetPause(false);
        
        string currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log("Restarting scene: " + currentSceneName);
        SceneManager.LoadScene(currentSceneName);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        
        if (pauseAudio)
        {
            AudioListener.pause = false;
        }
        
        PauseController.SetPause(false);
        
        Debug.Log("Loading main menu: " + mainMenuSceneName);
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}
