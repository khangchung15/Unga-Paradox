using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


/// <summary>
/// This class is meant to be used on buttons as a quick easy way to load levels (scenes)
/// </summary>
public class LevelLoadButton : MonoBehaviour
{
    /// <summary>
    /// Description:
    /// Loads a level according to the name provided
    /// Input:
    /// string levelToLoadName
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="levelToLoadName">The name of the level to load</param>
    /// 

    [SerializeField] private GameObject continueButton;
    private GameData gameData;

    private void Start()
    {
        if (!DataPersistenceManager.instance.HasGameData())
        {
            if (continueButton)
                continueButton.SetActive(false);
        }

        gameData = DataPersistenceManager.instance.getGameData();
    }

    public void OnNewGameClicked(string levelToLoadName)
    {
        // create a new game - which will initialize our game data
        DataPersistenceManager.instance.NewGame();
        // load the gameplay scene - which will in turn save the game beacuse of
        // OnSceneUnloaded() in the DataPersistence Manager
        SceneManager.LoadSceneAsync(levelToLoadName);
    }

    public void OnContinueGameClicked()
    {
        // load the next scene - which will in turn load the game beacuse of
        // OnSceneLoaded() in the DataPersistence Manager
        SceneManager.LoadSceneAsync(gameData.sceneName);
    }

    public void LoadLevelByName(string levelToLoadName)
    {
        SceneManager.LoadScene(levelToLoadName);
    }
    public void LoadLevelByNameDelayed(string levelToLoadName)
    {
        StartCoroutine(LoadLevelAfterDelay(levelToLoadName, 1.5f));
    }
    private IEnumerator LoadLevelAfterDelay(string levelToLoadName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(levelToLoadName);
    }

    public void OnExitClick()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }
}
