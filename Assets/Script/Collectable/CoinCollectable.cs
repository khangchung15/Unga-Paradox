using UnityEngine;
public class CoinCollectable : MonoBehaviour, ICollectable
{
    [SerializeField] private int _coinAmount;
    [SerializeField] private AudioClip coinSound;

    private AudioSource audioSource;

    public void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void OnCollected(GameObject player)
    {
        player.GetComponent<CoinManager>().AddCoin(_coinAmount);
        if (coinSound)
            AudioSource.PlayClipAtPoint(coinSound, transform.position);
    }
}