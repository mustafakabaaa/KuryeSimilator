using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MotorcycleUIElement : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public Image motorcycleImage;
    public Button buyButton;

    private MotorcycleShop.MotorcycleItem currentBike;
    private MotorcycleShop shop;

    public void Initialize(MotorcycleShop.MotorcycleItem bike, MotorcycleShop shopReference)
    {
        currentBike = bike;
        shop = shopReference;
        buyButton.onClick.RemoveAllListeners(); // Öncekileri temizle
        buyButton.onClick.AddListener(OnBuyButtonClicked);
        UpdateUI();

        // Para deðiþimlerini dinle
        WalletManager.Instance.walletAsset.OnBalanceChanged += UpdateUI;
    }

    private void OnDestroy()
    {
        if (WalletManager.Instance != null && WalletManager.Instance.walletAsset != null)
        {
            WalletManager.Instance.walletAsset.OnBalanceChanged -= UpdateUI;
        }
    }

    public void UpdateUI()
    {
        nameText.text = currentBike.name;
        priceText.text = "$" + currentBike.price;
        motorcycleImage.sprite = currentBike.motorcycleImage;

        bool canAfford = WalletManager.Instance.GetBalance() >= currentBike.price;
        buyButton.interactable = !currentBike.isPurchased && canAfford;

        buyButton.GetComponentInChildren<TextMeshProUGUI>().text =
            currentBike.isPurchased ? "OWNED" : "BUY";
    }

    public void OnBuyButtonClicked()
    {
        if (!currentBike.isPurchased)
        {
            bool success = WalletManager.Instance.SpendMoney(currentBike.price);
            if (success)
            {
                currentBike.isPurchased = true;
                UpdateUI();
                int index = System.Array.IndexOf(shop.motorcycles, currentBike);
                if (index != -1)
                {
                    shop.SpawnMotorcycle(index);
                }
            }
        }
    }
}