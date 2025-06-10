using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using System;

public class OrderManager : MonoBehaviour, ISaveable
{
    public static OrderManager Instance;
    public GameDataSO gameData;

    public SCDayData[] days;
    private int currentDayIndex = 0;

    public List<SCOrderData> availableOrders = new List<SCOrderData>();
    public List<SCOrderData> activeOrders = new List<SCOrderData>();

    // Tamamlanan siparişlerin takibi için
    private HashSet<string> completedOrderIDs = new HashSet<string>();

    [Header("NPC Ayarları")]
    public GameObject npcPrefab;
    private Dictionary<string, GameObject> spawnedNPCs = new Dictionary<string, GameObject>();
    OrderUI orderUI = new OrderUI();
    private string missingText = "Eksik ürünler:\n";

    public delegate void OrdersUpdatedDelegate();
    public static event OrdersUpdatedDelegate OnOrdersUpdated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // SaveManager'a kayıt ol
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.RegisterSystem(this);
                Debug.Log("OrderManager SaveManager'a kaydedildi");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // LightManager'ın gün döngüsü eventini dinle
        LightManager.OnDayCycleCompleted += OnDayCompleted;
    }

    private void OnDisable()
    {
        // Event dinlemeyi durdur
        LightManager.OnDayCycleCompleted -= OnDayCompleted;
    }

    // OrderManager.cs içinde bu metodu güncelleyin
    public void OnDayCompleted()
    {
        Debug.Log("Gün tamamlandı! Yeni güne geçiliyor...");

        // Clear completed orders for the new day
        completedOrderIDs.Clear();

        // Move to the next day
        currentDayIndex++;

        // Start the new day
        StartNewDay();

        // Save the game state after day transition
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
    }
    public void ResetAllOrders()
    {
        completedOrderIDs.Clear();
        currentDayIndex = 0;
        CleanupDay();
        StartNewDay();
    }

    public void SaveData(GameData data)
    {
        if (data.orderData == null)
        {
            data.orderData = new OrderSaveData();
        }

        data.orderData.completedOrderIDs = completedOrderIDs.ToList();
        data.orderData.currentDayIndex = currentDayIndex;

        Debug.Log($"OrderManager saved - Day: {currentDayIndex}, Completed orders: {completedOrderIDs.Count}");
    }

    public void LoadData(GameData data)
    {
        if (data.orderData != null)
        {
            completedOrderIDs = new HashSet<string>(data.orderData.completedOrderIDs);
            currentDayIndex = data.orderData.currentDayIndex;

            Debug.Log($"OrderManager loaded - Day: {currentDayIndex}, Completed orders: {completedOrderIDs.Count}");

            // Start the loaded day
            StartNewDay();
        }
    }

 

    private void Start()
    {
        StartNewDay();
    }

    public void StartNewDay()
    {
        // Check if we've completed all days
        if (currentDayIndex >= days.Length)
        {
            Debug.Log("Tüm günler tamamlandı!");

            // Optional: Loop back to day 0 or handle game completion
            // currentDayIndex = 0;
            return;
        }

        // Clean up previous day
        CleanupDay();

        // Load the current day's data
        SCDayData currentDay = days[currentDayIndex];

        // Add only orders that haven't been completed
        foreach (SCOrderData order in currentDay.orders)
        {
            if (!completedOrderIDs.Contains(order.orderID))
            {
                availableOrders.Add(order);
            }
        }

        Debug.Log($"Yeni gün başladı: {currentDay.dayName} (Gün {currentDayIndex + 1}/{days.Length}). {availableOrders.Count} yeni sipariş mevcut.");
    }

    public List<SCOrderData> GetAvailableOrders()
    {
        return availableOrders;
    }

    public List<SCOrderData> GetActiveOrders()
    {
        return activeOrders;
    }

    public bool AcceptOrder(string orderID)
    {
        if (activeOrders.Count > 0)
        {
            Debug.Log("Zaten aktif bir sipariş var! Önce onu tamamla.");
            return false;
        }

        SCOrderData order = availableOrders.Find(o => o.orderID == orderID);
        if (order != null)
        {
            availableOrders.Remove(order);
            activeOrders.Add(order);

            SpawnNPCForOrder(order);
            SpawnOrderItemsAtRestaurant(order);

            OnOrdersUpdated?.Invoke();
            return true;
        }
        return false;
    }

    private void SpawnNPCForOrder(SCOrderData order)
    {
        if (npcPrefab == null)
        {
            Debug.LogError("NPC Prefabı atanmamış!");
            return;
        }

        GameObject npc = Instantiate(npcPrefab, order.deliveryPosition, Quaternion.identity);
        MusteriNPC npcScript = npc.GetComponent<MusteriNPC>();

        if (npcScript != null)
        {
            npcScript.SetOrder(order);
            spawnedNPCs.Add(order.orderID, npc);
        }
        else
        {
            Debug.LogError("NPCMusteri prefabında MusteriNPC scripti yok!");
        }
    }

    public bool AreAllOrdersCompleted()
    {
        if (availableOrders.Count > 0)
        {
            return false;
        }
        return activeOrders.Count == 0;
    }

    public void CompleteOrder(string orderID)
    {
        SCOrderData order = activeOrders.FirstOrDefault(o => o.orderID == orderID);
        if (order == null)
        {
            Debug.LogError("[OrderManager] Sipariş bulunamadı: " + orderID);
            return;
        }

        Dictionary<string, int> requiredItems = new Dictionary<string, int>();
        foreach (SCItem item in order.requiredItems)
        {
            if (requiredItems.ContainsKey(item.itemID))
                requiredItems[item.itemID]++;
            else
                requiredItems.Add(item.itemID, 1);
        }

        bool canComplete = true;
        foreach (var item in requiredItems)
        {
            if (Inventory.Instance.GetItemCount(item.Key) < item.Value)
            {
                canComplete = false;
                break;
            }
        }

        if (!canComplete)
        {
            Debug.Log("[OrderManager] Sipariş tamamlanamaz: Ürünler eksik");
            DialogueUI.Instance.ShowSimpleMessage("Ürünler eksik");
            return;
        }

        Debug.Log("[OrderManager] Tüm ürünler mevcut, sipariş tamamlanıyor...");

        foreach (var item in requiredItems)
        {
            Inventory.Instance.RemoveItem(item.Key, item.Value);
        }

        if (spawnedNPCs.TryGetValue(orderID, out GameObject npc))
        {
            npc.GetComponent<MusteriNPC>()?.CompleteOrder();
            spawnedNPCs.Remove(orderID);
        }

        // Siparişi tamamlanmış olarak işaretle
        completedOrderIDs.Add(orderID);
        activeOrders.Remove(order);

        WalletManager.Instance.AddMoney(order.reward);
        gameData.AddUpgradePoints(order.upgradePointReward);
        GameEvents.Instance?.TriggerPointsUpdate();

        OnOrdersUpdated?.Invoke();

        Debug.Log($"[OrderManager] Sipariş tamamlandı: {order.orderName}");
    }

    private void SpawnOrderItemsAtRestaurant(SCOrderData order)
    {
        if (order.requiredItems == null || order.requiredItems.Length == 0) return;

        for (int i = 0; i < order.requiredItems.Length; i++)
        {
            SCItem requiredItem = order.requiredItems[i];
            if (requiredItem.itemPrefab == null) continue;

            Vector3 spawnPos = RestaurantManager.Instance.GetSpawnPosition(
                order.restaurantID,
                i,
                order.requiredItems.Length
            );

            GameObject itemObj = Instantiate(requiredItem.itemPrefab, spawnPos, Quaternion.identity);
            Item itemComponent = itemObj.GetComponent<Item>();
            if (itemComponent != null)
            {
                itemComponent.item = requiredItem;
            }
            Debug.LogWarning("olusturuldu");
        }
    }

    public void CleanupDay()
    {
        foreach (var npcEntry in spawnedNPCs)
        {
            if (npcEntry.Value != null)
            {
                Destroy(npcEntry.Value);
            }
        }
        availableOrders.Clear();
        spawnedNPCs.Clear();
        activeOrders.Clear();
    }

    public void CancelOrder(string orderID)
    {
        SCOrderData order = activeOrders.FirstOrDefault(o => o.orderID == orderID);
        if (order == null)
        {
            Debug.LogError("[OrderManager] İptal edilecek sipariş bulunamadı: " + orderID);
            return;
        }

        if (spawnedNPCs.TryGetValue(orderID, out GameObject npc))
        {
            Destroy(npc);
            spawnedNPCs.Remove(orderID);
        }

        activeOrders.Remove(order);
        WalletManager.Instance.SpendMoney(order.reward);
        OnOrdersUpdated?.Invoke();
        Debug.Log($"[OrderManager] Sipariş iptal edildi ve listeden kaldırıldı: {order.orderName}");
    }

    // Debug için - günü manuel olarak tamamla
    [ContextMenu("Complete Day Manually")]
    public void CompleteDayManually()
    {
        OnDayCompleted();
    }
}