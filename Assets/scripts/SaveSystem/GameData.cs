// GameData.cs
using UnityEngine;
using System;
using static GameData;

[Serializable]
public class MotorcycleSaveData
{
    public bool[] purchasedMotorcycles; // Hangi motorlarýn satýn alýndýðý
    public int activeMotorcycleIndex; // Aktif motor indexi
    public Vector3Serializable motorcyclePosition; // Motorun pozisyonu
    public Vector3Serializable motorcycleRotation; // Motorun rotasyonu
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
