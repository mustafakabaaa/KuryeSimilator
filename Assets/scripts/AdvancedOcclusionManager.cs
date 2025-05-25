using UnityEngine;

public class AdvancedOcclusionManager : MonoBehaviour
{
    public static AdvancedOcclusionManager Instance { get; private set; }

    public Camera playerCamera;
    public LayerMask occlusionLayers;
    public float checkInterval = 0.3f;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (!playerCamera) playerCamera = Camera.main;
    }

    public bool IsOccluded(Vector3 targetPosition, GameObject targetObject)
    {
        // 1. Viewport testi (ekranda görünüyor mu)
        Vector3 viewportPos = playerCamera.WorldToViewportPoint(targetPosition);
        if (viewportPos.z < 0 || viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
            return true; // Ekran dýþýnda

        // 2. Frustum testi
        if (!GeometryUtility.TestPlanesAABB(
            GeometryUtility.CalculateFrustumPlanes(playerCamera),
            new Bounds(targetPosition, Vector3.one * 2f)))
            return true; // Kamera frustumunda deðil

        // 3. Raycast occlusion test
        Vector3 rayDirection = targetPosition - playerCamera.transform.position;
        float distance = rayDirection.magnitude;
        rayDirection.Normalize();

        if (Physics.Raycast(playerCamera.transform.position, rayDirection,
            out RaycastHit hit, distance, occlusionLayers))
        {
            if (hit.collider.gameObject != targetObject)
                return true; // Baþka bir obje tarafýndan engelleniyor
        }

        return false; // Görünür durumda
    }
}