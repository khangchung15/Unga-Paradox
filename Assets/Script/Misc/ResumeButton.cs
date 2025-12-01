using UnityEngine;

public class ResumeButton : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The pause menu panel to hide")]
    public GameObject pauseMenuPanel;

    public void ResumeGame()
    {
        Debug.Log("[ResumeButton] Resuming game");
        
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        
        Time.timeScale = 1f;
        PauseController.SetPause(false);
        
        Debug.Log("[ResumeButton] Game resumed successfully - TimeScale: " + Time.timeScale);
    }
}
