using System;
//using UnityEditor.TerrainTools;
using UnityEngine;

public class CoinManager : MonoBehaviour//, IDataPersistence
{
    public static CoinManager Instance { get; private set; }

    public int totalCurrency = 0;
    public int currentCurrency = 0;
    public int CurrentCurrency => currentCurrency;
    private CoinCounter coinCounter;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("GameManager already exists, destroying object.");
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

    //public void LoadData(GameData data)
    //{
    //    Debug.Log("Loading currency: " + data.totalCurrency);
    //    this.currentCurrency = data.totalCurrency;
    //}

    //public void SaveData(ref GameData data)
    //{
    //    Debug.Log("Saving currency: " + this.totalCurrency);
    //    data.totalCurrency = this.totalCurrency;
    //}

    private void OnSceneChange()
    {
        if (coinCounter == null)
            coinCounter = FindAnyObjectByType<CoinCounter>();
    }

    public void Update()
    {
        OnSceneChange();
    }

    public void AddCoin(int amount)
    {
        if (amount <= 0) return;

        currentCurrency += amount;
        totalCurrency += amount;
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
