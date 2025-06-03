// SaveManager.cs
using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    private string SAVE_PATH => Path.Combine(Application.persistentDataPath, "save.json");

    public void SaveGame(CharrController player, GameDataSO gameData, SleepStaminaSystem staminaSystem)
    {
        GameData data = new GameData();

        // 1. Pozisyon kaydet
        data.playerPosition = new GameData.Vector3Serializable(player.transform.position);
        data.playerRotationY = player.transform.eulerAngles.y;

        // 2. Ýstatistikler
        data.upgradePoints = gameData.upgradePoints;
        data.currentStamina = staminaSystem.currentStamina;

        // 3. Save upgrade data
        gameData.SaveUpgradeState(); // This updates savedUpgradeData
        data.upgradeData = gameData.savedUpgradeData;

        // JSON'a çevir ve kaydet
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SAVE_PATH, json);

        Debug.Log("Oyun kaydedildi: " + SAVE_PATH);
    }
    public void LoadGameAndApply(GameDataSO gameData)
    {
        GameData loadedData = LoadGame();
        if (loadedData == null) return;

        gameData.LoadUpgradeState();

        CharacterProgressionManager.Instance.ReloadStatsFromGameData(); // <-- burasý önemli

        gameData.upgradePoints = loadedData.upgradePoints;
    }

    public void DeleteSave()
    {
        if (File.Exists(SAVE_PATH))
        {
            File.Delete(SAVE_PATH);
            Debug.Log("Kayýt dosyasý silindi.");
        }
        else
        {
            Debug.LogWarning("Silinecek kayýt dosyasý bulunamadý.");
        }
    }

    public GameData LoadGame()
    {
        if (File.Exists(SAVE_PATH))
        {
            string json = File.ReadAllText(SAVE_PATH);
            GameData data = JsonUtility.FromJson<GameData>(json);

            // Make sure upgradeData isn't null
            if (data.upgradeData == null)
            {
                data.upgradeData = new GameSaveData();
            }

            return data;
        }
        return null;
    }
}