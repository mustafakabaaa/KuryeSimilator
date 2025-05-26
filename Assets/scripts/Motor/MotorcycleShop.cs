using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MotorcycleShop : MonoBehaviour
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
    public GameObject player;
    public GameObject playerCamera;

    public Transform spawnPoint;
    [Header("Motorcycle Settings")]
    public MotorcycleItem[] motorcycles;
    public Transform uiParent; // UI elemanlarýnýn ekleneceði parent

    [Header("UI Prefab")]
    public GameObject motorcycleUIPrefab; // Oluþturulacak UI prefabý
    private List<MotorcycleVehicle> spawnedMotors = new List<MotorcycleVehicle>();

    private void Start()
    {
        CreateMotorcycleUI();
        UpdateAllUI();

    }
    public void SpawnMotorcycle(int index)
    {
        if (index < 0 || index >= motorcycles.Length) return;

        ClearExistingMotorcycles();

        GameObject newMotor = Instantiate(motorcycles[index].motorcyclePrefab, spawnPoint.position, spawnPoint.rotation);
        MotorcycleVehicle vehicleScript = newMotor.GetComponent<MotorcycleVehicle>();

        if (vehicleScript != null)
        {
            vehicleScript._player = player;
            vehicleScript._playerCamera = playerCamera;
            spawnedMotors.Add(vehicleScript); // Listeye ekle
        }
    }

    private void ClearExistingMotorcycles()
    {
        foreach (var motor in spawnedMotors)
        {
            if (motor != null && motor.gameObject != null)
            {
                Destroy(motor.gameObject);
            }
        }
        spawnedMotors.Clear();
    }
    private void CreateMotorcycleUI()
    {
        foreach (var bike in motorcycles)
        {
            // UI elementini oluþtur
            GameObject bikeUI = Instantiate(motorcycleUIPrefab, uiParent);

            // Motor bilgilerini UI'a aktar
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
            UpdateAllUI();
            return true;
        }
        return false;
    }
}