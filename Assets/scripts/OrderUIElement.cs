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
    public Button cancelOrderButton; // Inspector'da atanacak
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
        bool hasActiveOrder = OrderManager.Instance.GetActiveOrders().Count > 0;

        // Eðer sipariþ aktif sipariþler listesindeyse "Take Order" butonunu devre dýþý býrak
        takeOrderButton.interactable = !OrderManager.Instance.GetActiveOrders().Contains(order);
    }

    private void OnTakeOrderButtonClicked()
    {
        bool success = OrderManager.Instance.AcceptOrder(currentOrder.orderID);

        if (success)
        {
            // Eðer sipariþi aldýysak UI'yý güncelle
            OrderUI.Instance.OnOrderListUpdated();
        }
        else
        {
            ToastManager.Instance.ShowToast("Zaten aktif bir  siparisiniz var!", 3f);
        }
    }

    private void OnInfoButtonClicked()
    {
        // InfoPanel'i aç ve sipariþ verilerini göster
        infoPanelController.ShowOrderInfo(currentOrder);
    }
}