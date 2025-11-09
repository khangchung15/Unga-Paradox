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
        if (player.GetComponent<CoinManager>().coinCounter == null)
        {
            Debug.LogError("No coin counter found. Check if player has coin manager counter. If not, drag the currency to the coin manager slot from the PlayerUI.");
        }
        player.GetComponent<CoinManager>().AddCoin(_coinAmount);
        if (coinSound)
            AudioSource.PlayClipAtPoint(coinSound, transform.position);
    }
}