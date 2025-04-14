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
        // 1. Siparişi aktif siparişler listesinde bul
        SCOrderData order = activeOrders.Find(o => o.orderID == orderID);
        if (order == null)
        {
            Debug.LogWarning($"Sipariş bulunamadı: {orderID}");
            return;
        }

        // 2. Gerekli tüm itemler envanterde var mı kontrol et
        List<SCItem> missingItems = new List<SCItem>();
        foreach (SCItem requiredItem in order.requiredItems)
        {
            if (!Inventory.Instance.HasItem(requiredItem.itemID))
            {
                missingItems.Add(requiredItem);
            }
        }

        // 3. Eksik item varsa uyarı göster ve işlemi iptal et
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

        // 4. Tüm itemleri envanterden sil
        foreach (SCItem requiredItem in order.requiredItems)
        {
            Inventory.Instance.RemoveItem(requiredItem.itemID);
        }

        // 5. NPC'yi yok olma moduna geçir (direkt destroy etme)
        if (spawnedNPCs.TryGetValue(orderID, out GameObject npc))
        {
            MusteriNPC npcScript = npc.GetComponent<MusteriNPC>();
            if (npcScript != null)
            {
                npcScript.CompleteOrder(); // NPC artık Update'te yok olma koşullarını kontrol edecek
            }
            else
            {
                Debug.LogError("NPC'de MusteriNPC scripti yok!");
            }

            // Dictionary'den kaldır (artık yok olma Update'te kontrol edilecek)
            spawnedNPCs.Remove(orderID);
        }

        // 6. Siparişi aktif listesinden kaldır ve event tetikle
        activeOrders.Remove(order);
        OnOrdersUpdated?.Invoke();

        Debug.Log($"Sipariş tamamlandı: {order.orderName}");
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