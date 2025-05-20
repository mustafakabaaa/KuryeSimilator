using UnityEngine;

public class WalletManager : MonoBehaviour
{
    public static WalletManager Instance { get; private set; }
    [Tooltip("Inspector’dan sürükleyip býrakacaðýn CurrencyWallet asset’i")]
    public CurrencyWallet walletAsset;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Opsiyonel: PlayerPrefs’ten önceki session bakiyesini yükle
        walletAsset.balance = PlayerPrefs.GetInt("PlayerBalance", walletAsset.balance);
    }

    public void AddMoney(int amount)
    {
        walletAsset.Add(amount);
        Save();
    }

    public bool SpendMoney(int amount)
    {
        bool ok = walletAsset.Spend(amount);
        if (ok) Save();
        return ok;
    }

    void Save()
    {
        PlayerPrefs.SetInt("PlayerBalance", walletAsset.balance);
    }
}
