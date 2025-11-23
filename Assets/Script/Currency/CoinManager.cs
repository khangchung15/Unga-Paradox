using System;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    public int currentCurrency = 0;
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

    public void AddCoin(int amount)
    {
        currentCurrency += amount;

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
}
