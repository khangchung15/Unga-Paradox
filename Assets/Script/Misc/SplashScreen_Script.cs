using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashCanvasController : MonoBehaviour
{
    [Header("Timing")]
    public float popInDuration = 0.2f;
    public float displayDuration = 2f;
    public float fadeOutDuration = 0.5f;
    public float startDelay = 0.2f;

    [Header("Scene")]
    public string nextSceneName = "MainMenu";

    private CanvasGroup canvasGroup;
    private Transform popTransform;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        popTransform = transform;

        // Start hidden and scaled down
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        popTransform.localScale = Vector3.zero;
    }

    void Start()
    {
        StartCoroutine(ShowSplashRoutine());
    }

    IEnumerator ShowSplashRoutine()
    {
        yield return new WaitForSeconds(startDelay);
        canvasGroup.blocksRaycasts = true;

        // Pop-in scale and fade in
        float elapsed = 0f;
        while (elapsed < popInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / popInDuration);
            // Smooth scale and fade with ease out effect
            float scale = Mathf.SmoothStep(0f, 1f, t);
            canvasGroup.alpha = scale;
            popTransform.localScale = Vector3.one * scale;
            yield return null;
        }

        canvasGroup.interactable = true;
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        elapsed = 0f;
        canvasGroup.interactable = false;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        // Load next scene if set
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
