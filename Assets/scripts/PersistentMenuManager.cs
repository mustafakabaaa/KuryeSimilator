using UnityEngine;
using UnityEngine.UI;

public class PersistentMenuManager : MonoBehaviour
{
    public static PersistentMenuManager Instance { get; private set; }

    // Panel referansları
    public GameObject inventoryPanel;
    public GameObject upgradePanel;
    public GameObject phonePanel;
    public GameObject persistentMenuPanel;

    // Buton referansları
    public Button inventoryButton;
    public Button upgradeButton;
    public Button phoneButton;
    public Button closeButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        CheckPanels();
        persistentMenuPanel.SetActive(false);
    }

    private void Start()
    {
        // Buton eventlerini bağla
        inventoryButton.onClick.AddListener(() => TogglePanel(inventoryPanel));
        upgradeButton.onClick.AddListener(() => TogglePanel(upgradePanel));
        phoneButton.onClick.AddListener(() => TogglePanel(phonePanel));
        closeButton.onClick.AddListener(CloseAllPanels);
    }

    private void TogglePanel(GameObject panel)
    {
        bool isActive = !panel.activeSelf;

        // Eğer panel zaten açıksa, hiçbir şey yapma (kapatma)
        if (panel.activeSelf)
        {
            return;
        }

        // Diğer tüm panelleri kapat
        CloseAllPanels();

        // Yeni paneli aç
        panel.SetActive(isActive);

        // UI Manager durumlarını güncelle
        if (panel == inventoryPanel) UIManager.Instance.SetInventoryState(true);
        else if (panel == upgradePanel) UIManager.Instance.SetUpgradeState(true);
        else if (panel == phonePanel) UIManager.Instance.SetPhoneUIState(true);

        // Cursor'ı serbest bırak ve menüyü göster
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        persistentMenuPanel.SetActive(true);

        // Eğer açılan panel upgrade paneliyse
        if (panel == upgradePanel && isActive)
        {
            // Sahnedeki UpgradeManager objesini bul
            GameObject upgradeManager = GameObject.Find("UpgradeManager");
            if (upgradeManager != null)
            {
                UpgradeUI upgradeUI = upgradeManager.GetComponent<UpgradeUI>();
                if (upgradeUI != null)
                {
                    upgradeUI.OnPanelOpened();
                    Debug.Log("UpgradeUI.OnPanelOpened çağrıldı.");
                }
                else
                {
                    Debug.LogWarning("UpgradeUI bileşeni UpgradeManager'da bulunamadı!");
                }
            }
            else
            {
                Debug.LogWarning("UpgradeManager objesi sahnede bulunamadı!");
            }
        }
    }


    public void CloseAllPanels()
    {
        inventoryPanel.SetActive(false);
        upgradePanel.SetActive(false);
        phonePanel.SetActive(false);

        // UI Manager durumlarını sıfırla
        UIManager.Instance.SetInventoryState(false);
        UIManager.Instance.SetUpgradeState(false);
        UIManager.Instance.SetPhoneUIState(false);

        // Cursor'ı kilitle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Menüyü gizle
        persistentMenuPanel.SetActive(false);
    }

    public void CheckPanels()
    {
        bool shouldShowMenu = inventoryPanel.activeSelf ||
                            upgradePanel.activeSelf ||
                            phonePanel.activeSelf;

        persistentMenuPanel.SetActive(shouldShowMenu);
    }
}