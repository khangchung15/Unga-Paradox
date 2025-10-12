using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CountdownSceneTransition : MonoBehaviour
{
    [Header("Countdown Settings")]
    public float countdownDuration = 5f; // Total countdown time in seconds
    public string targetSceneName = ""; // Scene to load after countdown
    
    [Header("UI References")]
    public Text countdownText; // UI Text to display countdown
    public Slider countdownSlider; // Optional slider for visual progress
    
    private float currentTime;
    private bool countdownStarted = false;

    void Start()
    {
        // Initialize countdown
        currentTime = countdownDuration;
        
        // Start the countdown automatically
        StartCountdown();
    }

    void Update()
    {
        if (!countdownStarted) return;
        
        // Update countdown timer
        currentTime -= Time.deltaTime;
        
        // Update UI
        UpdateCountdownUI();
        
        // Check if countdown finished
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            LoadTargetScene();
        }
    }

    public void StartCountdown()
    {
        countdownStarted = true;
        currentTime = countdownDuration;
        UpdateCountdownUI();
        
    }

    public void StopCountdown()
    {
        countdownStarted = false;
    }

    void UpdateCountdownUI()
    {
        // Update countdown text
        if (countdownText != null)
        {
            countdownText.text = Mathf.CeilToInt(currentTime).ToString();
            // Or for decimal display: countdownText.text = currentTime.ToString("F1");
        }
        
        // Update countdown slider
        if (countdownSlider != null)
        {
            countdownSlider.value = currentTime / countdownDuration;
        }
    }

    void LoadTargetScene()
    {

        SceneManager.LoadScene(targetSceneName);
    }

    // Public method to change target scene during runtime
    public void SetTargetScene(string newSceneName)
    {
        targetSceneName = newSceneName;
        Debug.Log($"Target scene changed to: {targetSceneName}");
    }

    // Public method to change countdown duration during runtime
    public void SetCountdownDuration(float newDuration)
    {
        countdownDuration = newDuration;
        currentTime = countdownDuration;
        UpdateCountdownUI();
    }

    // Public property to check current countdown time
    public float CurrentTime
    {
        get { return currentTime; }
    }

    // Public property to check if countdown is active
    public bool IsCountdownActive
    {
        get { return countdownStarted; }
    }
}