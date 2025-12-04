using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// Class which manages the game
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "_Scenes/Menu";


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        HookUpUIReferences();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HookUpUIReferences();
    }

    private void HookUpUIReferences()
    {
        if (gameOverUI == null)
        {
            var playerUI = GameObject.FindWithTag("PlayerUI");
            if (playerUI != null)
            {
                var allChildren = playerUI.GetComponentsInChildren<Transform>(true);
                foreach (var t in allChildren)
                {
                    if (t.name == "GameOverScreen")
                    {
                        gameOverUI = t.gameObject;
                        break;
                    }
                }

                if (gameOverUI == null)
                {
                    var go = GameObject.Find("GameOverScreen");
                    if (go != null)
                    {
                        gameOverUI = go;
                    }
                }
            }
            else
            {
                Debug.LogWarning("GameManager: No object with tag 'PlayerUI' found in this scene.");
            }

            if (gameOverUI == null)
            {
                Debug.LogWarning("GameManager: Could not find GameOverScreen in this scene. Make sure it is named 'GameOverScreen' and is part of the UI.");
            }
        }

        // Sync the currency UI with the current coin count if a CoinCounter exists in the scene
        var coinCounter = FindObjectOfType<CoinCounter>();
        if (coinCounter != null && CoinManager.Instance != null)
        {
            coinCounter.SetValue(CoinManager.Instance.currentCurrency);
        }
    }

    public CoinManager CoinManager => CoinManager.Instance;
    public GameObject gameOverUI;

    public void GameOver()
    {
        if (gameOverUI == null)
        {
            HookUpUIReferences();
        }

        if (gameOverUI == null)
        {
            Debug.LogError("GameManager.GameOver called but no GameOverScreen is assigned or found in the scene.");
            return;
        }

        gameOverUI.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMainMenu()
    {
        ResetPersistentState();

        // Load the main menu scene immediately
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            SceneManager.LoadScene("_Scenes/Menu");
        }
    }

    public static void ResetPersistentState()
    {
        // Destroy the persistent player if it exists
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Destroy(player);
        }

        if (CoinManager.Instance != null)
        {
            Destroy(CoinManager.Instance.gameObject);
        }

        if (Instance != null)
        {
            Destroy(Instance.gameObject);
            Instance = null;
        }
    }
}
