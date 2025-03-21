using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance; // Singleton ornegi

    public List<SCOrderData> availableOrders = new List<SCOrderData>(); // Mevcut siparisler
    public List<SCOrderData> activeOrders = new List<SCOrderData>(); // Aktif siparisler

    private Dictionary<string, DeliveryPoint> deliveryPoints = new Dictionary<string, DeliveryPoint>(); // Teslimat noktalari

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

    // Teslimat noktasini kaydet
    public void RegisterDeliveryPoint(string orderID, DeliveryPoint deliveryPoint)
    {
        if (!deliveryPoints.ContainsKey(orderID))
        {
            deliveryPoints.Add(orderID, deliveryPoint);
            deliveryPoint.gameObject.SetActive(false); // Baslangicta teslimat noktasini devre disi birak
        }
    }

    public List<SCOrderData> GetAvailableOrders()
    {
        return availableOrders;
    }

    public List<SCOrderData> GetActiveOrders()
    {
        return activeOrders;
    }

    // Siparisi kabul et
    public void AcceptOrder(string orderID)
    {
        SCOrderData order = availableOrders.Find(o => o.orderID == orderID);
        if (order != null)
        {
            activeOrders.Add(order);
            availableOrders.Remove(order);
            Debug.Log("Order accepted: " + order.orderName);

            // Teslimat noktasini aktif hale getir ve isigi yanip sondur
            if (deliveryPoints.ContainsKey(orderID))
            {
                DeliveryPoint deliveryPoint = deliveryPoints[orderID];
                deliveryPoint.gameObject.SetActive(true); // Teslimat noktasini aktif hale getir
                deliveryPoint.StartBlinking(); // Isigi yanip sondur
            }
        }
    }

    // Siparisi tamamla
    public void CompleteOrder(string orderID)
    {
        SCOrderData order = activeOrders.Find(o => o.orderID == orderID);
        if (order != null)
        {
            // Envanterde gerekli nesneler var mi kontrol et
            foreach (SCItem item in order.requiredItems)
            {
                if (!Inventory.Instance.HasItem(item.itemID))
                {
                    Debug.LogWarning("Required item not found: " + item.itemName);
                    return;
                }
            }

            // Nesneleri envanterden kaldir
            foreach (SCItem item in order.requiredItems)
            {
                Inventory.Instance.RemoveItem(item.itemID);
            }

            // Siparisi tamamla
            Debug.Log("Order completed: " + order.orderName);
            activeOrders.Remove(order);

            // Teslimat noktasini yok et
            if (deliveryPoints.ContainsKey(orderID))
            {
                DeliveryPoint deliveryPoint = deliveryPoints[orderID];
                deliveryPoints.Remove(orderID); // Dictionary'den kaldir
                Destroy(deliveryPoint.gameObject); // Teslimat noktasini yok et
            }
        }
    }
}