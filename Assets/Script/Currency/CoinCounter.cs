using TMPro;
using UnityEngine;

public class CoinCounter : MonoBehaviour
{
    [SerializeField] CoinManager coin;
    [SerializeField] TMP_Text coinCount;
    
    private void Start()
    {
        if (coinCount == null)
        {
            Debug.LogError("Coin system from player needs to be assigned. Check PlayerUI canvas in Inspector.");
        }
        coinCount.SetText($"{coin.currentCurrency}");
    }

    public void SetValue(int newValue)
    {
        coinCount.SetText(newValue.ToString());
    }
}
