using UnityEngine;

[CreateAssetMenu(fileName = "NewOrder", menuName = "SC/Orders/OrderData")]
public class SCOrderData : ScriptableObject
{
    
    public TextAsset dialogueJson; // Yeni JSON sistemi

    public string orderID; // Sipariþin benzersiz kimliði
    public string orderName; // Sipariþ adý
    public string description; // Sipariþ açýklamasý
    public SCItem[] requiredItems; // Toplanacak nesneler
    public string deliveryAddress; // Teslimat adresi (metin olarak)
    public Vector3 deliveryPosition; // Teslimat pozisyonu (Unity'deki konum)
    public int reward; // Ödül (para)

    [Header("Restoran Ayarlarý")]
    public string restaurantID; // Hangi restorandan gelecek
}