// GameData.cs
using UnityEngine;
using System;

[Serializable]
public class GameData
{
    // Karakter verileri
    public Vector3Serializable playerPosition;
    public float playerRotationY;
    public GameSaveData upgradeData;

    // Ýstatistikler
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