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
    public GameObject resumeButton;
    public TMP_Text gameOverText;
    
    [Header("Settings")]
    public bool pauseAudioOnGameOver = true;

    public void GameOver()
    {
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
        
        if (resumeButton != null)
        {
            resumeButton.SetActive(false);
        }
        
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
        }
        
        Debug.Log("Game Over!");
    }

    public void Restart()
    {
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
        Time.timeScale = 1f;
        
        if (pauseAudioOnGameOver)
        {
            AudioListener.pause = false;
        }
        
        PauseController.SetPause(false);
        
        SceneManager.LoadScene("Menu");
    }
}
