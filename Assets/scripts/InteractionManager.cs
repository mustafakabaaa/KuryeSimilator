using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Sadece bisiklet ve motor için interaction text yönetimi
/// </summary>
public class InteractionManager : MonoBehaviour
{
    private const string UiTable = "UI";
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI interactText;
    
    [Header("Settings")]
    [SerializeField] private float maxInteractionDistance = 5f;
    [SerializeField] private bool enableDebugLogs = false;
    
    // Singleton pattern
    public static InteractionManager Instance { get; private set; }
    
    // Vehicle tracking
    private List<VehicleData> nearbyVehicles = new List<VehicleData>();
    private VehicleData currentVehicle;
    private Transform playerTransform;

    // Oyuncu aynı anda tek araca binebilir. Motor ve bisiklet F'yi ayrı dinlediği
    // için yan yana durunca ikisi de binme sayılabiliyordu; bu kilit onu önler.
    // Player may occupy only one vehicle. Bike and motorcycle both listen to F,
    // so standing between them used to mount both; this lock prevents that.
    private static MonoBehaviour occupiedVehicle;

    /// <summary>Başka bir araçta oyuncu var mı? / Is any vehicle already occupied?</summary>
    public static bool HasOccupiedVehicle => occupiedVehicle != null;

    /// <summary>
    /// Bu aracı "dolu" olarak işaretler. Başka araç doluysa false döner.
    /// Claims this vehicle as occupied. Returns false if another vehicle is already claimed.
    /// </summary>
    public static bool TryClaimVehicle(MonoBehaviour vehicle)
    {
        if (vehicle == null) return false;
        if (occupiedVehicle != null && occupiedVehicle != vehicle) return false;
        occupiedVehicle = vehicle;
        return true;
    }

    /// <summary>
    /// İnerken kilidi açar, böylece başka bir araca binilebilir.
    /// Releases the occupancy lock so another vehicle can be mounted.
    /// </summary>
    public static void ReleaseVehicle(MonoBehaviour vehicle)
    {
        if (occupiedVehicle == vehicle)
            occupiedVehicle = null;
    }
    
    [System.Serializable]
    private class VehicleData
    {
        public MonoBehaviour vehicle; // BicycleVehicle veya MotorcycleVehicle
        public Transform transform;
        public float distance;
        public string interactionText;
        public bool canInteract;
        
        public VehicleData(MonoBehaviour vehicle, Transform transform, float distance, string text, bool canInteract)
        {
            this.vehicle = vehicle;
            this.transform = transform;
            this.distance = distance;
            this.interactionText = text;
            this.canInteract = canInteract;
        }
    }

    private void Awake()
    {
        InitializeSingleton();
        FindPlayerTransform();
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning($"Multiple InteractionManager instances detected. Destroying {gameObject.name}");
            Destroy(gameObject);
        }
    }

    private void FindPlayerTransform()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure player has 'Player' tag.");
        }
    }

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (interactText == null)
        {
            interactText = GameObject.FindGameObjectWithTag("MotorText")?.GetComponent<TextMeshProUGUI>();
            if (interactText == null)
            {
                Debug.LogError("MotorText not found! Make sure UI has 'MotorText' tag.");
            }
        }
        
        HideInteractionText();
    }

    private void Update()
    {
        if (playerTransform == null) return;
        
        UpdateNearbyVehicles();
        UpdateCurrentVehicle();
        UpdateUI();
    }

    /// <summary>
    /// Araç kaydı
    /// </summary>
    public void RegisterVehicle(MonoBehaviour vehicle, Transform vehicleTransform, string interactionText, bool canInteract)
    {
        if (vehicle == null || vehicleTransform == null) return;
        
        // Sadece geçerli araç tiplerini kabul et
        if (!IsValidVehicle(vehicle)) return;
        
        // Zaten kayıtlı mı kontrol et
        for (int i = 0; i < nearbyVehicles.Count; i++)
        {
            if (nearbyVehicles[i].vehicle == vehicle)
            {
                // Mevcut kaydı güncelle
                float distance = Vector3.Distance(playerTransform.position, vehicleTransform.position);
                nearbyVehicles[i] = new VehicleData(vehicle, vehicleTransform, distance, interactionText, canInteract);
                return;
            }
        }
        
        // Yeni kayıt ekle
        float newDistance = Vector3.Distance(playerTransform.position, vehicleTransform.position);
        if (newDistance <= maxInteractionDistance)
        {
            VehicleData newData = new VehicleData(vehicle, vehicleTransform, newDistance, interactionText, canInteract);
            nearbyVehicles.Add(newData);
            LogDebug($"Registered vehicle: {vehicleTransform.name} at distance {newDistance:F2}");
        }
    }

    /// <summary>
    /// Araç kaydını kaldır
    /// </summary>
    public void UnregisterVehicle(MonoBehaviour vehicle)
    {
        for (int i = nearbyVehicles.Count - 1; i >= 0; i--)
        {
            if (nearbyVehicles[i].vehicle == vehicle)
            {
                nearbyVehicles.RemoveAt(i);
                LogDebug($"Unregistered vehicle: {vehicle}");
                break;
            }
        }
    }

    private bool IsValidVehicle(MonoBehaviour vehicle)
    {
        // Sadece BicycleVehicle ve MotorcycleVehicle kabul et
        return vehicle is BicycleVehicle || vehicle is MotorcycleVehicle;
    }

    private void UpdateNearbyVehicles()
    {
        // Mesafeleri güncelle ve menzil dışındaki araçları kaldır
        for (int i = nearbyVehicles.Count - 1; i >= 0; i--)
        {
            if (nearbyVehicles[i].transform == null)
            {
                nearbyVehicles.RemoveAt(i);
                continue;
            }
            
            float distance = Vector3.Distance(playerTransform.position, nearbyVehicles[i].transform.position);
            nearbyVehicles[i].distance = distance;
            
            // Çok uzak veya etkileşim yapılamıyorsa kaldır
            if (distance > maxInteractionDistance || !nearbyVehicles[i].canInteract)
            {
                nearbyVehicles.RemoveAt(i);
                LogDebug($"Removed vehicle: distance {distance:F2} > {maxInteractionDistance}");
            }
        }
    }

    private void UpdateCurrentVehicle()
    {
        if (nearbyVehicles.Count == 0)
        {
            currentVehicle = null;
            return;
        }
        
        // Mesafeye göre sırala (en yakın önce)
        nearbyVehicles.Sort((a, b) => a.distance.CompareTo(b.distance));
        
        currentVehicle = nearbyVehicles[0];
        LogDebug($"Current vehicle: {currentVehicle.transform.name} at distance {currentVehicle.distance:F2}");
    }

    private void UpdateUI()
    {
        if (currentVehicle == null)
        {
            HideInteractionText();
            return;
        }
        
        ShowInteractionText(currentVehicle.interactionText);
    }

    /// <summary>
    /// Interaction text'ini göster
    /// </summary>
    public void ShowInteractionText(string text)
    {
        if (interactText != null && !string.IsNullOrEmpty(text))
        {
            interactText.text = LocalizationHelper.Localize(UiTable, text);
            interactText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Interaction text'ini gizle
    /// </summary>
    public void HideInteractionText()
    {
        if (interactText != null)
        {
            interactText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Mevcut en yakın aracı al
    /// </summary>
    public MonoBehaviour GetCurrentVehicle()
    {
        return currentVehicle?.vehicle;
    }

    /// <summary>
    /// Aktif araç interaction'ı var mı kontrol et
    /// </summary>
    public bool HasActiveVehicleInteraction()
    {
        return currentVehicle != null;
    }

    /// <summary>
    /// Mevcut araç interaction text'ini al
    /// </summary>
    public string GetCurrentVehicleInteractionText()
    {
        return currentVehicle?.interactionText ?? "";
    }

    /// <summary>
    /// Tüm kayıtlı araçları temizle (sahne geçişleri için)
    /// </summary>
    public void ClearAllVehicles()
    {
        nearbyVehicles.Clear();
        currentVehicle = null;
        HideInteractionText();
        LogDebug("All vehicles cleared");
    }

    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[InteractionManager] {message}");
        }
    }

    // Debug görselleştirme
    private void OnDrawGizmosSelected()
    {
        if (playerTransform == null) return;
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(playerTransform.position, maxInteractionDistance);
        
        // Yakındaki araçlara çizgiler çiz
        foreach (var vehicle in nearbyVehicles)
        {
            if (vehicle.transform != null)
            {
                Color lineColor = Color.cyan;
                if (vehicle == currentVehicle)
                {
                    lineColor = Color.red; // Mevcut araç
                }
                
                Gizmos.color = lineColor;
                Gizmos.DrawLine(playerTransform.position, vehicle.transform.position);
            }
        }
    }
}
