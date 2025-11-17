using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Class which manages the game
/// </summary>
public class GameManager : MonoBehaviour
{
    public GameObject gameOverUI;

    public void GameOver()
    {
        gameOverUI.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("_Scenes/Menu");
    }
}
