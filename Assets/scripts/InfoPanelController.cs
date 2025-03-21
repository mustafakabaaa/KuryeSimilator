using TMPro;
using UnityEngine;

public class InfoPanelController : MonoBehaviour
{
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

    // InfoPanel'i ac ve siparis verilerini goster
    public void ShowOrderInfo(SCOrderData orderData)
    {
        infoPanel.SetActive(true);

        // Siparis verilerini UI elementlerine ata
        infoOrderIDText.text = "ID: " + orderData.orderID;
        infoOrderNameText.text = "Name: " + orderData.orderName;
        infoDeliveryAddressText.text = "Address: " + orderData.deliveryAddress;
        infoRewardText.text = orderData.reward.ToString() + "$";

        // Gerekli item'leri goster
        infoRequiredItemsText.text = "Orders: ";
        foreach (SCItem item in orderData.requiredItems)
        {
            infoRequiredItemsText.text += item.itemName + ", ";
        }
        infoRequiredItemsText.text = infoRequiredItemsText.text.TrimEnd(',', ' ');
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