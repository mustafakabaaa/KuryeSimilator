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
        // Eğer OrderUI açıksa, önce onu kapat ve Phone'u aç
        if (OrderUI.Instance != null && UIManager.Instance.IsPhoneUIOpen())
        {
            OrderUI.Instance.CloseUI();
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

    public void Close()
    {
        isOpen = false;
        mainPanel.SetActive(false);
        UIManager.Instance.SetPhoneMain(false);
        SetCursor(false);
    }

    private void SetCursor(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    // ?? Order butonu
    private void OpenOrders()
    {
        Close(); // Ana men�y� kapat
        OrderUI.Instance.OpenUI(); // Order UI'yi a�
    }
}
