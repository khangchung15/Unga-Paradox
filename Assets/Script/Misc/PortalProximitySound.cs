using UnityEngine;

[RequireComponent(typeof(AudioSource), typeof(Collider2D))]
public class PortalProximitySound : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true; // portal hum loops
        audioSource.playOnAwake = false; // don't start immediately
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }
    }
}