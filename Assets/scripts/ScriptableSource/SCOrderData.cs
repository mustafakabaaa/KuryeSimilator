
using UnityEngine;
[CreateAssetMenu(fileName = "NewOrder", menuName = "Orders/OrderData")]

public class SCOrderData : ScriptableObject
{
    public string orderID; // Siparisin benzersiz kimligi
    public string orderName; // Siparis adý
    public string description; // Siparis acýklamasý
    public SCItem[] requiredItems; // Toplanacak nesneler
    public string deliveryAddress; // Teslimat adresi
    public int reward; // para
}
