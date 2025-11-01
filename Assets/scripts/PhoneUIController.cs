using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Phone UI'nin ana controller'ı - Orders ve Vehicles sekmelerini yönetir
/// </summary>
public class PhoneUIController : MonoBehaviour
{
    public static PhoneUIController Instance { get; private set; }

    [Header("Phone Panels")]
    public GameObject phoneMainMenu; // Ana menü (Orders ve Vehicles butonları)
    public GameObject ordersPanel;   // Siparişler paneli
    public GameObject vehiclesPanel; // Araçlar paneli

    [Header("Main Menu Buttons")]
    public Button ordersButton;
    public Button vehiclesButton;
    public Button backToMainMenuButton; // Her panelde olabilir

    private enum PhoneView
    {
        MainMenu,
        Orders,
        Vehicles
    }

    private PhoneView currentView = PhoneView.MainMenu;

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

    private void Start()
    {
        // Buton eventlerini bağla
        if (ordersButton != null)
            ordersButton.onClick.AddListener(ShowOrdersView);
        
        if (vehiclesButton != null)
            vehiclesButton.onClick.AddListener(ShowVehiclesView);
        
        if (backToMainMenuButton != null)
            backToMainMenuButton.onClick.AddListener(ShowMainMenuView);

        // Başlangıçta sadece ana menüyü göster
        ShowMainMenuView();
    }

    /// <summary>
    /// Ana menüyü göster (Orders ve Vehicles butonları)
    /// </summary>
    public void ShowMainMenuView()
    {
        currentView = PhoneView.MainMenu;
        
        phoneMainMenu.SetActive(true);
        ordersPanel.SetActive(false);
        vehiclesPanel.SetActive(false);
    }

    /// <summary>
    /// Siparişler panelini göster
    /// </summary>
    public void ShowOrdersView()
    {
        currentView = PhoneView.Orders;
        
        phoneMainMenu.SetActive(false);
        ordersPanel.SetActive(true);
        vehiclesPanel.SetActive(false);

        // OrderUI'yi güncelle
        if (OrderUI.Instance != null)
        {
            OrderUI.Instance.OnOrderListUpdated();
        }
    }

    /// <summary>
    /// Araçlar panelini göster
    /// </summary>
    public void ShowVehiclesView()
    {
        currentView = PhoneView.Vehicles;
        
        phoneMainMenu.SetActive(false);
        ordersPanel.SetActive(false);
        vehiclesPanel.SetActive(true);

        // VehiclesUI'yi güncelle (oluşturacağız)
        if (VehiclesUI.Instance != null)
        {
            VehiclesUI.Instance.RefreshVehiclesList();
        }
    }

    /// <summary>
    /// Telefonu kapat
    /// </summary>
    public void ClosePhone()
    {
        ShowMainMenuView(); // Ana menüye dön
        gameObject.SetActive(false);
    }
}
