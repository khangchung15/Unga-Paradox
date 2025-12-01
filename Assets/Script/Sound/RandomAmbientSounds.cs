using System.Collections;
using UnityEngine;

public class RandomAmbientSounds : MonoBehaviour
{
    [Header("Audio Clips")]
    [Tooltip("List of ambient sounds to randomly play")]
    public AudioClip[] ambientSounds;
    
    [Header("Timing Settings")]
    [Tooltip("Minimum delay between sounds (in seconds)")]
    public float minDelay = 5f;
    [Tooltip("Maximum delay between sounds (in seconds)")]
    public float maxDelay = 15f;
    
    [Header("Volume Settings")]
    [Tooltip("Minimum volume for random sounds")]
    [Range(0f, 1f)]
    public float minVolume = 0.5f;
    [Tooltip("Maximum volume for random sounds")]
    [Range(0f, 1f)]
    public float maxVolume = 1f;
    
    [Header("Randomization")]
    [Tooltip("If true, prevents the same sound from playing twice in a row")]
    public bool avoidRepetition = true;
    
    [Header("References")]
    [Tooltip("Audio source to play sounds through")]
    public AudioSource audioSource;

    private int lastPlayedIndex = -1;
    private Coroutine playbackCoroutine;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            Debug.LogError("No AudioSource found on " + gameObject.name);
            return;
        }

        if (ambientSounds == null || ambientSounds.Length == 0)
        {
            Debug.LogWarning("No ambient sounds assigned to " + gameObject.name);
            return;
        }

        audioSource.loop = false;
        audioSource.playOnAwake = false;

        StartPlayback();
    }

    void OnEnable()
    {
        if (playbackCoroutine == null && ambientSounds != null && ambientSounds.Length > 0)
        {
            StartPlayback();
        }
    }

    void OnDisable()
    {
        StopPlayback();
    }

    public void StartPlayback()
    {
        if (playbackCoroutine != null)
        {
            StopCoroutine(playbackCoroutine);
        }
        playbackCoroutine = StartCoroutine(PlayRandomSoundsRoutine());
    }

    public void StopPlayback()
    {
        if (playbackCoroutine != null)
        {
            StopCoroutine(playbackCoroutine);
            playbackCoroutine = null;
        }
        
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    private IEnumerator PlayRandomSoundsRoutine()
    {
        float initialDelay = Random.Range(minDelay, maxDelay);
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            int selectedIndex = GetRandomSoundIndex();
            AudioClip selectedClip = ambientSounds[selectedIndex];

            if (selectedClip != null)
            {
                float randomVolume = Random.Range(minVolume, maxVolume);
                audioSource.volume = randomVolume;
                
                audioSource.clip = selectedClip;
                audioSource.Play();
                
                lastPlayedIndex = selectedIndex;
                
                yield return new WaitForSeconds(selectedClip.length);
            }
            
            float waitTime = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private int GetRandomSoundIndex()
    {
        if (ambientSounds.Length == 1)
        {
            return 0;
        }

        if (avoidRepetition && ambientSounds.Length > 1)
        {
            int newIndex;
            int attempts = 0;
            const int maxAttempts = 10;
            
            do
            {
                newIndex = Random.Range(0, ambientSounds.Length);
                attempts++;
            }
            while (newIndex == lastPlayedIndex && attempts < maxAttempts);
            
            return newIndex;
        }
        else
        {
            return Random.Range(0, ambientSounds.Length);
        }
    }

    public void PlaySoundImmediately()
    {
        if (ambientSounds == null || ambientSounds.Length == 0)
        {
            return;
        }

        int selectedIndex = GetRandomSoundIndex();
        AudioClip selectedClip = ambientSounds[selectedIndex];

        if (selectedClip != null)
        {
            float randomVolume = Random.Range(minVolume, maxVolume);
            audioSource.volume = randomVolume;
            audioSource.clip = selectedClip;
            audioSource.Play();
            lastPlayedIndex = selectedIndex;
        }
    }
}
