using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    private GameManager gameManager;

    private void Awake()
    {
        if (GameManager.Instance != null)
        {
            gameManager = GameManager.Instance;
        }
        else
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (gameManager == null)
        {
            Debug.LogError("GameOverScreen: Could not find GameManager in the scene.");
        }
    }

    public void OnRestartButton()
    {
        if (gameManager == null)
        {
            Debug.LogWarning("GameOverScreen: No GameManager available for Restart.");
            return;
        }

        gameManager.Restart();
    }

    public void OnMainMenuButton()
    {
        if (gameManager == null)
        {
            Debug.LogWarning("GameOverScreen: No GameManager available for BackToMainMenu.");
            return;
        }

        gameManager.BackToMainMenu();
    }
}