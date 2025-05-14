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

        // MASA varsa → Masanın üstüne rastgele XZ offset ile
        if (restaurant.tablePositions != null && restaurant.tablePositions.Count > 0)
        {
            int tableIndex = itemIndex % restaurant.tablePositions.Count;
            Vector3 basePos = restaurant.tablePositions[tableIndex].position;

            float maxOffset = 0.3f;
            float offsetX = Random.Range(-maxOffset, maxOffset);
            float offsetZ = Random.Range(-maxOffset, maxOffset);

            return new Vector3(
                basePos.x + offsetX,
                basePos.y+restaurant.verticalSpacing,
                basePos.z + offsetZ
            );
        }

        // MASA yoksa → Rastgele alan içine spawnla
        Vector3 center = restaurant.spawnAreaCenter.position;
        Vector2 size = restaurant.spawnAreaSize;

        float randomX = Random.Range(-size.x / 2f, size.x / 2f);
        float randomZ = Random.Range(-size.y / 2f, size.y / 2f);

        return new Vector3(
            center.x + randomX,
            center.y+restaurant.verticalSpacing,
            center.z + randomZ
        );
    }


}