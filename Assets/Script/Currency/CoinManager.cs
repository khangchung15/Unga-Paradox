using System;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    public int currentCurrency = 0;
    public int CurrentCurrency => currentCurrency;
    private CoinCounter coinCounter;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void UpdateCoinUI()
    {
        if (coinCounter == null)
        {
            coinCounter = FindObjectOfType<CoinCounter>();
        }

        if (coinCounter != null)
        {
            coinCounter.SetValue(currentCurrency);
        }
        else
        {
            Debug.LogWarning("CoinManager: No CoinCounter found in the scene to display currency.");
        }
    }

    public void AddCoin(int amount)
    {
        if (amount <= 0) return;

        currentCurrency += amount;
        UpdateCoinUI();
    }

    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0)
            return false;

        if (currentCurrency < amount)
            return false;

        currentCurrency -= amount;
        UpdateCoinUI();
        return true;
    }
}
