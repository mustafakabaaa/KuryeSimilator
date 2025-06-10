using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private List<ISaveable> saveableSystems = new List<ISaveable>();
    private GameData currentGameData;

    [Header("References")]
    public GameDataSO gameDataSO;
    public SleepStaminaSystem staminaSystem;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("SaveManager Instance created");
    }

    private IEnumerator Start()
    {
        // FIXED: Give more time for systems to register
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.1f); // Additional delay
                                               // OrderManager'ı bul ve kaydet
        var orderManager = FindObjectOfType<OrderManager>();
        if (orderManager != null && !saveableSystems.Contains(orderManager))
        {
            RegisterSystem(orderManager);
        }
        // Manually register GameDataSO if not already registered
        if (gameDataSO != null && !saveableSystems.Contains(gameDataSO))
        {
            RegisterSystem(gameDataSO);
        }

        // Wait for at least one system to register
        yield return new WaitUntil(() => saveableSystems.Count > 0);

        Debug.Log($"SaveManager Start completed. Registered systems: {saveableSystems.Count}");

        // Auto-load logic
        if (HasAnySaveData())
        {
            string lastSave = GetLastSaveFileName();
            if (!string.IsNullOrEmpty(lastSave))
            {
                LoadSpecificSave(lastSave);
            }
            else
            {
                InitializeDefaultData();
            }
        }
        else
        {
            InitializeDefaultData();
        }
    }
    // SaveManager.cs içine ekle
    public void ResetGameData()
    {
        InitializeDefaultData();

        // OrderManager'ı bul ve resetle
        var orderManager = FindObjectOfType<OrderManager>();
        if (orderManager != null)
        {
            orderManager.ResetAllOrders();
        }

        Debug.Log("Game data reset complete");
    }
    public bool HasAnySaveData()
    {
        string[] files = Directory.GetFiles(Application.persistentDataPath, "*.json");
        return files.Length > 0;
    }

    public bool HasSaveData()
    {
        return HasAnySaveData();
    }

    public void LoadGame()
    {
        if (HasAnySaveData())
        {
            string lastSave = GetLastSaveFileName();
            if (!string.IsNullOrEmpty(lastSave))
            {
                LoadSpecificSave(lastSave);
            }
        }
        else
        {
            Debug.Log("No save files found");
        }
    }

    private string GetLastSaveFileName()
    {
        string[] files = Directory.GetFiles(Application.persistentDataPath, "*.json");

        if (files.Length == 0) return null;

        string lastFile = files[0];
        System.DateTime lastTime = File.GetLastWriteTime(lastFile);

        foreach (string file in files)
        {
            System.DateTime fileTime = File.GetLastWriteTime(file);
            if (fileTime > lastTime)
            {
                lastTime = fileTime;
                lastFile = file;
            }
        }

        return Path.GetFileNameWithoutExtension(lastFile);
    }

    public GameData GetCurrentGameData()
    {
        return currentGameData;
    }

    public void RegisterSystem(ISaveable system)
    {
        if (!saveableSystems.Contains(system))
        {
            saveableSystems.Add(system);
            Debug.Log($"System registered: {system.GetType().Name} - Total systems: {saveableSystems.Count}");
        }
        else
        {
            Debug.Log($"System already registered: {system.GetType().Name}");
        }
    }

    // FIXED: Cleaner save process
    public void SaveGame(string saveName = "default")
    {
        Debug.Log($"SaveGame called with name: {saveName}");

        currentGameData = new GameData();
        currentGameData.saveDateTime = DateTime.Now;
        currentGameData.realWorldTime = DateTime.Now.ToString("HH:mm:ss");

        Debug.Log($"Registered systems count: {saveableSystems.Count}");

        // Save all registered systems
        foreach (var system in saveableSystems)
        {
            try
            {
                Debug.Log($"Saving system: {system.GetType().Name}");
                system.SaveData(currentGameData);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving system {system.GetType().Name}: {e.Message}");
            }
        }

        // Save MotorcycleShop separately if needed
        var motorcycleShop = FindObjectOfType<MotorcycleShop>();
        if (motorcycleShop != null && !saveableSystems.Contains(motorcycleShop))
        {
            Debug.Log("Saving MotorcycleShop data");
            motorcycleShop.SaveData(currentGameData);
        }

        // Write to file
        string savePath = Path.Combine(Application.persistentDataPath, $"{saveName}.json");
        string json = JsonUtility.ToJson(currentGameData, true);
        File.WriteAllText(savePath, json);

        Debug.Log($"Game saved to: {savePath}");
        Debug.Log($"Final save data - upgradePoints: {currentGameData.upgradePoints}");
    }

    public void LoadSpecificSave(string saveName)
    {
        string savePath = Path.Combine(Application.persistentDataPath, $"{saveName}.json");

        if (!File.Exists(savePath))
        {
            Debug.LogWarning($"Save file not found: {savePath}");
            InitializeDefaultData();
            return;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            currentGameData = JsonUtility.FromJson<GameData>(json);

            Debug.Log($"Loading save: {saveName}");
            Debug.Log($"Loaded upgradePoints: {currentGameData.upgradePoints}");

            // Load data for all registered systems
            foreach (var system in saveableSystems)
            {
                try
                {
                    Debug.Log($"Loading data for system: {system.GetType().Name}");
                    system.LoadData(currentGameData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error loading system {system.GetType().Name}: {e.Message}");
                }
            }

            // Load stamina if available
            if (staminaSystem != null)
            {
                staminaSystem.currentStamina = currentGameData.currentStamina;
            }

            Debug.Log($"Save loaded successfully: {saveName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading save {saveName}: {e.Message}");
            InitializeDefaultData();
        }
    }

    private void InitializeDefaultData()
    {
        currentGameData = new GameData();
        Debug.Log("Default data initialized");
    }

    public void DeleteSave(string saveName)
    {
        string savePath = Path.Combine(Application.persistentDataPath, $"{saveName}.json");

        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log($"Save deleted: {saveName}");
        }
    }

    public List<string> GetAllSaveFiles()
    {
        List<string> saveFiles = new List<string>();
        string[] files = Directory.GetFiles(Application.persistentDataPath, "*.json");

        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            saveFiles.Add(fileName);
        }

        return saveFiles;
    }
}