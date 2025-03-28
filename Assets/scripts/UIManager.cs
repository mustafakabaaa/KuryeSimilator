using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private bool _isInventoryOpen;
    private bool _isUpgradeOpen;
    private bool _isPhoneUIOpen;
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

    // UI durum güncellemeleri
    public void SetInventoryState(bool state) => _isInventoryOpen = state;
    public void SetUpgradeState(bool state) => _isUpgradeOpen = state;
    public void SetPhoneUIState(bool state) => _isPhoneUIOpen = state;

    // UI durum sorgulamalarý
    public bool IsPhoneUIOpen() => _isPhoneUIOpen;
    public bool IsInventoryOpen() => _isInventoryOpen;
    public bool IsUpgradeOpen() => _isUpgradeOpen;
    public bool IsAnyUIOpen() => _isInventoryOpen || _isUpgradeOpen || _isPhoneUIOpen;
}