using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeButtonOnClick : MonoBehaviour
{
    public enum ButtonType { LoadScene, ExitGame }
    
    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;
    
    [Header("Button Functionality")]
    public ButtonType buttonType = ButtonType.LoadScene;
    public string sceneName = ""; // Name of the scene to load
    
    private Button button;
    private Image buttonImage;
    private Text buttonText;
    private bool isFading = false;

    void Start()
    {
        // Get components
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        buttonText = GetComponentInChildren<Text>();
        
        button.onClick.AddListener(OnButtonClicked);

    }

    void OnButtonClicked()
    {
        if (!isFading)
        {
            StartCoroutine(FadeAndExecute());
        }
    }

    IEnumerator FadeAndExecute()
    {
        isFading = true;
        
        // Disable the button to prevent multiple clicks
        if (button != null)
            button.interactable = false;
        
        float elapsedTime = 0f;
        
        // Fade out over time
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            
            // Fade the button image
            if (buttonImage != null)
            {
                Color newColor = buttonImage.color;
                newColor.a = alpha;
                buttonImage.color = newColor;
            }
            
            // Fade the text
            if (buttonText != null)
            {
                Color newColor = buttonText.color;
                newColor.a = alpha;
                buttonText.color = newColor;
            }
            
            yield return null;
        }
        
        // Ensure alpha is 0
        if (buttonImage != null)
        {
            Color finalColor = buttonImage.color;
            finalColor.a = 0f;
            buttonImage.color = finalColor;
        }
        
        if (buttonText != null)
        {
            Color finalColor = buttonText.color;
            finalColor.a = 0f;
            buttonText.color = finalColor;
        }
        
        // Wait one more frame to ensure the fade is complete
        yield return null;
        
        // Execute the appropriate action based on button type
        ExecuteButtonAction();
    }

    void ExecuteButtonAction()
    {
        switch (buttonType)
        {
            case ButtonType.LoadScene:
                LoadTargetScene();
                break;
                
            case ButtonType.ExitGame:
                ExitGame();
                break;
        }
        
        // Destroy the button GameObject after executing the action
        Destroy(gameObject);
    }

    void LoadTargetScene()
    {
        SceneManager.LoadScene(sceneName);

    }

    void ExitGame()
    {
        #if UNITY_EDITOR
        // If running in the Unity editor
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // If running in a build
        Application.Quit();
        #endif
    }

    void OnDestroy()
    {
        // Clean up the click listener to prevent memory leaks
        if (button != null)
            button.onClick.RemoveListener(OnButtonClicked);
    }
}