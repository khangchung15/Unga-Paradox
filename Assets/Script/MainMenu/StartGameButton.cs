using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ButtonClickHandler : MonoBehaviour
{
    public Button startButton;             // Assign in Inspector
    public string sceneToLoad = "Cutscene";
    public float delay = 1.5f;
    public AudioSource clickSound;         // Optional

    void Start()
    {
        // Clear any listeners added in the Inspector (optional)
        // startButton.onClick.RemoveAllListeners();

        // Add your delay behavior
        startButton.onClick.AddListener(() => StartCoroutine(LoadSceneAfterDelay()));
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        if (clickSound != null)
            clickSound.Play();

        yield return new WaitForSeconds(delay);

        SceneManager.LoadScene(sceneToLoad);
    }
}
