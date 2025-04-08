using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance;

    public SCDayData[] days;
    private int currentDayIndex = 0;

    public List<SCOrderData> availableOrders = new List<SCOrderData>();
    public List<SCOrderData> activeOrders = new List<SCOrderData>();

    [Header("NPC Ayarları")]
    public GameObject npcPrefab;
    private Dictionary<string, GameObject> spawnedNPCs = new Dictionary<string, GameObject>(); // Spawn edilen NPC'leri sakla
    OrderUI orderUI = new OrderUI();
  
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
        if (currentDayIndex >= days.Length)
        {
            Debug.Log("Tüm günler tamamlandı!");
            return;
        }

        availableOrders.Clear();
        activeOrders.Clear();

        SCDayData currentDay = days[currentDayIndex];
        foreach (var order in currentDay.orders)
        {
            availableOrders.Add(order);
        }

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

    public void AcceptOrder(string orderID)
    {
        SCOrderData order = availableOrders.Find(o => o.orderID == orderID);
        if (order != null)
        {
            activeOrders.Add(order);
            availableOrders.Remove(order);
            Debug.Log("Order accepted: " + order.orderName);

            // NPC'yi spawnla
            SpawnNPCForOrder(order);
        }
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
        return activeOrders.Count == 0;
    }
    public void CompleteOrder(string orderID)
    {
        SCOrderData order = activeOrders.Find(o => o.orderID == orderID);
        if (order == null) return;

        // 1. TÜM REQUIRED ITEM'LERİN ENVANTERDE OLUP OLMADIĞINI KONTROL ET
        List<SCItem> missingItems = new List<SCItem>();

        foreach (SCItem requiredItem in order.requiredItems)
        {
            bool itemFound = false;

            // Oyuncu envanterinde ara (SADECE itemID'ye göre kontrol)
            foreach (Slot slot in Inventory.Instance.playerInventory.inventorySlots)
            {
                if (slot.item != null && slot.item.itemID == requiredItem.itemID)
                {
                    itemFound = true;
                    break;
                }
            }

            if (!itemFound)
            {
                missingItems.Add(requiredItem);
            }
        }

        // 2. EKSİK VARSA UYARI VER
        if (missingItems.Count > 0)
        {
            string missingText = "Eksik ürünler:\n";
            foreach (var item in missingItems)
            {
                missingText += $"- {item.itemName}\n";
            }
            DialogueUI.Instance.ShowSimpleMessage(missingText);
            return;
        }

        // 3. TÜM ÜRÜNLER VARSA ENVANTERDEN SİL
        foreach (SCItem requiredItem in order.requiredItems)
        {
            Inventory.Instance.RemoveItem(requiredItem.itemID);
        }

        // 4. SİPARİŞİ TAMAMLA
        activeOrders.Remove(order);
        if (spawnedNPCs.ContainsKey(orderID))
        {
            Destroy(spawnedNPCs[orderID]);
            spawnedNPCs.Remove(orderID);
        }
        Debug.Log($"Sipariş tamamlandı: {order.orderName}");
        // Doğru şekilde OrderUI'ya erişim:
        OnOrdersUpdated?.Invoke();

    }
}