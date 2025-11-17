using System;
using UnityEngine;

public class CoinManager : MonoBehaviour, IDataPersistence
{
    public int currentCurrency;
    
    // Set coin amount here
    [SerializeField] public CoinCounter coinCounter;

    public void LoadData(GameData data)
    {
        this.currentCurrency = data.currentCurrency;
    }

    public void SaveData(ref GameData data)
    {
        data.currentCurrency = this.currentCurrency;
    }

    public void AddCoin(int amount)
    {
        currentCurrency += amount;
        Debug.Log(currentCurrency);
        coinCounter.SetValue(currentCurrency);
    }
}
