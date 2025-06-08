// GameData.cs
using UnityEngine;
using System;
using static GameData;

[Serializable]
public class MotorcycleSaveData
{
    public bool[] purchasedMotorcycles;
    public int activeMotorcycleIndex;
    public Vector3Serializable motorcyclePosition;
    public Vector3Serializable motorcycleRotation;
    public bool isPlayerOnBike; // Add this to track if player was on bike
}

[Serializable]
public class GameData
{
    public Vector3Serializable playerPosition;
    public float playerRotationY;

    public GameSaveData upgradeData;
    public MotorcycleSaveData motorcycleData;

    public int upgradePoints;
    public float currentStamina;
    public string realWorldTime; // Gerçek dünya saati
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
