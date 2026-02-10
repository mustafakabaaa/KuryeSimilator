using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoPanelController : MonoBehaviour
{
    private const string UiTable = "UI";
    private const string OrdersTable = "Orders";
    public GameObject infoPanel; // InfoImage objesi
    public TextMeshProUGUI infoOrderIDText;
    public TextMeshProUGUI infoOrderNameText;
    public TextMeshProUGUI infoRequiredItemsText;
    public TextMeshProUGUI infoDeliveryAddressText;
    public TextMeshProUGUI infoRewardText;

    private void Start()
    {
        // Baslangicta infoPanel'i gizle
        infoPanel.SetActive(false);
    }
    private void OnEnable()
    {
        // Mevcut kodlar...
        UIManager.OnCloseInfoPanel += HideOrderInfo; // ✅ Ekle
    }

    private void OnDisable()
    {
        // Mevcut kodlar...
        UIManager.OnCloseInfoPanel -= HideOrderInfo; // ✅ Ekle
    }
    public void ShowOrderInfo(SCOrderData orderData)
    {
        infoPanel.SetActive(true);

        // Siparis verilerini UI elementlerine ata
        string idLabel = LocalizationHelper.Localize(UiTable, "ui.info_id");
        string nameLabel = LocalizationHelper.Localize(UiTable, "ui.info_name");
        string addressLabel = LocalizationHelper.Localize(UiTable, "ui.info_address");

        infoOrderIDText.text = $"{idLabel} {orderData.orderID}";
        infoOrderNameText.text = $"{nameLabel} {LocalizationHelper.Localize(OrdersTable, orderData.orderName)}";
        infoDeliveryAddressText.text = $"{addressLabel} {orderData.deliveryAddress}";
        infoRewardText.text = $"{orderData.reward}$";

        // Gerekli item'leri grupla ve sayılarıyla birlikte göster
        var groupedItems = new Dictionary<string, int>();
        foreach (SCItem item in orderData.requiredItems)
        {
            if (groupedItems.ContainsKey(item.itemName))
            {
                groupedItems[item.itemName]++;
            }
            else
            {
                groupedItems.Add(item.itemName, 1);
            }
        }

        string itemsLabel = LocalizationHelper.Localize(UiTable, "ui.info_items");
        infoRequiredItemsText.text = $"{itemsLabel} ";
        bool firstItem = true;
        foreach (var item in groupedItems)
        {
            if (!firstItem)
            {
                infoRequiredItemsText.text += ", ";
            }
            infoRequiredItemsText.text += $"{item.Key} x{item.Value}";
            firstItem = false;
        }
    }

    // InfoPanel'i kapat
    public void HideOrderInfo()
    {
        infoPanel.SetActive(false);
    }

    // Baska bir yere tiklandiginda InfoPanel'i kapat
    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Sol tik
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                infoPanel.GetComponent<RectTransform>(), Input.mousePosition))
            {
                HideOrderInfo();
            }
        }
    }
}