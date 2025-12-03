using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    [SerializeField] private PlayerUpgrades playerUpgrades;
    [SerializeField] private UpgradeType upgradeType;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text costText;
    
    [SerializeField] private TextMeshProUGUI feedbackText;

    private Button button;

    private void Awake()
    {
        if (playerUpgrades == null)
        {
            playerUpgrades = FindObjectOfType<PlayerUpgrades>();
            if (playerUpgrades == null)
            {
                Debug.LogWarning("SkillButton: Could not auto-detect PlayerUpgrades in the scene.");
            }
        }
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClickPurchase);
        }
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    public void OnClickPurchase()
    {
        if (playerUpgrades == null)
        {
            Debug.LogWarning("SkillButton: PlayerUpgrades reference is missing.");
            return;
        }

        int currentLevel = playerUpgrades.GetCurrentLevel(upgradeType);
        int maxLevel = playerUpgrades.GetMaxLevel(upgradeType);

        if (currentLevel >= maxLevel && maxLevel > 0)
        {
            SetFeedback($"Me too strong for {GetUpgradeName()}");
            return;
        }

        bool success = playerUpgrades.TryUpgrade(upgradeType);

        if (success)
        {
            int newLevel = playerUpgrades.GetCurrentLevel(upgradeType);

            if (upgradeType == UpgradeType.RainbowTrail)
            {
                SetFeedback("me get scammed");
            }
            else
            {
                SetFeedback($"me stronger in {GetUpgradeName()} to Lv {newLevel}!");
            }
        }
        else
        {
            SetFeedback("Me too poor!");
        }

        RefreshUI();
    }
    
    private void SetFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
        else
        {
            Debug.Log(message);
        }
    }

    private string GetUpgradeName()
    {
        switch (upgradeType)
        {
            case UpgradeType.Health:
                return "Health";
            case UpgradeType.Speed:
                return "Speed";
            case UpgradeType.ShieldCooldown:
                return "Shield Cooldown";
            case UpgradeType.RainbowTrail:
                return "Rainbow Trail";
            default:
                return upgradeType.ToString();
        }
    }

    public void RefreshUI()
    {
        if (playerUpgrades == null) return;

        int level = playerUpgrades.GetCurrentLevel(upgradeType);
        int maxLevel = playerUpgrades.GetMaxLevel(upgradeType);
        int nextCost = playerUpgrades.GetNextCost(upgradeType);

        if (levelText != null)
            levelText.text = $"Lv {level}/{maxLevel}";

        if (costText != null)
        {
            costText.text = nextCost < 0 ? "MAX" : nextCost.ToString();
        }
    }
}