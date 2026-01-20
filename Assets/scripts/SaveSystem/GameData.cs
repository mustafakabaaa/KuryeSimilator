// GameData.cs
using UnityEngine;
using System;
using static GameData;
using System.Collections.Generic;

public enum ArizaSeviyesi
{
    Saglam = 0,
    Hafif = 1,
    Orta = 2,
    Agir = 3
}

[Serializable]
public class OrderSaveData
{
    public List<string> completedOrderIDs = new List<string>();
    public int currentDayIndex;
}

[Serializable]
public class SleepSaveData
{
    public float currentTimeOfDay;
    public float lastSleepTime;
    public bool isSleeping;
}

[Serializable]
public class MotorcycleSaveData
{
    public bool[] purchasedMotorcycles;
    public int activeMotorcycleIndex;
    public Vector3Serializable motorcyclePosition;
    public Vector3Serializable motorcycleRotation;
    public bool isPlayerOnBike; // Add this to track if player was on bike
    public float[] motorcycleKilometres;
    public ArizaSeviyesi[] arizaSeviyeleri;
    public float[] sonArizaKontrolKm;
}

[Serializable]
public class WalletSaveData
{
    public int balance;
}

[Serializable]
public class GameData
{// Mevcut de�i�kenlerden sonra ekle
    public InventorySaveData inventorySaveData;
    // Mevcut de�i�kenlerden sonra ekle
    public ItemSaveData itemData;
    public OrderSaveData orderData;
    public SleepSaveData sleepData;
    public Vector3Serializable playerPosition;
    public float playerRotationY;
    public GameSaveData upgradeData;
    public MotorcycleSaveData motorcycleData;
    public WalletSaveData walletData;
    public int upgradePoints;
    public float currentStamina;
    public string realWorldTime; // Ger�ek d�nya saati
    public DateTime saveDateTime;

    [Serializable]
    public struct Vector3Serializable
    {
        public float x, y, z;

        public Vector3Serializable(Vector3 pos)
        {
            x = pos.x;
            y = pos.y;
            z = pos.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }
}