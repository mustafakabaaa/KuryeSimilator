using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderUI : MonoBehaviour
{
    public GameObject orderUIPanel; // Sipariþlerin gösterileceði UI paneli
    public Transform orderContainer; // Sipariþlerin ekleneceði container (ScrollView Content)
    public GameObject orderPrefab; // Order prefab'i
    public Button toggleOrdersButton; // Geçiþ butonu
    public Button cancelOrderButton; // Ýptal butonu (Inspector'dan atanacak)

    private bool isUIOpen = false;
    private bool showingActiveOrders = false;
    public static OrderUI Instance { get; private set; }

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

    private void OnEnable()
    {
        OrderManager.OnOrdersUpdated += OnOrderListUpdated;
    }

    private void OnDisable()
    {
        OrderManager.OnOrdersUpdated -= OnOrderListUpdated;
    }

    private void Start()
    {
        if (toggleOrdersButton == null || cancelOrderButton == null)
        {
            Debug.LogError("Butonlar inspector'da atanmamýþ!");
            return;
        }

        LoadOrders();
        orderUIPanel.SetActive(false);
        SetCursorState(false);

        // Buton dinleyicilerini ekle
        toggleOrdersButton.onClick.AddListener(ToggleOrders);
        cancelOrderButton.onClick.AddListener(OnCancelOrderClicked);
        cancelOrderButton.gameObject.SetActive(false); // Baþlangýçta gizli
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (orderUIPanel.activeSelf)
            {
                CloseUI();
            }
            else if (!UIManager.Instance.IsAnyUIOpen())
            {
                OpenUI();
            }
            PersistentMenuManager.Instance.CheckPanels();
        }
    }

    private void OpenUI()
    {
        isUIOpen = true;
        orderUIPanel.SetActive(true);
        UIManager.Instance.SetPhoneUIState(true);
        SetCursorState(true);
        LoadOrders();
    }

    public void OnOrderListUpdated()
    {
        LoadOrders();
    }

    public void CloseUI()
    {
        isUIOpen = false;
        orderUIPanel.SetActive(false);
        UIManager.Instance.SetPhoneUIState(false);
        SetCursorState(false);
        FindAnyObjectByType<InfoPanelController>()?.HideOrderInfo();
    }

    private void OnCancelOrderClicked()
    {
        if (OrderManager.Instance.GetActiveOrders().Count > 0)
        {
            SCOrderData activeOrder = OrderManager.Instance.GetActiveOrders()[0];
            OrderManager.Instance.CancelOrder(activeOrder.orderID);

            // UI'yi güncelle
            OnOrderListUpdated();

            // Cancel butonunu gizle (artýk aktif sipariþ yoksa)
            cancelOrderButton.gameObject.SetActive(false);
        }
    }

    private void ToggleOrders()
    {
        showingActiveOrders = !showingActiveOrders;
        LoadOrders();

        // Cancel butonunu sadece aktif sipariþ varsa göster
        cancelOrderButton.gameObject.SetActive(
            showingActiveOrders &&
            OrderManager.Instance.GetActiveOrders().Count > 0
        );

        // Toggle buton metnini güncelle
        TextMeshProUGUI buttonText = toggleOrdersButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = showingActiveOrders ? "Show Available Orders" : "Show Active Orders";
        }
    }

    private void LoadOrders()
    {
        foreach (Transform child in orderContainer)
        {
            Destroy(child.gameObject);
        }

        List<SCOrderData> orders = showingActiveOrders ?
            OrderManager.Instance.GetActiveOrders() :
            OrderManager.Instance.GetAvailableOrders();

        foreach (SCOrderData order in orders)
        {
            GameObject orderUI = Instantiate(orderPrefab, orderContainer);
            orderUI.GetComponent<OrderUIElement>().Setup(order);
        }
    }

   

    private void SetCursorState(bool isVisible)
    {
        Cursor.visible = isVisible;
        Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}