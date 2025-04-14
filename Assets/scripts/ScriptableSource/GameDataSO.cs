using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameData", menuName = "SC/Game/Data")]
public class GameDataSO : ScriptableObject
{
    public int upgradePoints = 0;
    public List<StatUpgrade> availableUpgrades = new List<StatUpgrade>();

    private Dictionary<CharacterStat, float> activeStats = new Dictionary<CharacterStat, float>();
    private Dictionary<CharacterStat, int> upgradeCounts = new Dictionary<CharacterStat, int>();

    public bool CanApplyUpgrade(StatUpgrade upgrade)
    {
        return upgradePoints >= upgrade.requiredPoints && !IsUpgradeMaxedOut(upgrade);
    }

    public bool IsUpgradeMaxedOut(StatUpgrade upgrade)
    {
        if (!upgradeCounts.ContainsKey(upgrade.affectedStat))
            return false;
        Debug.Log($"Upgrade uygulandý: {upgrade.affectedStat.statName} (+{upgrade.valueIncrease})"); // Bu satýrý ekleyin

        upgradePoints -= upgrade.requiredPoints;

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
    }

    public void ResetAllUpgrades()
    {
        activeStats.Clear();
        upgradeCounts.Clear();
    }
}