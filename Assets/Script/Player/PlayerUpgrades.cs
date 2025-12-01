using UnityEngine;

public enum UpgradeType
{
    Health,
    Speed,
    ShieldCooldown,
    RainbowTrail
}

public class PlayerUpgrades : MonoBehaviour
{
    public static PlayerUpgrades Instance;

    [Header("Base Stats")]
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float baseShieldCooldown = 5f;

    [Header("Per-Level Values")]
    [SerializeField] private float healthPerLevel = 20f;
    [SerializeField] private float speedPerLevel = 1f;
    [SerializeField] private float shieldCooldownPerLevel = -0.5f; // negative to reduce cooldown

    [Header("Max Levels")]
    [SerializeField] private int maxHealthLevel = 3;
    [SerializeField] private int maxSpeedLevel = 3;
    [SerializeField] private int maxShieldLevel = 3;

    [Header("Rainbow Trail Upgrade")]
    [Tooltip("Coin cost to unlock the rainbow trail cosmetic.")]
    [SerializeField] private int rainbowTrailCost = 5;

    [Header("Coin Costs Per Level")]
    [SerializeField] private int[] healthUpgradeCosts = { 5, 10, 15 };
    [SerializeField] private int[] speedUpgradeCosts = { 5, 10, 15};
    [SerializeField] private int[] shieldUpgradeCosts = { 5, 10, 15 };

    [Header("References To Player Components")]
    [SerializeField] private Health playerHealth;    // health script
    [SerializeField] private PlayerController playerMovement; // movement script
    [SerializeField] private PlayerController playerShield;    // shield / parry script
    [SerializeField] private PlayerController playerController;
    
    [Header("Current Levels (runtime)")]
    [SerializeField] private int healthLevel = 0;
    [SerializeField] private int speedLevel = 0;
    [SerializeField] private int shieldLevel = 0;
    [SerializeField] private bool rainbowTrailUnlocked = false;

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

    [SerializeField] private bool resetUpgradesOnPlay = true;

    private void Start()
    {
        if (resetUpgradesOnPlay)
        {
            healthLevel = 0;
            speedLevel = 0;
            shieldLevel = 0;
            rainbowTrailUnlocked = false;
        }

        ApplyAllUpgradesToPlayer();
    }
    
    public bool TryUpgrade(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Health:
                return TryUpgradeHealth();

            case UpgradeType.Speed:
                return TryUpgradeSpeed();

            case UpgradeType.ShieldCooldown:
                return TryUpgradeShield();

            case UpgradeType.RainbowTrail:
                return TryUpgradeRainbowTrail();

            default:
                return false;
        }
    }

    public int GetCurrentLevel(UpgradeType type)
    {
        return type switch
        {
            UpgradeType.Health => healthLevel,
            UpgradeType.Speed => speedLevel,
            UpgradeType.ShieldCooldown => shieldLevel,
            UpgradeType.RainbowTrail => rainbowTrailUnlocked ? 1 : 0,
            _ => 0
        };
    }

    public int GetMaxLevel(UpgradeType type)
    {
        return type switch
        {
            UpgradeType.Health => maxHealthLevel,
            UpgradeType.Speed => maxSpeedLevel,
            UpgradeType.ShieldCooldown => maxShieldLevel,
            UpgradeType.RainbowTrail => 1,
            _ => 0
        };
    }

    public int GetNextCost(UpgradeType type)
    {
        int level = GetCurrentLevel(type);

        if (type == UpgradeType.RainbowTrail)
        {
            // Single unlock: if already unlocked, no more cost
            return level >= 1 ? -1 : rainbowTrailCost;
        }

        int[] costs = GetCostsArray(type);

        if (costs == null || level >= costs.Length)
            return -1;

        return costs[level];
    }
    
    private bool TryUpgradeHealth()
    {
        if (healthLevel >= maxHealthLevel)
            return false;

        int cost = healthUpgradeCosts[healthLevel];
        if (!CoinManager.Instance.TrySpendCoins(cost))
            return false;

        healthLevel++;
        ApplyHealthToPlayer();
        return true;
    }

    private bool TryUpgradeSpeed()
    {
        if (speedLevel >= maxSpeedLevel)
            return false;

        int cost = speedUpgradeCosts[speedLevel];
        if (!CoinManager.Instance.TrySpendCoins(cost))
            return false;

        speedLevel++;
        ApplySpeedToPlayer();
        return true;
    }

    private bool TryUpgradeShield()
    {
        if (shieldLevel >= maxShieldLevel)
            return false;

        int cost = shieldUpgradeCosts[shieldLevel];
        if (!CoinManager.Instance.TrySpendCoins(cost))
            return false;

        shieldLevel++;
        ApplyShieldToPlayer();
        return true;
    }

    private bool TryUpgradeRainbowTrail()
    {
        // One-time cosmetic unlock
        if (rainbowTrailUnlocked)
            return false;

        if (!CoinManager.Instance.TrySpendCoins(rainbowTrailCost))
            return false;

        rainbowTrailUnlocked = true;
        ApplyRainbowTrailToPlayer();
        return true;
    }
    
    private void ApplyAllUpgradesToPlayer()
    {
        ApplyHealthToPlayer();
        ApplySpeedToPlayer();
        ApplyShieldToPlayer();
        ApplyRainbowTrailToPlayer();
    }

    private void ApplyHealthToPlayer()
    {
        if (playerHealth == null) return;

        float newMaxHealth = baseMaxHealth + healthLevel * healthPerLevel;
        playerHealth.SetMaxHealth(newMaxHealth);
    }

    private void ApplySpeedToPlayer()
    {
        if (playerMovement == null) return;

        float newSpeed = baseMoveSpeed + speedLevel * speedPerLevel;
        playerMovement.SetBaseMovementSpeed(newSpeed);
    }

    private void ApplyShieldToPlayer()
    {
        if (playerShield == null) return;

        float newCooldown = baseShieldCooldown + shieldLevel * shieldCooldownPerLevel;
        newCooldown = Mathf.Max(0.5f, newCooldown); 
        playerShield.SetParryCooldown(newCooldown);
    }

    private void ApplyRainbowTrailToPlayer()
    {
        if (playerController == null) return;

        if (rainbowTrailUnlocked)
        {
            playerController.EnableRainbowTrail();
        }
        else
        {
            playerController.DisableRainbowTrail();
        }
    }
    
    private int[] GetCostsArray(UpgradeType type)
    {
        return type switch
        {
            UpgradeType.Health => healthUpgradeCosts,
            UpgradeType.Speed => speedUpgradeCosts,
            UpgradeType.ShieldCooldown => shieldUpgradeCosts,
            _ => null
        };
    }
}