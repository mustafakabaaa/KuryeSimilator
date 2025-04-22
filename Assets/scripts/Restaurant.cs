using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Restaurant
{
    public string restaurantID;
    public string restaurantName;
    public Transform spawnAreaCenter; // Restoranýn merkez noktasý
    public Vector2 spawnAreaSize = new Vector2(5f, 5f); // Spawn alaný boyutu
    public float verticalSpacing = 0.5f;
    public List<Transform> tablePositions; // Masalarýn pozisyonlarý (opsiyonel)
}