using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;

    public SaveManager saveManager;
    public GameDataSO gameData;
    public SleepStaminaSystem staminaSystem;

    private CharrController player;

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

    private void Start()
    {
        player = FindObjectOfType<CharrController>();
    }

    public void SaveGame()
    {
        if (player == null)
            player = FindObjectOfType<CharrController>();

        saveManager.SaveGame(player, gameData, staminaSystem);
        gameData.SaveUpgradeState();
        player.SaveGameState();
    }

    // SaveSystem.cs'de LoadGame metodunu þu þekilde güncelleyin:
    public void LoadGame()
    {
        var data = saveManager.LoadGame();
        if (data != null)
        {
            gameData.upgradePoints = data.upgradePoints;
            gameData.savedUpgradeData = data.upgradeData ?? new GameSaveData();

            // ÖNEMLÝ: LoadUpgradeState'i doðru sýrada çaðýr
            gameData.LoadUpgradeState();
            CharacterProgressionManager.Instance?.ReloadStatsFromGameData();

            if (player == null)
                player = FindObjectOfType<CharrController>();

            player?.LoadGameState();
            staminaSystem.currentStamina = data.currentStamina;
        }
    }
}
