using UnityEngine;

public class Coin : MonoBehaviour
{
    public int currentCurrency;
    
    // Set coin amount here
    [SerializeField] private CoinCounter coinCounter;

    public void AddCoin(int amount)
    {
        currentCurrency += amount;
        Debug.Log(currentCurrency);
        coinCounter.SetValue(currentCurrency);
    }
}