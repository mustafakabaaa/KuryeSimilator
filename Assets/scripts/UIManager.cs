using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private bool _isInventoryOpen;
    private bool _isUpgradeOpen;
    private bool _isPhoneUIOpen;
    private bool _isMenuOpen;
    private bool _isDialogueOpen; // Yeni eklenen durum
    private bool _isInfoPanel;
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
    }
   
    // UI durum güncellemeleri
    public void SetDialogueState(bool state) => _isDialogueOpen = state;
    public void SetInventoryState(bool state) => _isInventoryOpen = state;
    public void SetUpgradeState(bool state) => _isUpgradeOpen = state;
    public void SetPhoneUIState(bool state) => _isPhoneUIOpen = state;
    public void SetMenuState(bool state) => _isMenuOpen = state;

    // UI durum sorgulamalarý
    public bool IsDialogueOpen() => _isDialogueOpen;
    public bool IsPhoneUIOpen() => _isPhoneUIOpen;
    public bool IsInventoryOpen() => _isInventoryOpen;
    public bool IsUpgradeOpen() => _isUpgradeOpen;
    public bool IsMenuOpen() => _isMenuOpen;


    public bool IsAnyUIOpen() => _isInventoryOpen || _isUpgradeOpen || _isPhoneUIOpen || _isMenuOpen || _isDialogueOpen;
    private void Update()
    {
        Debug.Log("isDialogueOpen =" + _isDialogueOpen);
    }
}