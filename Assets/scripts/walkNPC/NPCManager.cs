using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCManager : MonoBehaviour
{
    public Transform player;
    public float activationDistance = 30f;

    [System.Serializable]
    public class PathData
    {
        public string pathID;
        public PathList pathList;
        [HideInInspector] public NPCController spawnedNPC;
    }

    public List<PathData> paths = new List<PathData>();

    private void Update()
    {
        foreach (var pathData in paths)
        {
            // PathList null ise, ID'den bulmaya çalış
            if (pathData.pathList == null)
            {
                PathList foundPath = FindPathListByID(pathData.pathID);
                if (foundPath == null || foundPath.waypoints.Count == 0)
                    continue;
                else
                    pathData.pathList = foundPath;
            }

            float distance = Vector3.Distance(pathData.pathList.transform.position, player.position);

            if (distance <= activationDistance)
            {
                // Yakındaysa ve henüz spawn edilmediyse
                if (pathData.spawnedNPC == null)
                {
                    SpawnNPCForPath(pathData);
                }
            }
            else
            {
                // Uzaklaştıysa ama NPC hâlâ varsa
                if (pathData.spawnedNPC != null)
                {
                    // NPC hâlâ oyuncunun görüşündeyse despawn etme
                    if (AdvancedOcclusionManager.Instance != null &&
                             !AdvancedOcclusionManager.Instance.IsOccluded(
                             pathData.spawnedNPC.transform.position,
                             pathData.spawnedNPC.gameObject))
                    {
                        return;
                    }


                    // Görünmüyor ve uzakta -> Despawn et
                    DespawnNPCForPath(pathData);
                }
            }
        }
    }


    private PathList FindPathListByID(string id)
    {
        PathList[] allPaths = FindObjectsOfType<PathList>();
        foreach (var p in allPaths)
        {
            if (p.pathID == id)
                return p;
        }
        return null;
    }

    private void SpawnNPCForPath(PathData pathData)
    {
        // 1. Waypoint kontrolü
        if (pathData.pathList.waypoints.Count == 0 || pathData.pathList.waypoints[0] == null)
        {
            Debug.LogError($"Path {pathData.pathID} geçersiz - waypoint yok!");
            return;
        }

        Vector3 spawnPos = pathData.pathList.waypoints[0].position;

        // 2. Oyuncunun bakıp bakmadığını kontrol et
        if (IsVisibleToPlayer(spawnPos))
        {
            // Oyuncu bu noktaya bakıyorsa spawn etme
            return;
        }

        // 3. NavMesh kontrolü
        if (!NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
        {
            Debug.LogWarning($"NavMesh üzerinde spawn pozisyonu bulunamadı: {spawnPos}");
            return;
        }

        // 4. Pool'dan al, spawn et
        NPCController npc = NPCPool.Instance.GetNPC(hit.position);
        pathData.spawnedNPC = npc;

        // 5. Ayarları yap
        npc.pathList = pathData.pathList;
        npc.ResetHealth();
        npc.SetWaypointIndex(0);
    }


    private void DespawnNPCForPath(PathData pathData)
    {
        NPCPool.Instance.ReturnNPC(pathData.spawnedNPC);
        pathData.spawnedNPC = null;
    }
    private bool IsVisibleToPlayer(Vector3 position)
    {
        Camera cam = Camera.main;
        if (cam == null) return false;

        Vector3 viewportPoint = cam.WorldToViewportPoint(position);

        // Kamera önünde mi?
        if (viewportPoint.z < 0) return false;

        // Ekran içinde mi?
        if (viewportPoint.x < 0f || viewportPoint.x > 1f || viewportPoint.y < 0f || viewportPoint.y > 1f)
            return false;

        // Raycast ile gerçekten görüyor mu?
        Vector3 direction = position - cam.transform.position;
        if (Physics.Raycast(cam.transform.position, direction, out RaycastHit hit, direction.magnitude))
        {
            // Eğer başka bir şey engelliyorsa oyuncu göremez
            if (hit.point != position && hit.collider.gameObject.layer != LayerMask.NameToLayer("NPC"))
                return false;
        }

        return true; // Kamera görüyor
    }

}