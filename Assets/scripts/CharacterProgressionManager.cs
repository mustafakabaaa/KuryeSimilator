using UnityEngine;
using System.Collections.Generic;

public class CharacterProgressionManager : MonoBehaviour
{
    public static CharacterProgressionManager Instance;

    [Header("Config")]
    public GameDataSO gameData;

    [Header("Debug")]
    [SerializeField] private int currentUpgradePoints = 0;

    private Dictionary<CharacterStat, float> activeStats = new Dictionary<CharacterStat, float>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeStats();
    }

    private void InitializeStats()
    {
        currentUpgradePoints = gameData.upgradePoints;

        foreach (var upgrade in gameData.availableUpgrades)
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

    public void ApplyUpgrade(StatUpgrade upgrade)
    {
        if (gameData.CanApplyUpgrade(upgrade))
        {
            gameData.ApplyUpgrade(upgrade);
            activeStats[upgrade.affectedStat] = gameData.GetCurrentStatValue(upgrade.affectedStat);

            // Eventleri tetikle
            GameEvents.Instance.TriggerStatUpdate(upgrade.affectedStat, activeStats[upgrade.affectedStat]);
            GameEvents.Instance.TriggerPointsUpdate();
        }
    }

    public void AddUpgradePoints(int amount)
    {
        gameData.upgradePoints += amount;
        GameEvents.Instance.TriggerPointsUpdate();
    }

    public int GetCurrentUpgradePoints()
    {
        return gameData.upgradePoints;
    }
}