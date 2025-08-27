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
        if (SaveManager.Instance.HasAnySaveData()) // HasSaveData() yerine HasAnySaveData()
        {
            LoadMotorcycleFromSave();
        }
        else
        {
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

    public void SpawnMotorcycle(int index)
    {
        if (index < 0 || index >= motorcycles.Length) return;
        if (!motorcycles[index].isPurchased) return;

        ClearExistingMotorcycles();

        GameObject newMotor = Instantiate(
            motorcycles[index].motorcyclePrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        MotorcycleVehicle vehicleScript = newMotor.GetComponent<MotorcycleVehicle>();
        if (vehicleScript != null)
        {
            vehicleScript._player = player;
            vehicleScript._playerCamera = playerCamera;
            spawnedMotors.Add(vehicleScript);
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

        if (spawnedMotors.Count > 0 && spawnedMotors[0] != null)
        {
            data.motorcycleData.activeMotorcycleIndex = GetActiveMotorcycleIndex();
            data.motorcycleData.motorcyclePosition = new Vector3Serializable(spawnedMotors[0].transform.position);
            data.motorcycleData.motorcycleRotation = new Vector3Serializable(spawnedMotors[0].transform.eulerAngles);
            data.motorcycleData.isPlayerOnBike = spawnedMotors[0].IsPlayerOnBoard;
        }
        else
        {
            data.motorcycleData.activeMotorcycleIndex = -1;
            data.motorcycleData.isPlayerOnBike = false;
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

        // Sahnedeki tüm motor objelerini bul
        MotorcycleVehicle[] allMotors = FindObjectsOfType<MotorcycleVehicle>();
        
        // Motor satın alınmamışsa motor objelerini devre dışı bırak
        bool anyMotorPurchased = motorcycles.Any(m => m.isPurchased);
        if (!anyMotorPurchased)
        {
            foreach (var motor in allMotors)
            {
                if (motor != null && motor.gameObject != null)
                {
                    motor.gameObject.SetActive(false);
                }
            }
        }

        // Spawn active motorcycle if one was active
        if (data.motorcycleData.activeMotorcycleIndex >= 0 &&
            data.motorcycleData.activeMotorcycleIndex < motorcycles.Length &&
            motorcycles[data.motorcycleData.activeMotorcycleIndex].isPurchased)
        {
            ClearExistingMotorcycles();
            SpawnMotorcycle(data.motorcycleData.activeMotorcycleIndex);

            if (spawnedMotors.Count > 0 && spawnedMotors[0] != null)
            {
                StartCoroutine(SetMotorTransformAfterFrame(data.motorcycleData));

                // Restore player on bike state if needed
                if (data.motorcycleData.isPlayerOnBike)
                {
                    spawnedMotors[0].MountMotorcycle();
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

    private IEnumerator SetMotorTransformAfterFrame(MotorcycleSaveData data)
    {
        yield return new WaitForFixedUpdate();

        if (spawnedMotors.Count == 0 || spawnedMotors[0] == null) yield break;

        spawnedMotors[0].transform.position = data.motorcyclePosition.ToVector3();
        spawnedMotors[0].transform.rotation = Quaternion.Euler(data.motorcycleRotation.ToVector3());

        var rb = spawnedMotors[0].GetComponent<Rigidbody>();
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