using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance;
    public GameDataSO gameData;

    public SCDayData[] days;
    private int currentDayIndex = 0;

    public List<SCOrderData> availableOrders = new List<SCOrderData>();
    public List<SCOrderData> activeOrders = new List<SCOrderData>();

    [Header("NPC Ayarları")]
    public GameObject npcPrefab;
    private Dictionary<string, GameObject> spawnedNPCs = new Dictionary<string, GameObject>(); // Spawn edilen NPC'leri sakla
    OrderUI orderUI = new OrderUI();
    private string missingText = "Eksik ürünler:\n";
    public delegate void OrdersUpdatedDelegate();
    public static event OrdersUpdatedDelegate OnOrdersUpdated;
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
        StartNewDay();
    }

    public void StartNewDay()
    {
        // Mevcut gün index kontrolü
        if (currentDayIndex >= days.Length)
        {
            Debug.Log("Tüm günler tamamlandı!");
            return;
        }

        // Temizlik yap
        CleanupDay();

        // Yeni gün verilerini yükle
        SCDayData currentDay = days[currentDayIndex];
        availableOrders.AddRange(currentDay.orders);

        Debug.Log($"Yeni gün başladı: {currentDay.dayName}");
        currentDayIndex++;
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

            // EKSİK OLAN KISIMLAR:
            SpawnNPCForOrder(order); // NPC oluştur
            SpawnOrderItemsAtRestaurant(order); // Sipariş itemlerini spawnla

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

        // NPC'yi deliveryPosition'da oluştur
        GameObject npc = Instantiate(npcPrefab, order.deliveryPosition, Quaternion.identity);
        MusteriNPC npcScript = npc.GetComponent<MusteriNPC>();

        if (npcScript != null)
        {
            npcScript.SetOrder(order); // Diyalog ve sipariş bilgisini NPC'ye ver
            spawnedNPCs.Add(order.orderID, npc); // NPC'yi dictionary'de sakla
        }
        else
        {
            Debug.LogError("NPCMusteri prefabında MusteriNPC scripti yok!");
        }
    }



    public bool AreAllOrdersCompleted()
    {
        // If there are available orders that haven't been accepted yet, return false
        if (availableOrders.Count > 0)
        {
            return false;
        }

        // Only return true if all accepted orders have been completed
        return activeOrders.Count == 0;
    }
    public void CompleteOrder(string orderID)
    {
        // Siparişi bul
        SCOrderData order = activeOrders.FirstOrDefault(o => o.orderID == orderID);
        if (order == null)
        {
            Debug.LogError("[OrderManager] Sipariş bulunamadı: " + orderID);
            return;
        }

        // Gereken tüm itemleri ve miktarlarını hesapla
        Dictionary<string, int> requiredItems = new Dictionary<string, int>();
        foreach (SCItem item in order.requiredItems)
        {
            if (requiredItems.ContainsKey(item.itemID))
                requiredItems[item.itemID]++;
            else
                requiredItems.Add(item.itemID, 1);
        }

        // Envanter kontrolü
        bool canComplete = true;
        foreach (var item in requiredItems)
        {
            if (Inventory.Instance.GetItemCount(item.Key) < item.Value)
            {
                canComplete = false;
                break;
            }
        }

        // Eksik varsa
        if (!canComplete)
        {
            Debug.Log("[OrderManager] Sipariş tamamlanamaz: Ürünler eksik");
            DialogueUI.Instance.ShowSimpleMessage("Ürünler eksik");
            return;
        }

        // Tüm ürünler mevcutsa
        Debug.Log("[OrderManager] Tüm ürünler mevcut, sipariş tamamlanıyor...");

        // Envanterden ürünleri sil
        foreach (var item in requiredItems)
        {
            Inventory.Instance.RemoveItem(item.Key, item.Value);
        }

        // NPC'yi temizle
        if (spawnedNPCs.TryGetValue(orderID, out GameObject npc))
        {
            npc.GetComponent<MusteriNPC>()?.CompleteOrder();
            spawnedNPCs.Remove(orderID);
        }

        // Siparişi kaldır
        activeOrders.Remove(order);

        // Ödülleri ver
        WalletManager.Instance.AddMoney(order.reward);
        gameData.upgradePoints += order.upgradePointReward;

        // Event tetikle
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
        // Tüm NPC'leri yok et
        foreach (var npcEntry in spawnedNPCs)
        {
            if (npcEntry.Value != null)
            {
                Destroy(npcEntry.Value); // NPC'yi sahneden sil
            }
        }
        availableOrders.Clear();
        spawnedNPCs.Clear(); // Dictionary'yi temizle
        activeOrders.Clear(); // Aktif siparişleri temizle
    }
}