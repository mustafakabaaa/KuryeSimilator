using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderUIElement : MonoBehaviour
{
    private const string UiTable = "UI";
    private const string OrdersTable = "Orders";
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
        string rewardLabel = LocalizationHelper.Localize(UiTable, "ui.order_reward");
        string requiredLabel = LocalizationHelper.Localize(UiTable, "ui.order_required_items");

        orderNameText.text = LocalizationHelper.Localize(OrdersTable, order.orderName);
        orderRewardText.text = $"{rewardLabel} {order.reward}";

        // Gerekli item'leri göster
        requiredItemsText.text = $"{requiredLabel} ";
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
            ToastManager.Instance.ShowToast(
                LocalizationHelper.Localize(UiTable, "ui.order_already_active"),
                3f
            );
        }
    }

    private void OnInfoButtonClicked()
    {
        // InfoPanel'i aç ve sipariþ verilerini göster
        infoPanelController.ShowOrderInfo(currentOrder);
    }
}