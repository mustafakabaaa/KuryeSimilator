using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // Input System namespace

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
    [SerializeField] private GameObject upgradePanel;
    [Header("UI Management")]
    [SerializeField] private bool useUIManager = true; // Yeni eklenen kontrol deðiþkeni
    private List<GameObject> activeButtons = new List<GameObject>();
    private bool isPanelVisible = false;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private GameObject descriptionPanel;
    [Header("Description Style")]
    [SerializeField] private Color titleColor = Color.yellow;
    [SerializeField] private Color effectColor = Color.green;
    [SerializeField] private Color costColor = new Color(1f, 0.5f, 0f); // Turuncu
    [SerializeField] private Image descriptionIcon; // Yeni eklenen Image referansý
    private StatUpgrade currentlyDisplayedUpgrade;
    private PlayerInputs _playerInputs;

    private void Awake()
    {
        _playerInputs = new PlayerInputs();
    }
    private void OnEnable()
    {
        _playerInputs.UI.ToggleUpgrade.performed += OnToggleUpgrade;
        _playerInputs.UI.Enable();
        GameEvents.Instance.OnUpgradePointsChanged += UpdateUI; // Direk eriþim
        InitializePanel();
        UpdateUI();
    }

    private void OnDisable()
    {
        _playerInputs.UI.ToggleUpgrade.performed -= OnToggleUpgrade;
        _playerInputs.UI.Disable();
        GameEvents.Instance.OnUpgradePointsChanged -= UpdateUI;
    }
    private void OnToggleUpgrade(InputAction.CallbackContext context)
    {
        if (useUIManager)
        {
            if (panel.activeSelf)
            {
                panel.SetActive(false);
                UIManager.Instance.SetUpgradeState(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (!UIManager.Instance.IsAnyUIOpen())
            {
                panel.SetActive(true);
                UIManager.Instance.SetUpgradeState(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                UpdateUI();

            }
            PersistentMenuManager.Instance.CheckPanels();
        }
        else
        {
            LegacyTogglePanel();
        }
    }
    private void Update()
    {
       
    }

    public bool IsPanelActive() => panel.activeSelf;
   

    private void InitializePanel()
    {
        panel.SetActive(false);
        CreateButtons();
        UpdateUI(); // <-- daha kapsamlý güncelleme


    }

    private void LegacyTogglePanel()
    {
        bool newState = !panel.activeSelf;
        panel.SetActive(newState);

        // Fare kontrolü
        Cursor.lockState = newState ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = newState;

        // Oyun zamanýný durdurma
        if (pauseGameWhenOpen)
        {
            Time.timeScale = newState ? 0f : 1f;
        }
    }

    // Yeni panel kapatma metodu
    public void ClosePanel()
    {
        panel.SetActive(false);

        if (!useUIManager)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            UIManager.Instance.SetUpgradeState(false);

            if (pauseGameWhenOpen)
            {
                Time.timeScale = 1f;
            }
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
        // Componentleri bul
        Image icon = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
        TMP_Text statName = buttonObj.transform.Find("StatName")?.GetComponent<TMP_Text>();
        TMP_Text costText = buttonObj.transform.Find("CostText")?.GetComponent<TMP_Text>();
        Button button = buttonObj.GetComponent<Button>();

        // Null kontrolleri
        if (icon == null || statName == null || costText == null || button == null)
        {
            Debug.LogError($"Buton componentleri eksik: {buttonObj.name}");
            return;
        }

        // Görsel atamalarý
        icon.sprite = upgrade.icon != null ? upgrade.icon : upgrade.affectedStat.icon;
        statName.text = upgrade.affectedStat.statName;
        costText.text = gameData.IsUpgradeMaxedOut(upgrade) ? "MAX" : $"{upgrade.requiredPoints}";

        // Buton etkileþimi
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => ApplyUpgrade(upgrade));

        // Event Trigger ayarlarý
        EventTrigger trigger = buttonObj.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = buttonObj.AddComponent<EventTrigger>();
        }
        else
        {
            trigger.triggers.Clear();
        }

        // Fare üzerine gelince açýklama göster
        EventTrigger.Entry pointerEnter = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        pointerEnter.callback.AddListener((data) => ShowDescription(upgrade));
        trigger.triggers.Add(pointerEnter);

        // Fare ayrýlýnca açýklamayý gizle
        EventTrigger.Entry pointerExit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        pointerExit.callback.AddListener((data) => HideDescription());
        trigger.triggers.Add(pointerExit);

        // Buton görünümünü güncelle
        UpdateButtonVisuals(button, costText, upgrade);
    }
    private void ShowDescription(StatUpgrade upgrade)
    {
        currentlyDisplayedUpgrade = upgrade;
        // Image componentine sprite'ý ata
        if (descriptionIcon != null)
        {
            descriptionIcon.sprite = upgrade.icon != null ? upgrade.icon : upgrade.affectedStat.icon;
            descriptionIcon.gameObject.SetActive(true);
        }

        // Max seviye kontrolü
        bool isMaxedOut = gameData.IsUpgradeMaxedOut(upgrade);
        string maxLevelText = isMaxedOut ? "\n\n<color=#FF0000><b>MAX SEVÝYEDE</b></color>" : "";

        // Metni oluþtur
        string formattedText = $"<color=#{ColorUtility.ToHtmlStringRGBA(titleColor)}><b>{upgrade.affectedStat.statName}</b></color>\n" +
                              $"{upgrade.description}\n\n" +
                              $"<color=#{ColorUtility.ToHtmlStringRGBA(effectColor)}>";

        if (upgrade is InventoryUpgrade invUpgrade)
        {
            formattedText += $"• +{invUpgrade.unlockedSlotsCount} Envanter Slotu\n";
        }
        else
        {
            formattedText += $"• +{upgrade.affectedStat.baseValue} {upgrade.affectedStat.statName}\n";
        }

        formattedText += $"</color><color=#{ColorUtility.ToHtmlStringRGBA(costColor)}>" +
                        $"Gerekli Puan: {(isMaxedOut ? "-" : upgrade.requiredPoints.ToString())}</color>" +
                        maxLevelText;

        descriptionText.text = formattedText;
        descriptionPanel.SetActive(true);
    }

    private void HideDescription()
    {
        if (descriptionPanel != null)
        {
            descriptionPanel.SetActive(false);
            if (descriptionIcon != null)
                descriptionIcon.gameObject.SetActive(false);
        }
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
        Debug.Log($"Button for {upgrade.affectedStat.statName} - Maxed: {isMaxedOut}, Interactable: {button.interactable}");

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
        if (descriptionPanel.activeSelf && currentlyDisplayedUpgrade == upgrade)
        {
            ShowDescription(upgrade);
        }
    }
    public void ForceRefreshUI()
    {
        CreateButtons(); // Butonlarý baþtan oluþtur
        UpdatePointsText(); // Puanlarý güncelle
    }
    private void RefreshAllButtons()
    {
        for (int i = 0; i < activeButtons.Count; i++)
        {
            GameObject buttonObj = activeButtons[i];
            Button button = buttonObj.GetComponent<Button>();
            TMP_Text costText = buttonObj.transform.Find("CostText")?.GetComponent<TMP_Text>();
            StatUpgrade upgrade = gameData.availableUpgrades[i];

            bool isMaxedOut = gameData.IsUpgradeMaxedOut(upgrade);
            bool canAfford = gameData.upgradePoints >= upgrade.requiredPoints;

            // Görsel ve metin güncelle
            UpdateButtonVisuals(button, costText, upgrade);

            // OnClick temizle
            button.onClick.RemoveAllListeners();

            // Týklanabilirliði ve listener’ý sadece uygunsa ata
            if (!isMaxedOut && canAfford)
            {
                button.interactable = true;
                button.onClick.AddListener(() => ApplyUpgrade(upgrade));
            }
            else
            {
                button.interactable = false;
            }
        }
    }


    public void OnPanelOpened()
    {
        Debug.Log("UpgradeUI: OnPanelOpened called");

        RefreshAllButtons();
        UpdatePointsText();
    }

    private void UpdatePointsText()
    {
        upgradePointsText.text = $" {gameData.upgradePoints}";
    }

    private void UpdateUI()
    {
        UpdatePointsText();
        RefreshAllButtons();
    }
}