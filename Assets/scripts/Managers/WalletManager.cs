using UnityEngine;

public class WalletManager : MonoBehaviour, ISaveable
{
    public static WalletManager Instance { get; private set; }
    [Tooltip("Inspector�dan s�r�kleyip b�rakaca��n CurrencyWallet asset�i")]
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

        // Opsiyonel: PlayerPrefs�ten �nceki session bakiyesini y�kle
        // PlayerPrefs yükleme artık SaveManager tarafından yapılıyor
        // walletAsset.balance = PlayerPrefs.GetInt("PlayerBalance", walletAsset.balance);
    }

    private void Start()
    {
        // SaveManager'a kayıt ol
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterSystem(this);
            Debug.Log("WalletManager registered with SaveManager");
        }
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
    public int GetBalance()
    {
        return walletAsset.balance;
    }
    void Save()
    {
        // Artık SaveManager sistemi kullanılıyor, PlayerPrefs'e gerek yok
        // PlayerPrefs.SetInt("PlayerBalance", walletAsset.balance);
    }

    // ISaveable interface implementation
    public void SaveData(GameData data)
    {
        if (data.walletData == null)
        {
            data.walletData = new WalletSaveData();
        }

        data.walletData.balance = walletAsset.balance;
        Debug.Log($"[WalletManager] SaveData called - Balance: {walletAsset.balance}");
    }

    public void LoadData(GameData data)
    {
        if (data.walletData != null)
        {
            walletAsset.balance = data.walletData.balance;
            Debug.Log($"[WalletManager] LoadData called - Balance loaded: {walletAsset.balance}");
        }
        else
        {
            Debug.LogWarning("[WalletManager] No wallet data found in save file");
        }
    }
}
