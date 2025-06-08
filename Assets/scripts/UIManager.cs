using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private bool _isInventoryOpen;
    private bool _isUpgradeOpen;
    private bool _isPhoneUIOpen;
    private bool _isMenuOpen;
    private bool _isDialogueOpen;
    private bool _isInfoPanel;
    private bool _isBagInventoryOpen;
    private bool _isPCOpen;
    private bool _isMinimapOpen;
    private bool _isLoadGamePanelOpen;

    public delegate void UIStateChangedDelegate();
    public static event UIStateChangedDelegate OnUIStateChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CloseAllOpenPanels()
    {
        if (_isInfoPanel) FindAnyObjectByType<InfoPanelController>()?.HideOrderInfo();
        if (_isInventoryOpen) FindObjectOfType<InventoryUIController>()?.CloseInventory();
        if (_isUpgradeOpen) FindObjectOfType<UpgradeUI>()?.ClosePanel();
        if (_isPhoneUIOpen) FindObjectOfType<OrderUI>()?.CloseUI();
        if (_isDialogueOpen) FindObjectOfType<DialogueUI>()?.CloseDialogue();
        if (_isPCOpen) FindAnyObjectByType<PCUIController>()?.ClosePCUI();
        if (_isLoadGamePanelOpen) FindObjectOfType<LoadGamePanel>()?.ClosePanel();
    }
    public bool CanOpenNewUI()
    {
        // Diyalog açýksa yeni UI açýlmasýna izin verme
        if (_isDialogueOpen)
        {
            Debug.LogWarning("Yeni UI açýlamaz: Diyalog paneli þu anda açýk!");
            return false;
        }
        return true;
    }
    private void Update()
    {
        
    }
    public void SetLoadGamePanelState(bool state)
    {
        if (state && !CanOpenNewUI()) return;
        _isLoadGamePanelOpen = state;
        OnUIStateChanged?.Invoke();
    }
    // UI durum güncellemeleri (event tetikleyerek)
    public void SetBagInventoryState(bool state)
    {
        if (state && !CanOpenNewUI()) return;
        _isBagInventoryOpen = state;
        OnUIStateChanged?.Invoke();
    }

    public void SetDialogueState(bool state)
    {
        if (state && !CanOpenNewUI()) return;
        _isDialogueOpen = state;
        OnUIStateChanged?.Invoke();
    }

    public void SetInventoryState(bool state)
    {
        if (state && !CanOpenNewUI()) return;
        _isInventoryOpen = state;
        OnUIStateChanged?.Invoke();
    }

    public void SetUpgradeState(bool state)
    {
        if (state && !CanOpenNewUI()) return;
        _isUpgradeOpen = state;
        OnUIStateChanged?.Invoke();
    }

    public void SetPhoneUIState(bool state)
    {
        if (state && !CanOpenNewUI()) return;
        _isPhoneUIOpen = state;
        OnUIStateChanged?.Invoke();
    }

    public void SetMenuState(bool state)
    {
        if (state && !CanOpenNewUI()) return;
        _isMenuOpen = state;
        OnUIStateChanged?.Invoke();
    }

    public void SetPCState(bool state)
    {
        if (state && !CanOpenNewUI()) return;
        _isPCOpen = state;
        OnUIStateChanged?.Invoke();
    }

    // UI durum sorgulamalarý
    public bool IsBagInventoryOpen() => _isBagInventoryOpen;
    public bool IsDialogueOpen() => _isDialogueOpen;
    public bool IsPhoneUIOpen() => _isPhoneUIOpen;
    public bool IsInventoryOpen() => _isInventoryOpen;
    public bool IsUpgradeOpen() => _isUpgradeOpen;
    public bool IsMenuOpen() => _isMenuOpen;
    public bool IsPCOpen() => _isPCOpen;
    public bool IsLoadGamePanelOpen() => _isLoadGamePanelOpen;
    public bool IsAnyUIOpen() =>
        _isInventoryOpen ||
        _isUpgradeOpen ||
        _isPhoneUIOpen ||
        _isMenuOpen ||
        _isDialogueOpen ||
        _isBagInventoryOpen ||
        _isPCOpen ||
        _isLoadGamePanelOpen;
}
