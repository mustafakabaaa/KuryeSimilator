using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance;

    public SCDayData[] days;
    private int currentDayIndex = 0;

    public List<SCOrderData> availableOrders = new List<SCOrderData>();
    public List<SCOrderData> activeOrders = new List<SCOrderData>();

    private Dictionary<string, DeliveryPoint> deliveryPoints = new Dictionary<string, DeliveryPoint>();
    public GameObject deliveryPointPrefab; // DeliveryPoint prefab'ý

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
            Debug.Log("Tüm günler tamamlandý!");
            return;
        }

        availableOrders.Clear();
        activeOrders.Clear();

        SCDayData currentDay = days[currentDayIndex];
        foreach (var order in currentDay.orders)
        {
            availableOrders.Add(order);
        }

        Debug.Log($"Yeni gün baþladý: {currentDay.dayName}");
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

            // DeliveryPoint prefab'ýný oluþtur
            CreateDeliveryPoint(order);
        }
    }

    private void CreateDeliveryPoint(SCOrderData order)
    {
        if (deliveryPointPrefab == null)
        {
            Debug.LogError("DeliveryPoint prefab'ý atanmamýþ!");
            return;
        }

        // Prefab'ý yükle ve oluþtur
        GameObject deliveryPointObject = Instantiate(deliveryPointPrefab, order.deliveryPosition, Quaternion.identity);
        DeliveryPoint deliveryPoint = deliveryPointObject.GetComponent<DeliveryPoint>();

        if (deliveryPoint != null)
        {
            // DeliveryPoint'i kaydet
            deliveryPoints.Add(order.orderID, deliveryPoint);
            deliveryPoint.SetOrderID(order.orderID); // OrderID'yi DeliveryPoint'e atama
            deliveryPoint.gameObject.SetActive(true); // Teslimat noktasýný aktif hale getir
            deliveryPoint.StartBlinking(); // Iþýðý yanýp söndür
        }
        else
        {
            Debug.LogError("DeliveryPoint bileþeni bulunamadý!");
        }
    }

    public bool AreAllOrdersCompleted()
    {
        return activeOrders.Count == 0;
    }

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