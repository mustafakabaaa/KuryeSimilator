using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "Character/Upgrade")]
public class StatUpgrade : ScriptableObject
{
    public CharacterStat affectedStat;
    public float valueIncrease;
    public int requiredPoints;
    public int maxUpgradeCount = 3; // Yeni eklenen max limit
}