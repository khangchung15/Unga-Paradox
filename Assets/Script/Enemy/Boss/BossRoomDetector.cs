using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BossRoomTrigger : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject bossHealthBarRoot;
    [SerializeField] private BossHealthLatest bossHealth;

    [Header("Music")]
    [SerializeField] private AudioSource mainMusic;   
    [SerializeField] private AudioSource bossMusic;   

    private bool triggered = false;

    private void Awake()
    {
        if (bossHealthBarRoot != null)
            bossHealthBarRoot.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggered) return; 

        if (bossHealth != null && bossHealth.CurrentHealth > 0)
        {
            triggered = true;

            // Show boss health bar
            if (bossHealthBarRoot != null)
                bossHealthBarRoot.SetActive(true);

            // Stop main camera music
            if (mainMusic != null && mainMusic.isPlaying)
                mainMusic.Stop();

            if (bossMusic != null)
                bossMusic.Play();
        }
    }
}