using UnityEngine;

public abstract class PotionEffect : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField] protected float aoeRadius = 3f;
    [SerializeField] protected GameObject aoePrefab;
    [SerializeField] protected AudioClip impactSound;
    [SerializeField] protected LayerMask affectedLayers;
    
    protected AudioSource audioSource;
    
    protected virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }
    
    public void TriggerEffect(Vector3 position)
    {
        ApplyEffect(position);
        SpawnAoeVisual(position);
        PlayImpactSound();
    }
    
    protected abstract void ApplyEffect(Vector3 position);
    
    protected virtual void SpawnAoeVisual(Vector3 position)
    {
        if (aoePrefab != null)
        {
            GameObject aoe = Instantiate(aoePrefab, position, Quaternion.identity);
            Destroy(aoe, 2f);
        }
    }
    
    protected virtual void PlayImpactSound()
    {
        if (audioSource != null && impactSound != null)
        {
            audioSource.PlayOneShot(impactSound);
        }
    }
}
