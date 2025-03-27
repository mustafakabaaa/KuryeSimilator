using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text statNameText;
    [SerializeField] private TMP_Text currentValueText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button upgradeButton;

    private StatUpgrade currentUpgrade;

    public void Initialize(StatUpgrade upgrade)
    {
        currentUpgrade = upgrade;

        icon.sprite = upgrade.affectedStat.icon;
        statNameText.text = upgrade.affectedStat.statName;
        //currentValueText.text = CharacterProgressionManager.Instance.GetCurrentValue(upgrade.affectedStat).ToString("F1");
        costText.text = $"{upgrade.requiredPoints} Point";

        upgradeButton.onClick.AddListener(OnUpgradeClicked);
    }

    public void UpdateValue(float newValue)
    {
        currentValueText.text = newValue.ToString("F1");
    }

    private void OnUpgradeClicked()
    {
        CharacterProgressionManager.Instance.ApplyUpgrade(currentUpgrade);
    }
}