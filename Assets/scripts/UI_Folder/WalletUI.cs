using TMPro;
using UnityEngine;

public class WalletUI : MonoBehaviour
{
    public TMP_Text balanceText;
    private CurrencyWallet wallet;

    void Start()
    {
        wallet = WalletManager.Instance.walletAsset;

        // Ýlk yükleme anýnda göster
        balanceText.text =  wallet.balance+"$";

        // Event’e abone ol
        wallet.OnBalanceChanged += UpdateUI;
    }

    void OnDestroy()
    {
        // Aboneliði kaldýrmayý unutma (önemli!)
        if (wallet != null)
            wallet.OnBalanceChanged -= UpdateUI;
    }

    void UpdateUI()
    {
        balanceText.text = wallet.balance + "$";
    }
}
