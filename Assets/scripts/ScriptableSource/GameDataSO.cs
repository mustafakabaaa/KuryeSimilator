using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

[System.Serializable]
public class UpgradeSaveData
{
    public string upgradeID; // Unique ID (ScriptableObject name veya custom ID)
    public int currentLevel;
    public float currentValue;
}

[System.Serializable]
public class GameSaveData
{
    public int upgradePoints;
    public List<UpgradeSaveData> appliedUpgrades = new List<UpgradeSaveData>();
}

[CreateAssetMenu(fileName = "GameData", menuName = "SC/Game/Data")]
public class GameDataSO : ScriptableObject
{
    public event Action<CharacterStat> OnStatUpgraded;

    public int upgradePoints = 0;
    public List<StatUpgrade> availableUpgrades = new List<StatUpgrade>();

    private Dictionary<CharacterStat, float> activeStats = new Dictionary<CharacterStat, float>();
    private Dictionary<CharacterStat, int> upgradeCounts = new Dictionary<CharacterStat, int>();

    [Header("Save Data")]
    [SerializeField] private GameSaveData _savedUpgradeData = new GameSaveData();

    // Property olarak kullaným
    public GameSaveData savedUpgradeData
    {
        get { return _savedUpgradeData ?? (_savedUpgradeData = new GameSaveData()); }
        set { _savedUpgradeData = value; }
    }

    public bool CanApplyUpgrade(StatUpgrade upgrade)
    {
        return upgradePoints >= upgrade.requiredPoints && !IsUpgradeMaxedOut(upgrade);
    }

    public bool IsUpgradeMaxedOut(StatUpgrade upgrade)
    {
        if (!upgradeCounts.ContainsKey(upgrade.affectedStat))
            return false;

        return upgradeCounts[upgrade.affectedStat] >= upgrade.maxUpgradeCount;
    }

    public float GetCurrentStatValue(CharacterStat stat)
    {
        if (activeStats.ContainsKey(stat))
            return activeStats[stat];
        return stat.baseValue;
    }

    public void ApplyUpgrade(StatUpgrade upgrade)
    {
        if (!CanApplyUpgrade(upgrade)) return;

        upgradePoints -= upgrade.requiredPoints;

        if (!activeStats.ContainsKey(upgrade.affectedStat))
            activeStats.Add(upgrade.affectedStat, upgrade.affectedStat.baseValue);

        activeStats[upgrade.affectedStat] += upgrade.valueIncrease;

        if (!upgradeCounts.ContainsKey(upgrade.affectedStat))
            upgradeCounts.Add(upgrade.affectedStat, 0);

        upgradeCounts[upgrade.affectedStat]++;
        OnStatUpgraded?.Invoke(upgrade.affectedStat);
    }

    public void ResetAllUpgrades()
    {
        activeStats.Clear();
        upgradeCounts.Clear();
    }

    public void AddUpgradePoints(int amount)
    {
        upgradePoints += amount;
    }

    public void SaveUpgradeState()
    {
        _savedUpgradeData = new GameSaveData();
        _savedUpgradeData.upgradePoints = upgradePoints;

        foreach (var entry in upgradeCounts)
        {
            _savedUpgradeData.appliedUpgrades.Add(new UpgradeSaveData
            {
                upgradeID = entry.Key.name,
                currentLevel = entry.Value,
                currentValue = activeStats[entry.Key]
            });
        }

    }

    // GameDataSO.cs'de LoadUpgradeState metodunu güçlendirin:
    public void LoadUpgradeState()
    {
        if (_savedUpgradeData == null)
        {
            Debug.LogWarning("No saved upgrade data found");
            return;
        }

        upgradePoints = _savedUpgradeData.upgradePoints;
        activeStats.Clear();
        upgradeCounts.Clear();

        foreach (var savedUpgrade in _savedUpgradeData.appliedUpgrades)
        {
            CharacterStat stat = FindStatByID(savedUpgrade.upgradeID);
            if (stat != null)
            {
                activeStats[stat] = savedUpgrade.currentValue;
                upgradeCounts[stat] = savedUpgrade.currentLevel;
                Debug.Log($"Loaded upgrade: {stat.name} Lvl:{savedUpgrade.currentLevel}");
            }
            else
            {
                Debug.LogWarning($"Stat not found: {savedUpgrade.upgradeID}");
            }
        }
    }

    private CharacterStat FindStatByID(string id)
    {
        // Tüm availableUpgrades'te ara
        foreach (var upgrade in availableUpgrades)
        {
            if (upgrade.affectedStat != null && upgrade.affectedStat.name == id)
            {
                return upgrade.affectedStat;
            }
        }

        // Resources'ta ara
        var allStats = Resources.LoadAll<CharacterStat>("Stats");
        foreach (var stat in allStats)
        {
            if (stat.name == id) return stat;
        }

        Debug.LogError($"Stat with ID '{id}' not found in available upgrades or Resources/Stats folder");
        return null;
    }
}