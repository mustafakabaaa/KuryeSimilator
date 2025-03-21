using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderUIElement : MonoBehaviour
{
    public TextMeshProUGUI orderNameText; // Sipariþ adý
    public TextMeshProUGUI orderRewardText; // Sipariþ ödülü
    public TextMeshProUGUI requiredItemsText; // Gerekli item'ler
    public Button takeOrderButton; // Sipariþi almak için buton
    public Button infoButton; // Bilgi butonu

    private SCOrderData currentOrder;
    public InfoPanelController infoPanelController; // Manuel atama için

    void Start()
    {
        // InfoPanelController'ý bul
        infoPanelController = FindObjectOfType<InfoPanelController>();

        // Buton týklamalarýný dinle
        infoButton.onClick.AddListener(OnInfoButtonClicked);
        takeOrderButton.onClick.AddListener(OnTakeOrderButtonClicked);
    }

    public void Setup(SCOrderData order)
    {
        currentOrder = order;

        // UI elementlerini güncelle
        orderNameText.text = order.orderName;
        orderRewardText.text = "Reward: " + order.reward.ToString();

        // Gerekli item'leri göster
        requiredItemsText.text = "Required Items: ";
        foreach (SCItem item in order.requiredItems)
        {
            requiredItemsText.text += item.itemName + ", ";
        }
        requiredItemsText.text = requiredItemsText.text.TrimEnd(',', ' ');

        // Eðer sipariþ aktif sipariþler listesindeyse "Take Order" butonunu devre dýþý býrak
        takeOrderButton.interactable = !OrderManager.Instance.GetActiveOrders().Contains(order);
    }

    private void OnTakeOrderButtonClicked()
    {
        OrderManager.Instance.AcceptOrder(currentOrder.orderID);
        Destroy(gameObject); // Sipariþ alýndýktan sonra UI elementini kaldýr
    }

    private void OnInfoButtonClicked()
    {
        // InfoPanel'i aç ve sipariþ verilerini göster
        infoPanelController.ShowOrderInfo(currentOrder);
    }
}