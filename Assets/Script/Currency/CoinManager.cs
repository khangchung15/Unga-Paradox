using System;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public int currentCurrency;
    
    // Set coin amount here
    [SerializeField] public CoinCounter coinCounter;

    public void AddCoin(int amount)
    {
        currentCurrency += amount;
        Debug.Log(currentCurrency);
        coinCounter.SetValue(currentCurrency);
    }
}
