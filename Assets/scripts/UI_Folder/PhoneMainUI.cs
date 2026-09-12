using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PhoneMainUI : MonoBehaviour
{
    public static PhoneMainUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private Button ordersButton;
    [SerializeField] private Button closeButton;

    private PlayerInputs _playerInputs;
    private bool isOpen;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _playerInputs = new PlayerInputs();
    }

    private void OnEnable()
    {
        _playerInputs.UI.OrderKey.performed += OnPhoneKeyPressed;
        _playerInputs.UI.Enable();
    }

    private void OnDisable()
    {
        _playerInputs.UI.OrderKey.performed -= OnPhoneKeyPressed;
        _playerInputs.UI.Disable();
    }

    private void Start()
    {
        mainPanel.SetActive(false);

        ordersButton.onClick.AddListener(OpenOrders);
        closeButton.onClick.AddListener(Close);
    }

    // ?? Phone tu�una bas�ld���nda
    private void OnPhoneKeyPressed(InputAction.CallbackContext ctx)
    {
        // Sipariş listesi açıksa önce onu kapat, ana telefon menüsüne dön.
        // Telefon ekranları arasında geçerken imleci kilitleme: aynı karede lock/unlock
        // Unity'de OS imlecini kaybettirir.
        // If orders are open, close them and return to the phone home screen.
        // Do not lock the cursor while switching phone screens — lock+unlock in the
        // same click frame can hide the OS cursor.
        if (OrderUI.Instance != null && UIManager.Instance.IsPhoneUIOpen())
        {
            OrderUI.Instance.CloseUI(hideCursor: false);
            Open();
        }
        else if (isOpen)
        {
            Close();
        }
        else if (!UIManager.Instance.IsAnyUIOpen())
        {
            Open();
        }

        PersistentMenuManager.Instance?.CheckPanels();
    }

    public void Open()
    {
        isOpen = true;
        mainPanel.SetActive(true);
        UIManager.Instance.SetPhoneMain(true);
        SetCursor(true);
    }

    /// <summary>
    /// Telefonu tamamen kapatır ve imleci kilitler.
    /// Closes the phone entirely and locks the cursor.
    /// </summary>
    public void Close()
    {
        CloseInternal(hideCursor: true);
    }

    /// <summary>
    /// Ana telefon panelini kapatır.
    /// hideCursor true: oyuna dönülüyor, imleç kilitlenir.
    /// hideCursor false: başka bir telefon ekranına geçiliyor, imleç açık kalır.
    /// Closes the phone home panel. Pass hideCursor false when switching to another phone UI
    /// so Unity does not hide the cursor in the same click frame.
    /// </summary>
    private void CloseInternal(bool hideCursor)
    {
        isOpen = false;
        mainPanel.SetActive(false);
        UIManager.Instance.SetPhoneMain(false);
        if (hideCursor)
            SetCursor(false);
    }

    private void SetCursor(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    /// <summary>
    /// Sipariş listesine geçer. Ana paneli kapatır ama imleci kilitlemez;
    /// OrderUI.OpenUI imleci görünür tutar.
    /// Opens the order list. Closes the home panel without locking the cursor.
    /// </summary>
    private void OpenOrders()
    {
        CloseInternal(hideCursor: false);
        OrderUI.Instance.OpenUI();
    }
}
