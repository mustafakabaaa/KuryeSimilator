using UnityEngine;
using System.Collections.Generic;

public class WaypointManager : MonoBehaviour
{
    public GameObject Path;
    private List<Transform> waypoints = new List<Transform>();

    private void Start()
    {
        if (Path != null)
        {
            // Path GameObject'inden PathList component'ini al
            PathList pathList = Path.GetComponent<PathList>();
            if (pathList != null)
            {
                // PathList'teki waypoints listesini kendi listemize ata
                waypoints = new List<Transform>(pathList.waypoints);

                // Kontrol için waypoints'leri yazdýr
                foreach (Transform waypoint in waypoints)
                {
                    Debug.Log(waypoint.name);
                }
            }
            else
            {
                Debug.LogError("Path GameObject'inde PathList component'i bulunamadý!");
            }
        }
        else
        {
            Debug.LogError("Path GameObject'i atanmamýþ!");
        }
    }
    void OnDrawGizmos()
    {
        if (waypoints.Count == 0) return;

        // Waypoint'ler arasýnda çizgi çiz
        Gizmos.color = Color.green; // Çizgi rengi
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;

            // Mevcut waypoint'ten bir sonraki waypoint'e çizgi çiz
            if (i < waypoints.Count - 1 && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
            // Son waypoint'ten ilk waypoint'e çizgi çiz (döngüyü tamamla)
            else if (i == waypoints.Count - 1 && waypoints[0] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
            }
        }

        // Waypoint'leri küçük kürelerle göster
        Gizmos.color = Color.red; // Waypoint rengi
        foreach (Transform waypoint in waypoints)
        {
            if (waypoint != null)
            {
                Gizmos.DrawSphere(waypoint.position, 0.1f); // Küçük bir küre çiz
            }
        }
    }

    public Transform GetNextWaypoint(ref int currentIndex)
    {
        if (waypoints.Count == 0)
        {
            Debug.LogWarning("No waypoints assigned!");
            return null;
        }

        currentIndex = (currentIndex + 1) % waypoints.Count;
        return waypoints[currentIndex];
    }

    public Transform GetCurrentWaypoint(int currentIndex)
    {
        if (waypoints.Count == 0)
        {
            Debug.LogWarning("No waypoints assigned!");
            return null;
        }

        return waypoints[currentIndex];
    }
}