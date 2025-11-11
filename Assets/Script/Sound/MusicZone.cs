using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class MusicZone : MonoBehaviour
{
    [Header("Zone Settings")]
    public AudioClip track;
    public float fade = 1f;
    public bool revertOnExit = true;

    [Tooltip("Higher wins when zones overlap.")]
    public int priority = 0;

    [Header("References")]
    public MusicManager manager; // drag the scene manager here

    void Reset()
    {
        var col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || manager == null || track == null) return;

        // Only switch if this zone is as high or higher than current
        // (Simple approach: always switch. If you need strict priority control,
        //  keep current zone in the manager and compare priorities.)
        manager.Play(track, fade, pushToStack: revertOnExit);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || manager == null) return;
        if (revertOnExit)
            manager.Revert(fade);
    }
}
