using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DelayedMusicPlayer : MonoBehaviour
{
    [Header("Music Settings")]
    public AudioClip musicClip;
    public float delayInSeconds = 2.0f;
    
    [Header("Audio Source (Optional)")]
    public AudioSource audioSource; // Assign if you have a specific AudioSource
    
    [Header("Button Trigger")]
    public Button triggerButton; // Drag your button here
    public bool playOnStart = false; // Whether to play automatically on start
    
    private bool hasPlayed = false; // Prevent multiple plays

    void Start()
    {
        // If no AudioSource is assigned, get or create one
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Set up button listener if a button is assigned
        if (triggerButton != null)
        {
            triggerButton.onClick.AddListener(PlayMusicWithDelay);
        }
        
        // Play automatically on start if configured
        if (playOnStart)
        {
            PlayMusicWithDelay();
        }
    }
    
    /// <summary>
    /// Starts playing music after the specified delay
    /// </summary>
    public void PlayMusicWithDelay()
    {
        if (!hasPlayed && musicClip != null)
        {
            StartCoroutine(PlayMusicAfterDelay());
            hasPlayed = true;
            
            // Optional: Disable button after click to prevent multiple triggers
            if (triggerButton != null)
            {
                triggerButton.interactable = false;
            }
        }
    }
    
    IEnumerator PlayMusicAfterDelay()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delayInSeconds);
        
        // Play the music
        if (musicClip != null && audioSource != null)
        {
            audioSource.clip = musicClip;
            audioSource.Play();
        }
    }
    
    /// <summary>
    /// Stops the currently playing music
    /// </summary>
    public void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
    /// <summary>
    /// Resets the player so music can be triggered again
    /// </summary>
    public void ResetPlayer()
    {
        StopMusic();
        hasPlayed = false;
        
        // Re-enable button if it exists
        if (triggerButton != null)
        {
            triggerButton.interactable = true;
        }
    }
    
    /// <summary>
    /// Changes the music clip and resets the player
    /// </summary>
    public void ChangeMusicClip(AudioClip newClip)
    {
        musicClip = newClip;
        ResetPlayer();
    }
    
    /// <summary>
    /// Sets the trigger button programmatically
    /// </summary>
    public void SetTriggerButton(Button newButton)
    {
        // Remove listener from old button if it exists
        if (triggerButton != null)
        {
            triggerButton.onClick.RemoveListener(PlayMusicWithDelay);
        }
        
        // Set new button and add listener
        triggerButton = newButton;
        if (triggerButton != null)
        {
            triggerButton.onClick.AddListener(PlayMusicWithDelay);
        }
    }
    
    /// <summary>
    /// Sets the delay time programmatically
    /// </summary>
    public void SetDelay(float newDelay)
    {
        delayInSeconds = newDelay;
    }
    
    // Clean up button listeners when destroyed
    private void OnDestroy()
    {
        if (triggerButton != null)
        {
            triggerButton.onClick.RemoveListener(PlayMusicWithDelay);
        }
    }
}