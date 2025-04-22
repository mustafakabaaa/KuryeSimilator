using System.Collections.Generic;
using UnityEngine;

public class RestaurantManager : MonoBehaviour
{
    public static RestaurantManager Instance;

    public List<Restaurant> restaurants = new List<Restaurant>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Vector3 GetSpawnPosition(string restaurantID, int itemIndex, int totalItems)
    {
        Restaurant restaurant = restaurants.Find(r => r.restaurantID == restaurantID);
        if (restaurant == null) return Vector3.zero;

        // Eðer tanýmlý masa pozisyonlarý varsa onlarý kullan
        if (restaurant.tablePositions != null && restaurant.tablePositions.Count > 0)
        {
            int tableIndex = itemIndex % restaurant.tablePositions.Count;
            return restaurant.tablePositions[tableIndex].position;
        }

        // Yoksa otomatik grid daðýlýmý yap
        int itemsPerRow = Mathf.CeilToInt(Mathf.Sqrt(totalItems));
        float xStep = restaurant.spawnAreaSize.x / itemsPerRow;
        float zStep = restaurant.spawnAreaSize.y / itemsPerRow;

        int row = itemIndex / itemsPerRow;
        int col = itemIndex % itemsPerRow;

        return restaurant.spawnAreaCenter.position +
               new Vector3(
                   col * xStep - restaurant.spawnAreaSize.x / 2 + xStep / 2,
                   row * restaurant.verticalSpacing,
                   row * zStep - restaurant.spawnAreaSize.y / 2 + zStep / 2
               );
    }
}