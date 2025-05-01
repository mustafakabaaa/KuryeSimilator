using System.Collections.Generic;
using UnityEngine;

public class MarkerHolder : MonoBehaviour
{
    public GameObject markerPrefab;
    public Transform player;
    public RectTransform minimapRect;
    public Camera minimapCamera;

    private List<(ObjectivePosition objective, RectTransform marker)> markers = new();

    void Update()
    {
        foreach (var (objective, marker) in markers)
        {
            if (objective == null || marker == null) continue;

            // Oyuncudan hedefe doðru vektör
            Vector3 worldDir = objective.transform.position - player.position;

            // Dünya düzleminde düzleþtir
            Vector2 flatDir = new Vector2(worldDir.x, worldDir.z).normalized;

            Vector2 minimapSize = minimapRect.rect.size;
            Vector2 offset = flatDir * (minimapSize.x / 2f - 30f);

            marker.anchoredPosition = offset;

            float angle = Mathf.Atan2(flatDir.x, flatDir.y) * Mathf.Rad2Deg;
            marker.localRotation = Quaternion.Euler(0, 0, -angle);
        }
    }


    public void AddObjectiveMarker(ObjectivePosition objective)
    {
        RectTransform marker = Instantiate(markerPrefab, minimapRect).GetComponent<RectTransform>();
        markers.Add((objective, marker));
    }

    public void RemoveObjectiveMarker(ObjectivePosition objective)
    {
        int index = markers.FindIndex(m => m.objective == objective);
        if (index >= 0)
        {
            Destroy(markers[index].marker.gameObject);
            markers.RemoveAt(index);
        }
    }
}
