using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Araçlar listesi UI - Bisiklet ve motorları gösterir ve çağırma işlevi sağlar
/// </summary>
public class VehiclesUI : MonoBehaviour
{
    public static VehiclesUI Instance { get; private set; }

    [Header("UI References")]
    public Transform vehicleContainer; // Araçların ekleneceği container (ScrollView Content)
    public GameObject vehicleItemPrefab; // Araç item prefab'i

    [Header("Vehicle Icons")]
    public Sprite bicycleIcon;
    public Sprite motorcycleIcon;

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
        RefreshVehiclesList();
    }

    /// <summary>
    /// Araçlar listesini yeniden yükle
    /// </summary>
    public void RefreshVehiclesList()
    {
        // Container'ı temizle
        foreach (Transform child in vehicleContainer)
        {
            Destroy(child.gameObject);
        }

        // Bisiklet ekle
        AddBicycleItem();

        // Satın alınan motorları ekle
        if (MotorcycleShop.Instance != null)
        {
            AddMotorcycleItems();
        }
    }

    /// <summary>
    /// Bisiklet item'ını ekle
    /// </summary>
    private void AddBicycleItem()
    {
        if (vehicleItemPrefab == null) return;

        GameObject bicycleItem = Instantiate(vehicleItemPrefab, vehicleContainer);
        
        VehicleUIItem itemComponent = bicycleItem.GetComponent<VehicleUIItem>();
        if (itemComponent != null)
        {
            itemComponent.Setup("Bisiklet", bicycleIcon, VehicleType.Bicycle, -1);
        }
    }

    /// <summary>
    /// Motorcycle item'larını ekle
    /// </summary>
    private void AddMotorcycleItems()
    {
        if (vehicleItemPrefab == null) return;

        MotorcycleShop shop = FindObjectOfType<MotorcycleShop>();
        if (shop == null) return;

        for (int i = 0; i < shop.motorcycles.Length; i++)
        {
            var motorcycle = shop.motorcycles[i];
            
            if (motorcycle.isPurchased)
            {
                GameObject motorItem = Instantiate(vehicleItemPrefab, vehicleContainer);
                
                VehicleUIItem itemComponent = motorItem.GetComponent<VehicleUIItem>();
                if (itemComponent != null)
                {
                    itemComponent.Setup(motorcycle.name, motorcycle.motorcycleImage, VehicleType.Motorcycle, i);
                }
            }
        }
    }
}

/// <summary>
/// Araç tipi enum'u
/// </summary>
public enum VehicleType
{
    Bicycle,
    Motorcycle
}

