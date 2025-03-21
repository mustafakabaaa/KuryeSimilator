using UnityEngine;
using System.Collections.Generic;

public class DeliveryPointPool : MonoBehaviour
{
    public GameObject deliveryPointPrefab; // Delivery point prefab'ý
    public int poolSize = 10; // Havuzdaki nesne sayýsý

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Start()
    {
        // Havuzu baþlangýçta doldur
        for (int i = 0; i < poolSize; i++)
        {
            GameObject point = Instantiate(deliveryPointPrefab);
            point.SetActive(false);
            pool.Enqueue(point);
        }
    }

    // Havuzdan bir delivery point al
    public GameObject GetDeliveryPoint()
    {
        if (pool.Count > 0)
        {
            GameObject point = pool.Dequeue();
            point.SetActive(true);
            return point;
        }
        return null; // Havuz boþsa null döndür
    }

    // Delivery point'i havuza geri ver
    public void ReturnDeliveryPoint(GameObject point)
    {
        point.SetActive(false);
        pool.Enqueue(point);
    }
}