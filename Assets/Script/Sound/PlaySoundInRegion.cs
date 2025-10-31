using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundInRegion : MonoBehaviour
{
    [Header("Music Settings")]
    [Tooltip("The audio source that plays the music")]
    public AudioSource musicSource;
    [Tooltip("Fade in duration when entering region")]
    public float fadeInDuration = 2f;
    [Tooltip("Fade out duration when exiting region")]
    public float fadeOutDuration = 2f;
    [Tooltip("If true, music will loop")]
    public bool loopMusic = true;

    [Header("Region Settings")]
    [Tooltip("If true, music will stop when leaving the region")]
    public bool stopOnExit = true;
    [Tooltip("If true, multiple entries won't restart the music")]
    public bool preventRestart = true;

    private float originalVolume;
    private bool isPlaying = false;
    private Coroutine fadeCoroutine;

    void Start()
    {
        // Ensure there's a collider
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            Debug.LogWarning("MusicRegion: No Collider2D found! Adding BoxCollider2D.");
            collider = gameObject.AddComponent<BoxCollider2D>();
        }
        collider.isTrigger = true;

        // Set up audio source if not assigned
        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
            if (musicSource == null)
            {
                Debug.LogWarning("MusicRegion: No AudioSource found! Please assign one.");
            }
        }

        // Store original volume and set up audio source
        if (musicSource != null)
        {
            originalVolume = musicSource.volume;
            musicSource.volume = 0f;
            musicSource.loop = loopMusic;
            musicSource.playOnAwake = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayMusic();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && stopOnExit)
        {
            StopMusic();
        }
    }

    public void PlayMusic()
    {
        if (musicSource == null) return;

        // If already playing and preventRestart is enabled, do nothing
        if (isPlaying && preventRestart) return;

        // Stop any existing fade coroutine
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        // Start playing if not already playing
        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }

        // Fade in
        fadeCoroutine = StartCoroutine(FadeMusic(0f, originalVolume, fadeInDuration));
        isPlaying = true;

        Debug.Log("Music started: " + gameObject.name);
    }

    public void StopMusic()
    {
        if (musicSource == null) return;

        // Stop any existing fade coroutine
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        // Fade out
        fadeCoroutine = StartCoroutine(FadeMusic(musicSource.volume, 0f, fadeOutDuration, true));
        isPlaying = false;

        Debug.Log("Music stopped: " + gameObject.name);
    }

    private IEnumerator FadeMusic(float fromVolume, float toVolume, float duration, bool stopAfterFade = false)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            musicSource.volume = Mathf.Lerp(fromVolume, toVolume, t);
            yield return null;
        }

        musicSource.volume = toVolume;

        // Stop the audio source if fading out completely
        if (stopAfterFade && musicSource.volume <= 0f)
        {
            musicSource.Stop();
        }
    }

    // Optional: Manual control methods
    public void SetVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
            originalVolume = volume;
        }
    }

    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.UnPause();
        }
    }

    // Draw the trigger area in the editor
    private void OnDrawGizmos()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f); // Green with transparency
            if (collider is BoxCollider2D boxCollider)
            {
                Gizmos.DrawCube(transform.position + (Vector3)boxCollider.offset, boxCollider.size);
            }
            else if (collider is CircleCollider2D circleCollider)
            {
                Gizmos.DrawSphere(transform.position + (Vector3)circleCollider.offset, circleCollider.radius);
            }
            else if (collider is PolygonCollider2D)
            {
                Gizmos.DrawWireMesh(GetComponent<MeshFilter>().sharedMesh, transform.position);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Gizmos.color = Color.green;
            if (collider is BoxCollider2D boxCollider)
            {
                Gizmos.DrawWireCube(transform.position + (Vector3)boxCollider.offset, boxCollider.size);
            }
            else if (collider is CircleCollider2D circleCollider)
            {
                Gizmos.DrawWireSphere(transform.position + (Vector3)circleCollider.offset, circleCollider.radius);
            }
        }
    }
}