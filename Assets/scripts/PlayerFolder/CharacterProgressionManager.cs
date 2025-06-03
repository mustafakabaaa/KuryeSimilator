using UnityEngine;
using System.Collections.Generic;

public class CharacterProgressionManager : MonoBehaviour
{
    public static CharacterProgressionManager Instance;

    [Header("Config")]
    public GameDataSO gameDataSC;

    [Header("Debug")]
    [SerializeField] private int currentUpgradePoints = 0;

    private Dictionary<CharacterStat, float> activeStats = new Dictionary<CharacterStat, float>();

    private SCInventory playerInventory;
    private InventoryUIController inventoryUIController;

    private void Awake()
    {
        Debug.Log("CharacterProgressionManager Awake called"); // Bunu ekleyin

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Diðer baþlangýç ayarlarý
        playerInventory = FindObjectOfType<Inventory>()?.playerInventory;
        inventoryUIController = FindObjectOfType<InventoryUIController>();
        InitializeStats();
    }

    private void InitializeStats()
    {
        currentUpgradePoints = gameDataSC.upgradePoints;

        foreach (var upgrade in gameDataSC.availableUpgrades)
        {
            if (!activeStats.ContainsKey(upgrade.affectedStat))
            {
                activeStats[upgrade.affectedStat] = upgrade.affectedStat.baseValue;
            }
        }
    }

    public float GetCurrentStatValue(CharacterStat stat)
    {
        return activeStats.ContainsKey(stat) ? activeStats[stat] : stat.baseValue;
    }
    public void ApplyInventoryUpgrade(InventoryUpgrade upgrade)
    {
        if (gameDataSC.CanApplyUpgrade(upgrade))
        {
            gameDataSC.ApplyUpgrade(upgrade);

            // Inventory referansýný doðru þekilde al
            Inventory inventory = FindObjectOfType<Inventory>();
            if (inventory != null && inventory.playerInventory != null)
            {
                inventory.playerInventory.UnlockAdditionalSlots(upgrade.unlockedSlotsCount);

                // UI güncellemesi için doðru controller'ý bul
                InventoryUIController uiController = FindObjectOfType<InventoryUIController>();
                if (uiController != null)
                {
                    uiController.UpdateUI(inventory.playerInventory);
                    Debug.Log($"Yeni açýlan slot sayýsý: {inventory.playerInventory.maxUnlockedSlots}");
                }
            }

            GameEvents.Instance.TriggerPointsUpdate();
        }
    }
    public void ReloadStatsFromGameData()
    {
        activeStats.Clear();

        foreach (var upgrade in gameDataSC.availableUpgrades)
        {
            CharacterStat stat = upgrade.affectedStat;

            if (!activeStats.ContainsKey(stat))
            {
                float value = gameDataSC.GetCurrentStatValue(stat);
                activeStats[stat] = value;
            }
        }

        Debug.Log("Statlar yeniden yüklendi.");
    }

    public void ApplyUpgrade(StatUpgrade upgrade)
    {
        if (gameDataSC.CanApplyUpgrade(upgrade))
        {
            gameDataSC.ApplyUpgrade(upgrade);
            activeStats[upgrade.affectedStat] = gameDataSC.GetCurrentStatValue(upgrade.affectedStat);

            // Eventleri tetikle
            GameEvents.Instance.TriggerStatUpdate(upgrade.affectedStat, activeStats[upgrade.affectedStat]);
            GameEvents.Instance.TriggerPointsUpdate();
        }
    }

    public void AddUpgradePoints(int amount)
    {
        gameDataSC.upgradePoints += amount;
        GameEvents.Instance.TriggerPointsUpdate();
    }

    public int GetCurrentUpgradePoints()
    {
        return gameDataSC.upgradePoints;
    }
}