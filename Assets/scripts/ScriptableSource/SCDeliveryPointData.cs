using UnityEngine;

[CreateAssetMenu(fileName = "DeliveryPointData", menuName = "SC/Delivery/PointData")]
public class DeliveryPointData : ScriptableObject
{
    public Vector3 position; // Teslimat noktasýnýn konumu
    public string address; // Teslimat noktasýnýn adresi
    public string orderID; // Bu teslimat noktasýnýn baðlý olduðu sipariþin ID'si
}