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
    [Header("Game Over UI")]
    public GameObject gameOverUI;
    public TMP_Text gameOverText;
    
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
        
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
        
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
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
