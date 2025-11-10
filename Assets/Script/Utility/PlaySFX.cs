using UnityEngine;

public class PlaySFX : MonoBehaviour
{
    [Header("Default SFX (optional)")]
    [Tooltip("Clip played when calling Play() from animation event without parameter.")]
    [SerializeField] private AudioClip clip;

    [Header("Audio Source (optional)")]
    [Tooltip("If assigned, its spatial settings will be copied to the temporary source used to play clips.")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Playback Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;
    [Tooltip("Randomize pitch between min and max for variety.")]
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;

    // Public parameterless method — call this from an animation event (no args)
    public void Play()
    {
        PlayClipInternal(clip, volume);
    }

    // Public method that accepts an AudioClip as an Object Reference in the animation event
    // Add an animation event and set the Object reference to an AudioClip, then choose this method.
    public void PlayClip(AudioClip clipToPlay)
    {
        PlayClipInternal(clipToPlay, volume);
    }

    // Public method that accepts a float volume parameter from the animation event (optional)
    public void PlayWithVolume(float vol)
    {
        PlayClipInternal(clip, Mathf.Clamp01(vol));
    }

    // Internal helper: create a short-lived AudioSource, set randomized pitch and play the clip.
    private void PlayClipInternal(AudioClip c, float vol)
    {
        if (c == null) return;

        float pitch = Random.Range(minPitch, maxPitch);

        // Create a temporary GameObject with an AudioSource so we don't mutate shared sources
        GameObject tempGO = new GameObject("TempSfx");
        tempGO.transform.position = transform.position;
        var tempSource = tempGO.AddComponent<AudioSource>();

        // Copy spatial / rolloff settings from configured source if available
        if (sfxSource != null)
        {
            tempSource.spatialBlend = sfxSource.spatialBlend;
            tempSource.rolloffMode = sfxSource.rolloffMode;
            tempSource.minDistance = sfxSource.minDistance;
            tempSource.maxDistance = sfxSource.maxDistance;
            tempSource.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
        }

        tempSource.clip = c;
        tempSource.volume = vol;
        tempSource.pitch = pitch;
        tempSource.Play();

        // Destroy after playback (adjusted for pitch)
        float destroyAfter = c.length / Mathf.Max(0.01f, Mathf.Abs(pitch));
        Destroy(tempGO, destroyAfter);
    }
}