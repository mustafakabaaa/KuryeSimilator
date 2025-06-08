using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

[System.Serializable]
public class UpgradeSaveData
{
    public string upgradeID;
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
public class GameDataSO : ScriptableObject, ISaveable
{
    public event Action<CharacterStat> OnStatUpgraded;

    public int upgradePoints = 0;
    public List<StatUpgrade> availableUpgrades = new List<StatUpgrade>();

    private Dictionary<CharacterStat, float> activeStats = new Dictionary<CharacterStat, float>();
    private Dictionary<CharacterStat, int> upgradeCounts = new Dictionary<CharacterStat, int>();

    [Header("Save Data")]
    [SerializeField] private GameSaveData _savedUpgradeData = new GameSaveData();

    public GameSaveData savedUpgradeData
    {
        get { return _savedUpgradeData ?? (_savedUpgradeData = new GameSaveData()); }
        set { _savedUpgradeData = value; }
    }

    // FIXED: Changed from Awake to Start to ensure SaveManager is ready
    private void Start()
    {
        // Register with SaveManager if it exists
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterSystem(this);
            Debug.Log("GameDataSO registered with SaveManager");
        }
        else
        {
            Debug.LogError("SaveManager.Instance is null when trying to register GameDataSO");
        }
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

        Debug.Log($"Upgrade applied. Remaining points: {upgradePoints}");
    }

    // FIXED: Simplified SaveData method
    public void SaveData(GameData data)
    {
        Debug.Log($"GameDataSO.SaveData called - Current upgradePoints: {upgradePoints}");

        // Always save current state
        SaveUpgradeState();

        // Store both in GameData for consistency
        data.upgradeData = savedUpgradeData;
        data.upgradePoints = upgradePoints;

        Debug.Log($"Saved to GameData - upgradePoints: {data.upgradePoints}, upgradeData.upgradePoints: {data.upgradeData.upgradePoints}");
    }

    // FIXED: Simplified LoadData method
    public void LoadData(GameData data)
    {
        if (data == null)
        {
            Debug.LogWarning("GameData is null!");
            return;
        }

        Debug.Log($"GameDataSO.LoadData called - Data upgradePoints: {data.upgradePoints}");

        // Load upgrade points (prefer the main field)
        upgradePoints = data.upgradePoints;

        // Load upgrade data if exists
        if (data.upgradeData != null)
        {
            savedUpgradeData = data.upgradeData;
            // Use upgradeData.upgradePoints if main field is 0
            if (upgradePoints == 0 && data.upgradeData.upgradePoints > 0)
            {
                upgradePoints = data.upgradeData.upgradePoints;
            }
        }

        LoadUpgradeState();
        Debug.Log($"GameDataSO.LoadData completed - Final upgradePoints: {upgradePoints}");
    }

    public void ResetAllUpgrades()
    {
        activeStats.Clear();
        upgradeCounts.Clear();
    }

    public void AddUpgradePoints(int amount)
    {
        upgradePoints += amount;
        Debug.Log($"Added {amount} upgrade points. Total: {upgradePoints}");
    }

    // FIXED: Ensured proper synchronization
    public void SaveUpgradeState()
    {
        _savedUpgradeData = new GameSaveData();
        _savedUpgradeData.upgradePoints = upgradePoints; // Sync points

        foreach (var entry in upgradeCounts)
        {
            _savedUpgradeData.appliedUpgrades.Add(new UpgradeSaveData
            {
                upgradeID = entry.Key.name,
                currentLevel = entry.Value,
                currentValue = activeStats.ContainsKey(entry.Key) ? activeStats[entry.Key] : entry.Key.baseValue
            });
        }

        Debug.Log($"SaveUpgradeState - upgradePoints: {upgradePoints}, savedData.upgradePoints: {_savedUpgradeData.upgradePoints}, appliedUpgrades count: {_savedUpgradeData.appliedUpgrades.Count}");
    }

    public void LoadUpgradeState()
    {
        if (_savedUpgradeData == null)
        {
            Debug.LogWarning("No saved upgrade data found");
            return;
        }

        activeStats.Clear();
        upgradeCounts.Clear();

        foreach (var savedUpgrade in _savedUpgradeData.appliedUpgrades)
        {
            CharacterStat stat = FindStatByID(savedUpgrade.upgradeID);
            if (stat != null)
            {
                activeStats[stat] = savedUpgrade.currentValue;
                upgradeCounts[stat] = savedUpgrade.currentLevel;
                Debug.Log($"Loaded upgrade: {stat.name} Level: {savedUpgrade.currentLevel} Value: {savedUpgrade.currentValue}");
            }
            else
            {
                Debug.LogWarning($"Stat not found: {savedUpgrade.upgradeID}");
            }
        }

        Debug.Log($"LoadUpgradeState completed - upgradePoints: {upgradePoints}");
    }

    private CharacterStat FindStatByID(string id)
    {
        foreach (var upgrade in availableUpgrades)
        {
            if (upgrade.affectedStat != null && upgrade.affectedStat.name == id)
            {
                return upgrade.affectedStat;
            }
        }

        var allStats = Resources.LoadAll<CharacterStat>("Stats");
        foreach (var stat in allStats)
        {
            if (stat.name == id) return stat;
        }

        Debug.LogError($"Stat with ID '{id}' not found in available upgrades or Resources/Stats folder");
        return null;
    }
}