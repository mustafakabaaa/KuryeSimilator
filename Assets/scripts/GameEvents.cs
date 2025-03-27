using UnityEngine;
using System;

public class GameEvents : MonoBehaviour
{
    // Singleton YERINE sahne objesine eriþim (Inspector'den elle atanacak)
    public static GameEvents Instance; // Statik eriþim için

    void Awake()
    {
        // Instance'ý bu objeye ata
        Instance = this;
    }

    // Eventler
    public event Action<CharacterStat, float> OnStatUpdated;
    public void TriggerStatUpdate(CharacterStat stat, float value) => OnStatUpdated?.Invoke(stat, value);

    public event Action OnUpgradePointsChanged;
    public void TriggerPointsUpdate() => OnUpgradePointsChanged?.Invoke();

}