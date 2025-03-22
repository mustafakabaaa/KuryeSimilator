using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance; // Singleton örneði
   
    public SCDayData[] days; // Tüm günlerin SC'leri
    private int currentDayIndex = 0; // Þu anki günün index'i

    public List<SCOrderData> availableOrders = new List<SCOrderData>(); // Mevcut sipariþler
    public List<SCOrderData> activeOrders = new List<SCOrderData>(); // Aktif sipariþler

    private Dictionary<string, DeliveryPoint> deliveryPoints = new Dictionary<string, DeliveryPoint>(); // Teslimat noktalarý

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
        StartNewDay(); // Oyun baþladýðýnda ilk günü baþlat
    }

    // Yeni bir gün baþlat
    public void StartNewDay()
    {
        if (currentDayIndex >= days.Length)
        {
            Debug.Log("Tüm günler tamamlandý!");
            return;
        }

        // Mevcut ve aktif orderlarý temizle
        availableOrders.Clear();
        activeOrders.Clear();

        // Þu anki günün orderlarýný yükle
        SCDayData currentDay = days[currentDayIndex];
        foreach (var order in currentDay.orders)
        {
            availableOrders.Add(order);
        }

        Debug.Log($"Yeni gün baþladý: {currentDay.dayName}");
        currentDayIndex++; // Bir sonraki güne geç
    }
    // Mevcut sipariþleri döndür
    public List<SCOrderData> GetAvailableOrders()
    {
        return availableOrders;
    }

    // Aktif sipariþleri döndür
    public List<SCOrderData> GetActiveOrders()
    {
        return activeOrders;
    }

    // Teslimat noktasýný kaydet
    public void RegisterDeliveryPoint(string orderID, DeliveryPoint deliveryPoint)
    {
        if (!deliveryPoints.ContainsKey(orderID))
        {
            deliveryPoints.Add(orderID, deliveryPoint);
            deliveryPoint.gameObject.SetActive(false); // Baþlangýçta teslimat noktasýný devre dýþý býrak
        }
    }

    // Sipariþi kabul et
    public void AcceptOrder(string orderID)
    {
        SCOrderData order = availableOrders.Find(o => o.orderID == orderID);
        if (order != null)
        {
            activeOrders.Add(order);
            availableOrders.Remove(order);
            Debug.Log("Order accepted: " + order.orderName);

            // Teslimat noktasýný aktif hale getir ve ýþýðý yanýp söndür
            if (deliveryPoints.ContainsKey(orderID))
            {
                DeliveryPoint deliveryPoint = deliveryPoints[orderID];
                deliveryPoint.gameObject.SetActive(true); // Teslimat noktasýný aktif hale getir
                deliveryPoint.StartBlinking(); // Iþýðý yanýp söndür
            }
        }
    }
    public bool AreAllOrdersCompleted()
    {
        return activeOrders.Count == 0; // Aktif görev yoksa true döner
    }
    // Sipariþi tamamla
    public void CompleteOrder(string orderID)
    {
        SCOrderData order = activeOrders.Find(o => o.orderID == orderID);
        if (order != null)
        {
            // Envanterde gerekli nesneler var mý kontrol et
            foreach (SCItem item in order.requiredItems)
            {
                if (!Inventory.Instance.HasItem(item.itemID))
                {
                    Debug.LogWarning("Required item not found: " + item.itemName);
                    return;
                }
            }

            // Nesneleri envanterden kaldýr
            foreach (SCItem item in order.requiredItems)
            {
                Inventory.Instance.RemoveItem(item.itemID);
            }

            // Sipariþi tamamla
            Debug.Log("Order completed: " + order.orderName);
            activeOrders.Remove(order);

            // Teslimat noktasýný yok et
            if (deliveryPoints.ContainsKey(orderID))
            {
                DeliveryPoint deliveryPoint = deliveryPoints[orderID];
                deliveryPoints.Remove(orderID); // Dictionary'den kaldýr
                Destroy(deliveryPoint.gameObject); // Teslimat noktasýný yok et
            }
        }
    }
}