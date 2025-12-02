using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public string sceneToLoad;
    public string entryPointName;   // name of spawn point in the next scene

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SceneEntryManager.lastEntryPoint = entryPointName;
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
