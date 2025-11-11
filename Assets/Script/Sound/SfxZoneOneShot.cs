using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SfxZoneOneShot : MonoBehaviour
{
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 0.9f;

    public bool randomizePitch = true;
    public float minPitch = 0.95f;
    public float maxPitch = 1.05f;

    public string playerTag = "Player";
    public bool playOnlyOnce = false;
    public float cooldownSeconds = 0.5f;

    AudioSource src;
    float lastPlayed = -999f;
    bool playedOnce;

    void Reset()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    void Awake()
    {
        src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;
        src.spatialBlend = 0f; // 2D sound
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (playOnlyOnce && playedOnce) return;
        if (Time.time - lastPlayed < cooldownSeconds) return;
        if (clip == null) return;

        src.pitch = randomizePitch ? Random.Range(minPitch, maxPitch) : 1f;
        src.PlayOneShot(clip, volume);

        lastPlayed = Time.time;
        playedOnce = true;
    }
}
