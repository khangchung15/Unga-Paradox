using TMPro;
using UnityEngine;

public class CoinCounter : MonoBehaviour
{
    [SerializeField] TMP_Text coinCount;

    private void Start()
    {
        if (coinCount == null)
        {
            Debug.LogError("Coin count text is not assigned.");
            return;
        }

        coinCount.SetText(CoinManager.Instance.currentCurrency.ToString());
    }

    public void SetValue(int newValue)
    {
        coinCount.SetText(newValue.ToString());
    }
}