using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class UpgradeUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private string buttonPrefabPath = "UI/UpgradeButton";
    [SerializeField] private Transform buttonParent;
    [SerializeField] private TMP_Text upgradePointsText;
    [SerializeField] private GameObject panel;

    [Header("Game Data")]
    [SerializeField] private GameDataSO gameData;

    [Header("Input Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.K;
    [SerializeField] private bool pauseGameWhenOpen = true;

    [Header("Visual Settings")]
    [SerializeField] private Color affordableColor = Color.green;
    [SerializeField] private Color maxedOutColor = Color.gray;
    [SerializeField] private Color lockedColor = Color.red;

    private List<GameObject> activeButtons = new List<GameObject>();
    private bool isPanelVisible = false;

    private void OnEnable()
    {
        GameEvents.Instance.OnUpgradePointsChanged += UpdateUI; // Direk eriþim
        InitializePanel();
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnUpgradePointsChanged -= UpdateUI;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            TogglePanel();
        }
    }

    private void InitializePanel()
    {
        panel.SetActive(false);
        CreateButtons();
    }

    public void TogglePanel()
    {
        isPanelVisible = !isPanelVisible;
        panel.SetActive(isPanelVisible);

        // Fare kontrolü
        Cursor.lockState = isPanelVisible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPanelVisible;

        // Oyun zamanýný durdurma
        if (pauseGameWhenOpen)
        {
            Time.timeScale = isPanelVisible ? 0f : 1f;
        }
    }

    private void CreateButtons()
    {
        ClearExistingButtons();

        GameObject buttonPrefab = LoadButtonPrefab();
        if (buttonPrefab == null) return;

        // Tüm upgrade tiplerini göster
        foreach (var upgrade in gameData.availableUpgrades) // OfType<InventoryUpgrade>() kaldýrýldý
        {
            GameObject buttonObj = CreateButton(buttonPrefab, upgrade);
            if (buttonObj != null)
            {
                activeButtons.Add(buttonObj);
                SetupButton(buttonObj, upgrade);
            }
        }

        UpdatePointsText();
    }

    private void ClearExistingButtons()
    {
        foreach (var button in activeButtons)
        {
            Destroy(button);
        }
        activeButtons.Clear();
    }

    private GameObject LoadButtonPrefab()
    {
        GameObject prefab = Resources.Load<GameObject>(buttonPrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"Prefab bulunamadý: {buttonPrefabPath}");
        }
        return prefab;
    }

    private GameObject CreateButton(GameObject prefab, StatUpgrade upgrade)
    {
        return Instantiate(prefab, buttonParent);
    }

    private void SetupButton(GameObject buttonObj, StatUpgrade upgrade)
    {
        // Componentleri al
        Image icon = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
        TMP_Text statName = buttonObj.transform.Find("StatName")?.GetComponent<TMP_Text>();
        TMP_Text costText = buttonObj.transform.Find("CostText")?.GetComponent<TMP_Text>();
        Button button = buttonObj.GetComponent<Button>();

        // Görselleri ata
        if (icon != null) icon.sprite = upgrade.affectedStat.icon;
        if (statName != null) statName.text = upgrade.affectedStat.statName;

        // Buton etkileþimi
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => ApplyUpgrade(upgrade));

        // Buton durumunu güncelle
        UpdateButtonVisuals(button, costText, upgrade);
    }

    private void UpdateButtonVisuals(Button button, TMP_Text costText, StatUpgrade upgrade)
    {
        bool isMaxedOut = gameData.IsUpgradeMaxedOut(upgrade);
        bool canAfford = gameData.upgradePoints >= upgrade.requiredPoints;

        if (costText != null)
        {
            costText.text = isMaxedOut ? "MAX" : $"{upgrade.requiredPoints} Points";
        }

        button.interactable = canAfford && !isMaxedOut;

        // Görsel feedback
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = isMaxedOut ? maxedOutColor :
                              canAfford ? affordableColor : lockedColor;
        }
    }

    private void ApplyUpgrade(StatUpgrade upgrade)
    {
        if (gameData.CanApplyUpgrade(upgrade))
        {
            if (upgrade is InventoryUpgrade inventoryUpgrade)
            {
                if (CharacterProgressionManager.Instance != null)
                {
                    CharacterProgressionManager.Instance.ApplyInventoryUpgrade(inventoryUpgrade);
                }
                else
                {
                    Debug.LogError("CharacterProgressionManager.Instance is null!");
                    return;
                }
            }
            else
            {
                gameData.ApplyUpgrade(upgrade);
                GameEvents.Instance?.TriggerStatUpdate(
                    upgrade.affectedStat,
                    gameData.GetCurrentStatValue(upgrade.affectedStat)
                );
            }

            RefreshAllButtons();
            GameEvents.Instance?.TriggerPointsUpdate();
        }
    }

    private void RefreshAllButtons()
    {
        for (int i = 0; i < activeButtons.Count; i++)
        {
            Button button = activeButtons[i].GetComponent<Button>();
            StatUpgrade upgrade = gameData.availableUpgrades[i];
            TMP_Text costText = activeButtons[i].transform.Find("CostText")?.GetComponent<TMP_Text>();

            UpdateButtonVisuals(button, costText, upgrade);
        }
    }

    private void UpdatePointsText()
    {
        upgradePointsText.text = $"Upgrade Points: {gameData.upgradePoints}";
    }

    private void UpdateUI()
    {
        UpdatePointsText();
        RefreshAllButtons();
    }
}