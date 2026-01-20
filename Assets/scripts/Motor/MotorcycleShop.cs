using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static GameData;

public class MotorcycleShop : MonoBehaviour, ISaveable
{
    [System.Serializable]
    public class MotorcycleItem
    {
        public string name;
        public int price;
        public GameObject motorcyclePrefab;
        public Sprite motorcycleImage;
        [HideInInspector] public bool isPurchased = false;
    }
    public static MotorcycleShop Instance;
    public GameObject player;
    public GameObject playerCamera;
    public Transform spawnPoint;

    [Header("Motorcycle Settings")]
    public MotorcycleItem[] motorcycles;
    public Transform uiParent;

    [Header("UI Prefab")]
    public GameObject motorcycleUIPrefab;

    private List<MotorcycleVehicle> spawnedMotors = new List<MotorcycleVehicle>();

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        SaveManager.Instance.RegisterSystem(this);
        CreateMotorcycleUI();
        
        if (SaveManager.Instance.HasAnySaveData())
        {
            LoadMotorcycleFromSave();
        }
        else
        {
            // İlk başlangıçta satın alınan tüm motorları spawn et
            SpawnAllPurchasedMotorcycles();
            UpdateAllUI();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SaveManager.Instance.HasAnySaveData()) // HasSaveData() yerine HasAnySaveData()
        {
            LoadMotorcycleFromSave();
        }
    }

    private void LoadMotorcycleFromSave()
    {
        var savedData = SaveManager.Instance.GetCurrentGameData();
        if (savedData != null && savedData.motorcycleData != null)
        {
            LoadData(savedData);
        }
    }

    /// <summary>
    /// Belirli bir motoru spawn et (sabit konum sistemine göre)
    /// </summary>
    public void SpawnMotorcycle(int index)
    {
        if (index < 0 || index >= motorcycles.Length) return;
        if (!motorcycles[index].isPurchased) return;
        
        // Bu motor zaten spawn edilmiş mi kontrol et
        if (IsMotorcycleSpawned(index))
        {
            Debug.Log($"[MotorcycleShop] Motor {index} zaten spawn edilmiş");
            return;
        }

        // Home position'ı VehicleManager'dan al
        Vector3 spawnPosition = spawnPoint.position;
        Quaternion spawnRotation = spawnPoint.rotation;
        
        if (VehicleManager.Instance != null)
        {
            Vector3 homePos = VehicleManager.Instance.GetHomePosition(index);
            spawnPosition = homePos;
        }

        GameObject newMotor = Instantiate(
            motorcycles[index].motorcyclePrefab,
            spawnPosition,
            spawnRotation
        );

        MotorcycleVehicle vehicleScript = newMotor.GetComponent<MotorcycleVehicle>();
        if (vehicleScript != null)
        {
            vehicleScript._player = player;
            vehicleScript._playerCamera = playerCamera;
            spawnedMotors.Add(vehicleScript);
            
            // VehicleManager'a kaydet
            if (VehicleManager.Instance != null)
            {
                VehicleManager.Instance.RegisterMotorcycle(vehicleScript, index);
                Debug.Log($"[MotorcycleShop] Motor {index} spawn edildi ve VehicleManager'a kaydedildi");
            }
        }
    }

    /// <summary>
    /// Motor zaten spawn edilmiş mi kontrol et
    /// </summary>
    private bool IsMotorcycleSpawned(int index)
    {
        foreach (var motor in spawnedMotors)
        {
            if (motor != null && motor.gameObject != null)
            {
                int motorIndex = GetMotorcycleIndexForSpawned(motor);
                if (motorIndex == index)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Spawn edilmiş motorun index'ini bul
    /// </summary>
    private int GetMotorcycleIndexForSpawned(MotorcycleVehicle motor)
    {
        string motorName = motor.gameObject.name.Replace("(Clone)", "").Trim();
        
        for (int i = 0; i < motorcycles.Length; i++)
        {
            if (motorcycles[i].motorcyclePrefab != null)
            {
                string prefabName = motorcycles[i].motorcyclePrefab.name;
                if (motorName == prefabName)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    /// <summary>
    /// Tüm motorları spawn et (sabit konum sistemine göre)
    /// </summary>
    public void SpawnAllPurchasedMotorcycles()
    {
        for (int i = 0; i < motorcycles.Length; i++)
        {
            if (motorcycles[i].isPurchased)
            {
                SpawnMotorcycle(i);
            }
        }
    }

    /// <summary>
    /// Belirli bir motoru kaldır (artık kullanılmıyor - motorlar silinmiyor)
    /// </summary>
    private void ClearExistingMotorcycles()
    {
        // Artık motorlar silinmiyor, sadece temizleme yapılıyor
        List<MotorcycleVehicle> toRemove = new List<MotorcycleVehicle>();
        
        foreach (var motor in spawnedMotors)
        {
            if (motor == null || motor.gameObject == null)
            {
                toRemove.Add(motor);
            }
        }
        
        foreach (var motor in toRemove)
        {
            spawnedMotors.Remove(motor);
        }
    }

    private void CreateMotorcycleUI()
    {
        foreach (var bike in motorcycles)
        {
            GameObject bikeUI = Instantiate(motorcycleUIPrefab, uiParent);
            MotorcycleUIElement uiElement = bikeUI.GetComponent<MotorcycleUIElement>();
            if (uiElement != null)
            {
                uiElement.Initialize(bike, this);
            }
        }
    }

    public void UpdateAllUI()
    {
        foreach (Transform child in uiParent)
        {
            MotorcycleUIElement uiElement = child.GetComponent<MotorcycleUIElement>();
            if (uiElement != null)
            {
                uiElement.UpdateUI();
            }
        }
    }

    public bool TryBuyMotorcycle(MotorcycleItem item)
    {
        if (WalletManager.Instance.SpendMoney(item.price))
        {
            item.isPurchased = true;
            
            // Yeni satın alınan motoru spawn et (sabit konum sistemine göre)
            int index = System.Array.IndexOf(motorcycles, item);
            if (index >= 0)
            {
                SpawnMotorcycle(index);
            }
            
            UpdateAllUI();
            SaveManager.Instance.SaveGame();
            return true;
        }
        return false;
    }

    public void SaveData(GameData data)
    {
        if (data.motorcycleData == null)
        {
            data.motorcycleData = new MotorcycleSaveData();
        }

        data.motorcycleData.purchasedMotorcycles = motorcycles.Select(m => m.isPurchased).ToArray();

        // Aktif motoru bul (oyuncunun üzerinde olduğu veya en son kullanılan)
        MotorcycleVehicle activeMotor = null;
        int activeIndex = -1;
        
        foreach (var motor in spawnedMotors)
        {
            if (motor != null && motor.gameObject != null)
            {
                if (motor.IsPlayerOnBoard)
                {
                    activeMotor = motor;
                    activeIndex = GetMotorcycleIndexForSpawned(motor);
                    break;
                }
            }
        }
        
        // Eğer aktif motor yoksa, ilk spawn edilen motoru al
        if (activeMotor == null && spawnedMotors.Count > 0)
        {
            activeMotor = spawnedMotors[0];
            if (activeMotor != null && activeMotor.gameObject != null)
            {
                activeIndex = GetMotorcycleIndexForSpawned(activeMotor);
            }
        }

        if (activeMotor != null && activeMotor.gameObject != null)
        {
            data.motorcycleData.activeMotorcycleIndex = activeIndex;
            data.motorcycleData.motorcyclePosition = new Vector3Serializable(activeMotor.transform.position);
            data.motorcycleData.motorcycleRotation = new Vector3Serializable(activeMotor.transform.eulerAngles);
            data.motorcycleData.isPlayerOnBike = activeMotor.IsPlayerOnBoard;
        }
        else
        {
            data.motorcycleData.activeMotorcycleIndex = -1;
            data.motorcycleData.isPlayerOnBike = false;
        }

        data.motorcycleData.motorcycleKilometres = new float[motorcycles.Length];
        for (int i = 0; i < motorcycles.Length; i++)
        {
            var motor = GetMotorcycleByIndex(i);
            if (motor != null)
            {
                data.motorcycleData.motorcycleKilometres[i] = motor.TotalKilometre;
            }
            else
            {
                data.motorcycleData.motorcycleKilometres[i] = 0f;
            }
        }

        data.motorcycleData.arizaSeviyeleri = new ArizaSeviyesi[motorcycles.Length];
        data.motorcycleData.sonArizaKontrolKm = new float[motorcycles.Length];
        for (int i = 0; i < motorcycles.Length; i++)
        {
            var motor = GetMotorcycleByIndex(i);
            if (motor != null)
            {
                data.motorcycleData.arizaSeviyeleri[i] = motor.ArizaSeviyesi;
                data.motorcycleData.sonArizaKontrolKm[i] = motor.TotalKilometre - (motor.TotalKilometre % 50f);
            }
            else
            {
                data.motorcycleData.arizaSeviyeleri[i] = ArizaSeviyesi.Saglam;
                data.motorcycleData.sonArizaKontrolKm[i] = 0f;
            }
        }
    }

    // Enhance LoadData to properly restore state
    public void LoadData(GameData data)
    {
        if (data.motorcycleData == null) return;

        // Load purchase status
        for (int i = 0; i < Mathf.Min(motorcycles.Length, data.motorcycleData.purchasedMotorcycles.Length); i++)
        {
            motorcycles[i].isPurchased = data.motorcycleData.purchasedMotorcycles[i];
        }

        // Sabit konum sistemi: Tüm satın alınan motorları spawn et
        ClearExistingMotorcycles(); // Null referansları temizle
        
        // Tüm satın alınan motorları spawn et
        SpawnAllPurchasedMotorcycles();

        // Load edilen pozisyonları uygula (eğer kaydedilmişse)
        if (data.motorcycleData.activeMotorcycleIndex >= 0 &&
            data.motorcycleData.activeMotorcycleIndex < motorcycles.Length &&
            motorcycles[data.motorcycleData.activeMotorcycleIndex].isPurchased)
        {
            // Aktif motorun pozisyonunu yükle
            MotorcycleVehicle activeMotor = GetMotorcycleByIndex(data.motorcycleData.activeMotorcycleIndex);
            if (activeMotor != null)
            {
                StartCoroutine(SetMotorTransformAfterFrame(activeMotor, data.motorcycleData));

                // Restore player on bike state if needed
                if (data.motorcycleData.isPlayerOnBike)
                {
                    activeMotor.MountMotorcycle();
                }
            }
        }

        if (data.motorcycleData.motorcycleKilometres != null)
        {
            for (int i = 0; i < Mathf.Min(motorcycles.Length, data.motorcycleData.motorcycleKilometres.Length); i++)
            {
                var motor = GetMotorcycleByIndex(i);
                if (motor != null)
                {
                    motor.SetTotalKilometre(data.motorcycleData.motorcycleKilometres[i]);
                }
            }
        }

        if (data.motorcycleData.arizaSeviyeleri != null && data.motorcycleData.sonArizaKontrolKm != null)
        {
            for (int i = 0; i < Mathf.Min(motorcycles.Length, data.motorcycleData.arizaSeviyeleri.Length); i++)
            {
                var motor = GetMotorcycleByIndex(i);
                if (motor != null)
                {
                    motor.SetArizaVerisi(data.motorcycleData.arizaSeviyeleri[i], data.motorcycleData.sonArizaKontrolKm[i]);
                }
            }
        }

        UpdateAllUI();
    }
    public bool IsMotorcyclePurchased(int index)
    {
        if (index < 0 || index >= motorcycles.Length) return false;
        return motorcycles[index].isPurchased;
    }

    /// <summary>
    /// Belirli bir motoru index'e göre bul
    /// </summary>
    private MotorcycleVehicle GetMotorcycleByIndex(int index)
    {
        foreach (var motor in spawnedMotors)
        {
            if (motor != null && motor.gameObject != null)
            {
                int motorIndex = GetMotorcycleIndexForSpawned(motor);
                if (motorIndex == index)
                {
                    return motor;
                }
            }
        }
        return null;
    }

    private IEnumerator SetMotorTransformAfterFrame(MotorcycleVehicle motor, MotorcycleSaveData data)
    {
        if (motor == null || motor.gameObject == null) yield break;
        
        yield return new WaitForFixedUpdate();

        // Pozisyon kontrolü - geçersizse home position'a git
        Vector3 savedPosition = data.motorcyclePosition.ToVector3();
        
        // VehicleManager varsa pozisyon doğrulaması yap
        if (VehicleManager.Instance != null)
        {
            // Eğer kaydedilen pozisyon geçersizse home position kullan
            Vector3 homePos = VehicleManager.Instance.GetHomePosition(data.activeMotorcycleIndex);
            
            // Basit doğrulama: Y pozisyonu çok düşük veya çok yüksekse home position kullan
            if (savedPosition.y < -10f || savedPosition.y > 1000f)
            {
                savedPosition = homePos;
                Debug.LogWarning($"[MotorcycleShop] Kaydedilen pozisyon geçersiz, home position kullanılıyor");
            }
        }
        
        motor.transform.position = savedPosition;
        motor.transform.rotation = Quaternion.Euler(data.motorcycleRotation.ToVector3());

        var rb = motor.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private int GetActiveMotorcycleIndex()
    {
        if (spawnedMotors.Count == 0) return -1;

        for (int i = 0; i < motorcycles.Length; i++)
        {
            if (motorcycles[i].motorcyclePrefab.name == spawnedMotors[0].gameObject.name.Replace("(Clone)", ""))
            {
                return i;
            }
        }
        return -1;
    }
}
