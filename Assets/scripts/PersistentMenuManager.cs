using UnityEngine;
using UnityEngine.UI;

public class PersistentMenuManager : MonoBehaviour
{
    public static PersistentMenuManager Instance { get; private set; }

    // Panel referanslarý
    public GameObject inventoryPanel;
    public GameObject upgradePanel;
    public GameObject phonePanel;
    public GameObject persistentMenuPanel;

    // Buton referanslarý
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
        // Buton eventlerini baðla
        inventoryButton.onClick.AddListener(() => TogglePanel(inventoryPanel));
        upgradeButton.onClick.AddListener(() => TogglePanel(upgradePanel));
        phoneButton.onClick.AddListener(() => TogglePanel(phonePanel));
        closeButton.onClick.AddListener(CloseAllPanels);
    }

    private void TogglePanel(GameObject panel)
    {
        // Eðer panel zaten açýksa, hiçbir þey yapma (kapatma)
        if (panel.activeSelf)
        {
            return;
        }

        // Diðer tüm panelleri kapat
        CloseAllPanels();

        // Yeni paneli aç
        panel.SetActive(true);

        // UI Manager durumlarýný güncelle
        if (panel == inventoryPanel) UIManager.Instance.SetInventoryState(true);
        else if (panel == upgradePanel) UIManager.Instance.SetUpgradeState(true);
        else if (panel == phonePanel) UIManager.Instance.SetPhoneUIState(true);

        // Cursor'ý serbest býrak ve menüyü göster
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        persistentMenuPanel.SetActive(true);
    }

    public void CloseAllPanels()
    {
        inventoryPanel.SetActive(false);
        upgradePanel.SetActive(false);
        phonePanel.SetActive(false);

        // UI Manager durumlarýný sýfýrla
        UIManager.Instance.SetInventoryState(false);
        UIManager.Instance.SetUpgradeState(false);
        UIManager.Instance.SetPhoneUIState(false);

        // Cursor'ý kilitle
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