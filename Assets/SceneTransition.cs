using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public string sceneToLoad;
    public string entryPointName;  // Name of spawn object in the next scene

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check the ROOT object’s tag (handles child colliders)
        if (other.transform.root.CompareTag("Player"))
        {
            SceneEntryManager.lastEntryPoint = entryPointName;
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
