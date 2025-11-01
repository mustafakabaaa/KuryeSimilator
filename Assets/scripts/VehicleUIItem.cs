using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Araç UI item'ı - Her araç için list item
/// </summary>
public class VehicleUIItem : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI vehicleNameText;
    public Image vehicleIconImage;
    public Button callButton;

    private VehicleType vehicleType;
    private int motorcycleIndex = -1; // Bicycle için -1, motor için index

    private void Start()
    {
        if (callButton != null)
        {
            callButton.onClick.AddListener(OnCallVehicleClicked);
        }
    }

    /// <summary>
    /// Item'ı kur
    /// </summary>
    public void Setup(string name, Sprite icon, VehicleType type, int motorIndex)
    {
        vehicleType = type;
        motorcycleIndex = motorIndex;

        // UI'yi güncelle
        if (vehicleNameText != null)
        {
            vehicleNameText.text = name;
        }

        if (vehicleIconImage != null && icon != null)
        {
            vehicleIconImage.sprite = icon;
        }
    }

    /// <summary>
    /// Araç çağırma butonuna tıklandığında
    /// </summary>
    private void OnCallVehicleClicked()
    {
        if (vehicleType == VehicleType.Bicycle)
        {
            CallBicycle();
        }
        else if (vehicleType == VehicleType.Motorcycle)
        {
            CallMotorcycle(motorcycleIndex);
        }
    }

    /// <summary>
    /// Bisikleti çağır
    /// </summary>
    private void CallBicycle()
    {
        if (VehicleManager.Instance == null) return;

        BicycleVehicle bicycle = FindObjectOfType<BicycleVehicle>();
        if (bicycle != null)
        {
            // Bisikleti player'ın yanına getir
            Vector3 playerPos = GameObject.FindGameObjectWithTag("Player").transform.position;
            Vector3 spawnPos = playerPos + new Vector3(2f, 0f, 2f); // Biraz yanında spawn et

            bicycle.transform.position = spawnPos;
            bicycle.transform.rotation = Quaternion.identity;

            // Rigidbody'yi durdur
            Rigidbody rb = bicycle.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Debug.Log("[VehiclesUI] Bisiklet çağrıldı!");
            
            // Toast mesajı göster
            if (ToastManager.Instance != null)
            {
                ToastManager.Instance.ShowToast("Bisiklet çağrıldı!");
            }
        }
        else
        {
            Debug.LogWarning("[VehiclesUI] Bisiklet bulunamadı!");
        }
    }

    /// <summary>
    /// Motoru çağır
    /// </summary>
    private void CallMotorcycle(int index)
    {
        if (VehicleManager.Instance == null) return;

        MotorcycleShop shop = FindObjectOfType<MotorcycleShop>();
        if (shop == null) return;
        if (index < 0 || index >= shop.motorcycles.Length) return;

        // Motorun home position'ını al
        Vector3 motorHomePos = VehicleManager.Instance.GetHomePosition(index);

        // Sahnedeki tüm motorları bul
        MotorcycleVehicle[] allMotors = FindObjectsOfType<MotorcycleVehicle>();
        
        foreach (var motor in allMotors)
        {
            if (motor != null && motor.gameObject != null)
            {
                // Bu motorun index'ini kontrol et
                string motorName = motor.gameObject.name.Replace("(Clone)", "").Trim();
                string prefabName = shop.motorcycles[index].motorcyclePrefab.name;
                
                if (motorName == prefabName)
                {
                    // Motoru player'ın yanına getir
                    Vector3 playerPos = GameObject.FindGameObjectWithTag("Player").transform.position;
                    Vector3 spawnPos = playerPos + new Vector3(2f, 0f, 2f); // Biraz yanında spawn et

                    motor.transform.position = spawnPos;
                    motor.transform.rotation = Quaternion.identity;

                    // Rigidbody'yi durdur
                    Rigidbody rb = motor.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.velocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }

                    Debug.Log($"[VehiclesUI] Motor {index} çağrıldı!");
                    
                    // Toast mesajı göster
                    if (ToastManager.Instance != null)
                    {
                        ToastManager.Instance.ShowToast($"{shop.motorcycles[index].name} çağrıldı!");
                    }

                    return;
                }
            }
        }

        Debug.LogWarning($"[VehiclesUI] Motor {index} bulunamadı!");
    }
}

