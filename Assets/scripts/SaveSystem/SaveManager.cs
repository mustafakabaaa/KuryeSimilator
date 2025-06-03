using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Collections;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string SAVE_PATH => Path.Combine(Application.persistentDataPath, "save.json");
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
       
       

    }
    private IEnumerator Start()
    {
        // Diğer sistemlerin Awake()'ini bekler
        yield return new WaitForEndOfFrame();

        // En az 1 sistem kaydolana kadar bekle (örneğin UpgradeSystem)
        yield return new WaitUntil(() => saveableSystems.Count > 0);

        if (HasSaveData())
            LoadGame();
        else
            InitializeDefaultData();
    }
    public bool HasSaveData()
    {
        return File.Exists(SAVE_PATH);
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
            Debug.Log($"System registered: {system.GetType().Name}");
        }
    }

    public void SaveGame()
    {
        currentGameData = new GameData();

        // 1. Önce GameDataSO'ya kaydet
        gameDataSO.SaveUpgradeState();

        // 2. GameDataSO'dan GameData'ya kopyala
        currentGameData.upgradeData = gameDataSO.savedUpgradeData;
        currentGameData.upgradePoints = gameDataSO.upgradePoints;
        currentGameData.currentStamina = staminaSystem.currentStamina;

        // 3. Diğer sistemleri kaydet (motor, pozisyon vs.)
        foreach (var system in saveableSystems)
            system.SaveData(currentGameData);

        // JSON'a yaz
        string json = JsonUtility.ToJson(currentGameData, true);
        File.WriteAllText(SAVE_PATH, json);
    }

    public void LoadGame()
    {
        if (!File.Exists(SAVE_PATH))
        {
            InitializeDefaultData();
            return;
        }

        string json = File.ReadAllText(SAVE_PATH);
        currentGameData = JsonUtility.FromJson<GameData>(json);

        // 1. GameData'dan GameDataSO'ya yükle
        gameDataSO.savedUpgradeData = currentGameData.upgradeData;
        gameDataSO.upgradePoints = currentGameData.upgradePoints;
        gameDataSO.LoadUpgradeState();  // ↑↑↑ BU SATIR ÇOK ÖNEMLİ ↑↑↑

        // 2. Stamina'yı yükle
        staminaSystem.currentStamina = currentGameData.currentStamina;

        // 3. Diğer sistemleri yükle
        foreach (var system in saveableSystems)
            system?.LoadData(currentGameData);
    }

    private void InitializeDefaultData()
    {
        currentGameData = new GameData();
        // Varsayılan verileri burada ayarlayabilirsiniz
    }

    public void DeleteSave()
    {
        if (File.Exists(SAVE_PATH))
        {
            File.Delete(SAVE_PATH);
            Debug.Log("Save file deleted");
        }
    }
}