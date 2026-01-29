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
    public Button motorFixedButton;

    private MotorcycleShop.MotorcycleItem currentBike;
    private MotorcycleShop shop;

    public void Initialize(MotorcycleShop.MotorcycleItem bike, MotorcycleShop shopReference)
    {
        currentBike = bike;
        shop = shopReference;
        buyButton.onClick.RemoveAllListeners(); // Öncekileri temizle
        buyButton.onClick.AddListener(OnBuyButtonClicked);
        
        // Tamir butonu listener'ını ekle
        if (motorFixedButton != null)
        {
            motorFixedButton.onClick.RemoveAllListeners();
            motorFixedButton.onClick.AddListener(OnTamirButtonClicked);
        }
        
        UpdateUI();

        // Para değişimlerini dinle
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

        // Tamir butonu kontrolü
        UpdateTamirButton();
    }

    private void UpdateTamirButton()
    {
        if (motorFixedButton == null) 
        {
            Debug.LogWarning("motorFixedButton atanmamış!");
            return;
        }

        int motorIndex = System.Array.IndexOf(shop.motorcycles, currentBike);
        var motor = shop.GetMotorcycleByIndex(motorIndex);
        
        Debug.Log($"Motor kontrolü: {currentBike.name}, Index: {motorIndex}, Motor: {motor != null}");
        
        if (motor != null)
        {
            Debug.Log($"Motor durumu: Satın alınmış={currentBike.isPurchased}, Arıza={motor.ArizaSeviyesi}");
        }
        
        if (currentBike.isPurchased && motor != null && motor.ArizaSeviyesi != ArizaSeviyesi.Saglam)
        {
            motorFixedButton.gameObject.SetActive(true);
            
            int tamirMaliyeti = GetTamirMaliyeti(motor.TotalKilometre);
            bool tamirOdeyebilir = WalletManager.Instance.GetBalance() >= tamirMaliyeti;
            
            motorFixedButton.interactable = tamirOdeyebilir;
            motorFixedButton.GetComponentInChildren<TextMeshProUGUI>().text = $"TAMİR ET - {tamirMaliyeti}";
            
            Debug.Log($"Tamir butonu aktif: {tamirMaliyeti}₺, Bakiye: {WalletManager.Instance.GetBalance()}₺, Ödeyebilir: {tamirOdeyebilir}");
        }
        else
        {
            motorFixedButton.gameObject.SetActive(false);
            Debug.Log("Tamir butonu gizlendi");
        }
    }

    private int GetTamirMaliyeti(float kilometre)
    {
        if (kilometre < 1000f) return 100;
        if (kilometre < 5000f) return 250;
        if (kilometre < 10000f) return 500;
        return 1000;
    }

    private void OnTamirButtonClicked()
    {
        int motorIndex = System.Array.IndexOf(shop.motorcycles, currentBike);
        var motor = shop.GetMotorcycleByIndex(motorIndex);
        
        if (motor != null && motor.ArizaSeviyesi != ArizaSeviyesi.Saglam)
        {
            int tamirMaliyeti = GetTamirMaliyeti(motor.TotalKilometre);
            bool success = WalletManager.Instance.SpendMoney(tamirMaliyeti);
            
            if (success)
            {
                motor.TamirEt();
                UpdateUI();
                Debug.Log($"{currentBike.name} tamir edildi! - {tamirMaliyeti}₺ ödendi");
            }
        }
    }

    // Test için para ekleme metodu
    [ContextMenu("Test Para Ekle (1000₺)")]
    void TestParaEkle() {
        WalletManager.Instance.AddMoney(1000);
        Debug.Log($"Test para eklendi! Bakiye: {WalletManager.Instance.GetBalance()}₺");
        UpdateUI();
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