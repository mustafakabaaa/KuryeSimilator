using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tüm araçları (motor ve bisiklet) yönetir, pozisyon kontrolü ve güvenlik sistemleri sağlar
/// </summary>
public class VehicleManager : MonoBehaviour, ISaveable
{
    public static VehicleManager Instance { get; private set; }

    [System.Serializable]
    public class VehicleHomePosition
    {
        public int motorcycleIndex; // MotorcycleShop'taki index
        public Transform homePosition; // Inspector'dan atanacak sabit konum
        [HideInInspector] public Vector3 homePos; // Runtime'da kullanılacak pozisyon
    }

    [Header("Boundary Settings")]
    [SerializeField] private Vector3 worldMin = new Vector3(-500, -10, -500);
    [SerializeField] private Vector3 worldMax = new Vector3(500, 1000, 500);
    
    [Header("Safety Thresholds")]
    [SerializeField] private float minYPosition = -100f; // Düşme eşiği
    [SerializeField] private float maxYPosition = 1000f; // Çok yüksek eşik
    [SerializeField] private float maxVelocity = 200f; // Maksimum hız (bug tespiti için)
    [SerializeField] private float airTimeThreshold = 5f; // Havada kalma süresi (saniye)
    
    [Header("Motor Home Positions")]
    [SerializeField] private VehicleHomePosition[] motorcycleHomePositions;
    
    [Header("Bicycle Settings")]
    [SerializeField] private Transform bicycleHomePosition; // Bisiklet için sabit konum
    
    [Header("References")]
    [SerializeField] private MotorcycleShop motorcycleShop;
    
    // Takip edilen araçlar
    private Dictionary<int, MotorcycleVehicle> trackedMotorcycles = new Dictionary<int, MotorcycleVehicle>();
    private Dictionary<MotorcycleVehicle, VehicleTrackingData> vehicleTrackingData = new Dictionary<MotorcycleVehicle, VehicleTrackingData>();
    
    // Bisiklet takibi
    private BicycleVehicle trackedBicycle;
    private Vector3 bicycleHomePos;
    
    [System.Serializable]
    private class VehicleTrackingData
    {
        public int motorcycleIndex;
        public Vector3 homePosition;
        public float timeInAir = 0f;
        public bool wasGroundedLastFrame = true;
        public float lastValidYPosition;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Home pozisyonlarını Vector3'e çevir
            InitializeHomePositions();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterSystem(this);
        }
        
        // Mevcut motorları bul ve kaydet
        StartCoroutine(RegisterExistingVehicles());
    }

    private void InitializeHomePositions()
    {
        if (motorcycleHomePositions != null)
        {
            foreach (var homePos in motorcycleHomePositions)
            {
                if (homePos.homePosition != null)
                {
                    homePos.homePos = homePos.homePosition.position;
                }
            }
        }
        
        // Bisiklet home position'ı
        if (bicycleHomePosition != null)
        {
            bicycleHomePos = bicycleHomePosition.position;
        }
    }

    private IEnumerator RegisterExistingVehicles()
    {
        yield return new WaitForSeconds(1f); // MotorcycleShop'un hazır olmasını bekle
        
        // Sahnedeki tüm motorları bul
        MotorcycleVehicle[] allMotors = FindObjectsOfType<MotorcycleVehicle>();
        
        if (enableDebugLogs)
        {
            Debug.Log($"[VehicleManager] Sahnedeki motor sayısı: {allMotors.Length}");
        }
        
        foreach (var motor in allMotors)
        {
            if (motor != null && motor.gameObject != null)
            {
                int index = GetMotorcycleIndex(motor);
                if (index >= 0)
                {
                    RegisterMotorcycle(motor, index);
                }
                else if (enableDebugLogs)
                {
                    Debug.LogWarning($"[VehicleManager] Motor index bulunamadı: {motor.gameObject.name}");
                }
            }
        }
        
        // Bisikleti bul ve kaydet
        BicycleVehicle bicycle = FindObjectOfType<BicycleVehicle>();
        if (bicycle != null)
        {
            RegisterBicycle(bicycle);
            if (enableDebugLogs)
            {
                Debug.Log("[VehicleManager] Bisiklet bulundu ve kaydedildi");
            }
        }
        else if (enableDebugLogs)
        {
            Debug.LogWarning("[VehicleManager] Bisiklet bulunamadı!");
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"[VehicleManager] Toplam {vehicleTrackingData.Count} motor kaydedildi");
        }
    }

    /// <summary>
    /// Motoru sisteme kaydet
    /// </summary>
    public void RegisterMotorcycle(MotorcycleVehicle motor, int motorcycleIndex)
    {
        if (motor == null) return;
        
        trackedMotorcycles[motorcycleIndex] = motor;
        
        // Home position'ı bul
        Vector3 homePos = GetHomePosition(motorcycleIndex);
        
        VehicleTrackingData trackingData = new VehicleTrackingData
        {
            motorcycleIndex = motorcycleIndex,
            homePosition = homePos,
            lastValidYPosition = motor.transform.position.y
        };
        
        vehicleTrackingData[motor] = trackingData;
        
        Debug.Log($"[VehicleManager] Motor kaydedildi: Index {motorcycleIndex}, Home Position: {homePos}");
    }

    /// <summary>
    /// Motoru sistemden çıkar
    /// </summary>
    public void UnregisterMotorcycle(int motorcycleIndex)
    {
        if (trackedMotorcycles.TryGetValue(motorcycleIndex, out MotorcycleVehicle motor))
        {
            if (vehicleTrackingData.ContainsKey(motor))
            {
                vehicleTrackingData.Remove(motor);
            }
            trackedMotorcycles.Remove(motorcycleIndex);
        }
    }

    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = false;
    [SerializeField] private float checkInterval = 0.1f; // Her 0.1 saniyede bir kontrol et (performans için)
    
    private float lastCheckTime = 0f;

    private void Update()
    {
        // Performans için belirli aralıklarla kontrol et
        if (Time.time - lastCheckTime >= checkInterval)
        {
            lastCheckTime = Time.time;
            CheckAllVehicles();
            CheckBicycle(); // Bisiklet kontrolü
        }
    }

    private void CheckAllVehicles()
    {
        // Eğer hiç motor kayıtlı değilse uyarı ver
        if (vehicleTrackingData.Count == 0 && enableDebugLogs)
        {
            Debug.LogWarning("[VehicleManager] Kayıtlı motor bulunamadı! Motorlar VehicleManager'a kaydedilmemiş olabilir.");
        }
        
        List<MotorcycleVehicle> vehiclesToRemove = new List<MotorcycleVehicle>();
        
        foreach (var kvp in vehicleTrackingData)
        {
            MotorcycleVehicle motor = kvp.Key;
            VehicleTrackingData data = kvp.Value;
            
            if (motor == null || motor.gameObject == null)
            {
                vehiclesToRemove.Add(motor);
                continue;
            }
            
            // Motor aktif değilse kontrol etme (oyuncu üzerindeyse)
            if (!motor.gameObject.activeInHierarchy)
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"[VehicleManager] Motor {data.motorcycleIndex} aktif değil, kontrol edilmiyor");
                }
                continue;
            }
            
            if (motor.IsPlayerOnBoard)
            {
                // Oyuncu üzerindeyse kontrol etme ama debug log ekle
                if (enableDebugLogs)
                {
                    Debug.Log($"[VehicleManager] Motor {data.motorcycleIndex} oyuncu üzerinde, kontrol edilmiyor");
                }
                continue;
            }
            
            bool needsReset = false;
            string reason = "";
            Vector3 pos = motor.transform.position;
            
            // 1. Fall Detection (Düşme Tespiti) - Öncelikli kontrol
            if (IsFalling(pos))
            {
                needsReset = true;
                reason = $"Düşme tespiti (Y: {pos.y:F2} < {minYPosition})";
            }
            // 2. Boundary Kontrolü
            else if (!IsWithinBoundary(pos))
            {
                needsReset = true;
                reason = $"Boundary dışında (Pos: {pos})";
            }
            // 3. Yükseklik Kontrolü
            else if (pos.y > maxYPosition)
            {
                needsReset = true;
                reason = $"Çok yüksekte (Y: {pos.y:F2} > {maxYPosition})";
            }
            // 4. Velocity Kontrolü (Çok hızlı hareket - bug tespiti)
            else if (HasExcessiveVelocity(motor))
            {
                needsReset = true;
                Rigidbody rb = motor.GetComponent<Rigidbody>();
                float velocity = rb != null ? rb.linearVelocity.magnitude : 0f;
                reason = $"Aşırı hız (Velocity: {velocity:F2} > {maxVelocity})";
            }
            // 5. Havada Kalma Süresi
            else if (IsStuckInAir(motor, data))
            {
                needsReset = true;
                reason = $"Havada sıkışmış (Havada kalma süresi: {data.timeInAir:F2}s)";
            }
            
            if (needsReset)
            {
                Debug.LogWarning($"[VehicleManager] Motor resetleniyor (Index: {data.motorcycleIndex}) - Sebep: {reason} - Mevcut Pozisyon: {pos}");
                ResetMotorToHome(motor, data);
            }
            else
            {
                // Tracking data güncelle
                UpdateTrackingData(motor, data);
            }
        }
        
        // Silinen motorları temizle
        foreach (var motor in vehiclesToRemove)
        {
            if (motor != null && vehicleTrackingData.ContainsKey(motor))
            {
                int index = vehicleTrackingData[motor].motorcycleIndex;
                vehicleTrackingData.Remove(motor);
                trackedMotorcycles.Remove(index);
            }
        }
    }

    private bool IsWithinBoundary(Vector3 position)
    {
        return position.x >= worldMin.x && position.x <= worldMax.x &&
               position.y >= worldMin.y && position.y <= worldMax.y &&
               position.z >= worldMin.z && position.z <= worldMax.z;
    }

    private bool IsFalling(Vector3 position)
    {
        bool falling = position.y < minYPosition;
        if (falling && enableDebugLogs)
        {
            Debug.Log($"[VehicleManager] Düşme tespit edildi: Y = {position.y:F2} < {minYPosition}");
        }
        return falling;
    }

    private bool HasExcessiveVelocity(MotorcycleVehicle motor)
    {
        Rigidbody rb = motor.GetComponent<Rigidbody>();
        if (rb == null) return false;
        
        return rb.linearVelocity.magnitude > maxVelocity;
    }

    private bool IsStuckInAir(MotorcycleVehicle motor, VehicleTrackingData data)
    {
        // Zemin kontrolü yap (basit raycast)
        bool isGrounded = Physics.Raycast(motor.transform.position, Vector3.down, 2f);
        
        if (!isGrounded)
        {
            data.timeInAir += Time.deltaTime;
            if (data.timeInAir >= airTimeThreshold)
            {
                return true;
            }
        }
        else
        {
            data.timeInAir = 0f;
        }
        
        return false;
    }

    private void UpdateTrackingData(MotorcycleVehicle motor, VehicleTrackingData data)
    {
        // Valid pozisyonları kaydet
        if (motor.transform.position.y > minYPosition && motor.transform.position.y < maxYPosition)
        {
            data.lastValidYPosition = motor.transform.position.y;
        }
    }

    /// <summary>
    /// Motoru home position'a resetle
    /// </summary>
    public void ResetMotorToHome(int motorcycleIndex)
    {
        if (trackedMotorcycles.TryGetValue(motorcycleIndex, out MotorcycleVehicle motor))
        {
            if (vehicleTrackingData.TryGetValue(motor, out VehicleTrackingData data))
            {
                ResetMotorToHome(motor, data);
            }
        }
    }

    private void ResetMotorToHome(MotorcycleVehicle motor, VehicleTrackingData data)
    {
        if (motor == null || motor.gameObject == null) return;
        
        Debug.Log($"[VehicleManager] Motor resetleniyor: Index {data.motorcycleIndex}, Eski Pozisyon: {motor.transform.position}, Yeni Pozisyon: {data.homePosition}");
        
        // Rigidbody'yi durdur
        Rigidbody rb = motor.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        // Home position'a taşı
        motor.transform.position = data.homePosition;
        motor.transform.rotation = Quaternion.identity;
        
        // Tracking data'yı sıfırla
        data.timeInAir = 0f;
        data.wasGroundedLastFrame = true;
        data.lastValidYPosition = data.homePosition.y;
        
        Debug.Log($"[VehicleManager] Motor başarıyla home position'a resetlendi: Index {data.motorcycleIndex}");
    }

    /// <summary>
    /// Motorun home position'ını al
    /// </summary>
    public Vector3 GetHomePosition(int motorcycleIndex)
    {
        if (motorcycleHomePositions != null)
        {
            foreach (var homePos in motorcycleHomePositions)
            {
                if (homePos.motorcycleIndex == motorcycleIndex)
                {
                    return homePos.homePosition != null ? homePos.homePosition.position : homePos.homePos;
                }
            }
        }
        
        // Fallback: MotorcycleShop'un spawn point'ini kullan
        if (motorcycleShop != null && motorcycleShop.spawnPoint != null)
        {
            return motorcycleShop.spawnPoint.position;
        }
        
        return Vector3.zero;
    }

    /// <summary>
    /// Motorun index'ini bul (MotorcycleShop'tan)
    /// </summary>
    private int GetMotorcycleIndex(MotorcycleVehicle motor)
    {
        if (motorcycleShop == null) return -1;
        
        // Motor prefab adından index bul
        string motorName = motor.gameObject.name.Replace("(Clone)", "").Trim();
        
        for (int i = 0; i < motorcycleShop.motorcycles.Length; i++)
        {
            if (motorcycleShop.motorcycles[i].motorcyclePrefab != null)
            {
                string prefabName = motorcycleShop.motorcycles[i].motorcyclePrefab.name;
                if (motorName == prefabName)
                {
                    return i;
                }
            }
        }
        
        return -1;
    }

    /// <summary>
    /// Bisikleti sisteme kaydet
    /// </summary>
    public void RegisterBicycle(BicycleVehicle bicycle)
    {
        if (bicycle == null) return;
        
        trackedBicycle = bicycle;
        
        if (enableDebugLogs)
        {
            Debug.Log($"[VehicleManager] Bisiklet kaydedildi, Home Position: {bicycleHomePos}");
        }
    }

    /// <summary>
    /// Bisiklet kontrolü (basit - sadece boundary ve düşme)
    /// </summary>
    private void CheckBicycle()
    {
        if (trackedBicycle == null || trackedBicycle.gameObject == null)
        {
            return;
        }

        // Oyuncu üzerindeyse kontrol etme
        if (trackedBicycle.IsPlayerOnBoard)
        {
            return;
        }

        if (!trackedBicycle.gameObject.activeInHierarchy)
        {
            return;
        }

        Vector3 pos = trackedBicycle.transform.position;
        bool needsReset = false;
        string reason = "";

        // 1. Fall Detection (Düşme Tespiti) - Öncelikli
        if (IsFalling(pos))
        {
            needsReset = true;
            reason = $"Düşme tespiti (Y: {pos.y:F2} < {minYPosition})";
        }
        // 2. Boundary Kontrolü
        else if (!IsWithinBoundary(pos))
        {
            needsReset = true;
            reason = $"Boundary dışında (Pos: {pos})";
        }
        // 3. Yükseklik Kontrolü
        else if (pos.y > maxYPosition)
        {
            needsReset = true;
            reason = $"Çok yüksekte (Y: {pos.y:F2} > {maxYPosition})";
        }

        if (needsReset)
        {
            Debug.LogWarning($"[VehicleManager] Bisiklet resetleniyor - Sebep: {reason} - Mevcut Pozisyon: {pos}");
            ResetBicycleToHome();
        }
    }

    /// <summary>
    /// Bisikleti home position'a resetle
    /// </summary>
    public void ResetBicycleToHome()
    {
        if (trackedBicycle == null || trackedBicycle.gameObject == null) return;

        Debug.Log($"[VehicleManager] Bisiklet resetleniyor, Eski Pozisyon: {trackedBicycle.transform.position}, Yeni Pozisyon: {bicycleHomePos}");

        // Rigidbody'yi durdur
        Rigidbody rb = trackedBicycle.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Home position'a taşı
        trackedBicycle.transform.position = bicycleHomePos;
        trackedBicycle.transform.rotation = Quaternion.identity;

        Debug.Log($"[VehicleManager] Bisiklet başarıyla home position'a resetlendi");
    }

    /// <summary>
    /// Tüm motorları home position'a resetle (Debug için)
    /// </summary>
    [ContextMenu("Reset All Vehicles to Home")]
    public void ResetAllVehiclesToHome()
    {
        foreach (var kvp in vehicleTrackingData)
        {
            if (kvp.Key != null && kvp.Key.gameObject != null)
            {
                ResetMotorToHome(kvp.Key, kvp.Value);
            }
        }
        
        // Bisikleti de resetle
        if (trackedBicycle != null)
        {
            ResetBicycleToHome();
        }
    }

    // Save/Load Implementation
    public void SaveData(GameData data)
    {
        // VehicleManager motor pozisyonlarını doğrulayarak kaydeder
        // Asıl kayıt MotorcycleShop tarafından yapılıyor
        // Burada sadece pozisyon doğrulama yapıyoruz
        if (data.motorcycleData != null && trackedMotorcycles.Count > 0)
        {
            // Motor pozisyonlarını kontrol et ve geçersizse düzelt
            foreach (var kvp in trackedMotorcycles)
            {
                MotorcycleVehicle motor = kvp.Value;
                if (motor != null && motor.gameObject != null)
                {
                    Vector3 pos = motor.transform.position;
                    
                    // Geçersiz pozisyon kontrolü
                    if (!IsWithinBoundary(pos) || IsFalling(pos))
                    {
                        Debug.LogWarning($"[VehicleManager] Save öncesi geçersiz pozisyon tespit edildi, düzeltiliyor...");
                        if (vehicleTrackingData.TryGetValue(motor, out VehicleTrackingData trackingData))
                        {
                            ResetMotorToHome(motor, trackingData);
                        }
                    }
                }
            }
        }
    }

    public void LoadData(GameData data)
    {
        if (data.motorcycleData == null) return;
        
        // Load edildikten sonra motorları kontrol et
        StartCoroutine(ValidateVehiclePositionsAfterLoad());
    }

    private IEnumerator ValidateVehiclePositionsAfterLoad()
    {
        yield return new WaitForSeconds(1f); // Motorlar spawn olmasını bekle
        
        // Tüm motorları bul ve kaydet
        MotorcycleVehicle[] allMotors = FindObjectsOfType<MotorcycleVehicle>();
        foreach (var motor in allMotors)
        {
            if (motor != null && motor.gameObject.activeInHierarchy)
            {
                int index = GetMotorcycleIndex(motor);
                if (index >= 0 && !trackedMotorcycles.ContainsKey(index))
                {
                    RegisterMotorcycle(motor, index);
                }
                
                // Pozisyon kontrolü
                if (!IsWithinBoundary(motor.transform.position) || IsFalling(motor.transform.position))
                {
                    if (vehicleTrackingData.TryGetValue(motor, out VehicleTrackingData data))
                    {
                        Debug.LogWarning($"[VehicleManager] Load sonrası geçersiz pozisyon tespit edildi, resetleniyor...");
                        ResetMotorToHome(motor, data);
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Boundary görselleştirme
        Gizmos.color = Color.yellow;
        Vector3 center = (worldMin + worldMax) / 2f;
        Vector3 size = worldMax - worldMin;
        Gizmos.DrawWireCube(center, size);
        
        // Home position'ları göster
        if (motorcycleHomePositions != null)
        {
            Gizmos.color = Color.green;
            foreach (var homePos in motorcycleHomePositions)
            {
                if (homePos.homePosition != null)
                {
                    Gizmos.DrawWireSphere(homePos.homePosition.position, 1f);
                    Gizmos.DrawLine(homePos.homePosition.position, homePos.homePosition.position + Vector3.up * 2f);
                }
            }
        }
        
        // Bisiklet home position'ı göster
        if (bicycleHomePosition != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(bicycleHomePosition.position, 1.5f);
            Gizmos.DrawLine(bicycleHomePosition.position, bicycleHomePosition.position + Vector3.up * 2f);
        }
    }
}

