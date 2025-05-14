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
    }

    // UI durum güncellemeleri (event tetikleyerek)
    public void SetBagInventoryState(bool state)
    {
        _isBagInventoryOpen = state;
        OnUIStateChanged?.Invoke();
    }

    public void SetDialogueState(bool state)
    {
        _isDialogueOpen = state;
        OnUIStateChanged?.Invoke();
    }

    public void SetInventoryState(bool state)
    {
        _isInventoryOpen = state;
        OnUIStateChanged?.Invoke();
    }

    public void SetUpgradeState(bool state)
    {
        _isUpgradeOpen = state;
        OnUIStateChanged?.Invoke();
    }

    public void SetPhoneUIState(bool state)
    {
        _isPhoneUIOpen = state;
        OnUIStateChanged?.Invoke();
    }

    public void SetMenuState(bool state)
    {
        _isMenuOpen = state;
        OnUIStateChanged?.Invoke();
    }

    public void SetPCState(bool state)
    {
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

    public bool IsAnyUIOpen() =>
        _isInventoryOpen ||
        _isUpgradeOpen ||
        _isPhoneUIOpen ||
        _isMenuOpen ||
        _isDialogueOpen ||
        _isBagInventoryOpen ||
        _isPCOpen;
}
