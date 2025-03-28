using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderUI : MonoBehaviour
{
    public GameObject orderUIPanel; // Siparislerin gosterilecegi UI paneli
    public Transform orderContainer; // Siparislerin eklenecegi container (ScrollView Content)
    public GameObject orderPrefab; // Order prefab'i
    public Button toggleOrdersButton; // Gecis butonu

    private bool isUIOpen = false;
    private bool showingActiveOrders = false;

    private void Start()
    {
        if (toggleOrdersButton == null)
        {
            Debug.LogError("Toggle Orders Button inspector'da atanmamis!");
            return;
        }

        orderUIPanel.SetActive(false); // UI baslangicta kapali olsun
        SetCursorState(false); // Baslangicta imleci gizle

        // Gecis butonuna tiklanma olayini ekle
        toggleOrdersButton.onClick.AddListener(ToggleOrders);
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
        }
    }
    //suan kullanilmiyor
    private void ToggleUI()
    {
        isUIOpen = !isUIOpen;
        orderUIPanel.SetActive(isUIOpen);

        // UI acildiginda imleci goster, kapandiginda gizle
        SetCursorState(isUIOpen);

        if (isUIOpen)
        {
            LoadOrders(); // UI acildiginda siparisleri yukle
        }
    }
    private void OpenUI()
    {
        // UI durumunu güncelle
        isUIOpen = true;
        orderUIPanel.SetActive(true);
        UIManager.Instance.SetPhoneUIState(true); // UIManager'a durumu bildir

        // Görsel ayarlar
        SetCursorState(true);
        LoadOrders(); // Sipariþleri yükle

        // Diðer UI iþlemleri...
    }
    private void CloseUI()
    {
        // UI durumunu güncelle
        isUIOpen = false;
        orderUIPanel.SetActive(false);
        UIManager.Instance.SetPhoneUIState(false); // UIManager'a durumu bildir

        // Görsel ayarlar
        SetCursorState(false);

        // Gerekirse temizlik iþlemleri...
    }

    private void ToggleOrders()
    {
        if (OrderManager.Instance == null)
        {
            Debug.LogError("OrderManager ornegi null!");
            return;
        }

        showingActiveOrders = !showingActiveOrders;
        LoadOrders();

        // TextMeshProUGUI kullaniliyorsa
        TextMeshProUGUI buttonText = toggleOrdersButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = showingActiveOrders ? "Show Available Orders" : "Show Active Orders";
        }
        else
        {
            Debug.LogError("ToggleOrdersButton'da TextMeshProUGUI bileseni bulunamadi!");
        }
    }

    private void LoadOrders()
    {
        // Once mevcut siparisleri temizle (yalnizca sahnedeki GameObject'leri sil)
        foreach (Transform child in orderContainer)
        {
            Destroy(child.gameObject); // GameObject'i sil
        }

        // Mevcut siparisleri yukle
        List<SCOrderData> orders = showingActiveOrders ? OrderManager.Instance.GetActiveOrders() : OrderManager.Instance.GetAvailableOrders();
        foreach (SCOrderData order in orders)
        {
            // Prefab'i instantiate et ve orderContainer'in altina ekle
            GameObject orderUI = Instantiate(orderPrefab, orderContainer);
            orderUI.GetComponent<OrderUIElement>().Setup(order);
        }
    }

    // Imlecin gorunurlugunu ve kilidini ayarla
    private void SetCursorState(bool isVisible)
    {
        Cursor.visible = isVisible; // Imleci goster veya gizle
        Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked; // Imleci kilitle veya serbest birak
    }
}