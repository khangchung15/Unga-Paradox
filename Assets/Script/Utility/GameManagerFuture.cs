using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManagerFuture : MonoBehaviour
{
    [Header("Game Over Settings")]
    public GameObject resumeButton;
    public TMP_Text pauseText;
    public string gameOverMessage = "GAME OVER";
    public string pauseMessage = "PAUSED";
    
    [Header("Settings")]
    public bool pauseAudioOnGameOver = true;

    private static bool isGameOver = false;

    public static bool IsGameOver()
    {
        return isGameOver;
    }

    private void Awake()
    {
        isGameOver = false;
    }

    public void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;
        
        if (pauseAudioOnGameOver)
        {
            AudioListener.pause = true;
        }
        
        PauseController.SetPause(true);
        
        if (resumeButton != null)
        {
            resumeButton.SetActive(false);
        }
        
        if (pauseText != null)
        {
            pauseText.text = gameOverMessage;
        }
        
        EscMenuController escController = FindObjectOfType<EscMenuController>();
        if (escController != null)
        {
            escController.ShowPauseMenu();
        }
        
        Debug.Log("Game Over!");
    }

    public void Restart()
    {
        isGameOver = false;
        Time.timeScale = 1f;
        
        if (pauseAudioOnGameOver)
        {
            AudioListener.pause = false;
        }
        
        PauseController.SetPause(false);
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMainMenu()
    {
        isGameOver = false;
        Time.timeScale = 1f;
        
        if (pauseAudioOnGameOver)
        {
            AudioListener.pause = false;
        }
        
        PauseController.SetPause(false);
        
        SceneManager.LoadScene("Menu");
    }
}
