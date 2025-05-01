using UnityEngine;

public class MinimapPlayerIcon : MonoBehaviour
{
    [Header("References")]
    public Transform player;                        // Oyuncunun world üzerindeki transform'u
    public RectTransform minimapIcon;               // UI üzerindeki ok simgesi (oyuncuyu temsil eden)
    public Camera minimapCamera;
    public RectTransform minimapRect;

    [Header("Settings")]
    public bool rotateWithPlayer = true;
    public float positionOffset = 50f;

    void Update()
    {
        if (player == null || minimapCamera == null) return;

        // Oyuncunun pozisyonunu minimap'e çevir
        Vector3 viewportPos = minimapCamera.WorldToViewportPoint(player.position);

        Vector2 mapSize = minimapRect.rect.size;
        Vector2 uiOffset = new Vector2(mapSize.x / 2f, mapSize.y / 2f);
        Vector2 iconPos = new Vector2(
            (viewportPos.x * mapSize.x) - uiOffset.x,
            (viewportPos.y * mapSize.y) - uiOffset.y
        );

        // Pozisyonu sýnýrla ve ata
        float clampX = Mathf.Clamp(iconPos.x, -uiOffset.x + positionOffset, uiOffset.x - positionOffset);
        float clampY = Mathf.Clamp(iconPos.y, -uiOffset.y + positionOffset, uiOffset.y - positionOffset);
        minimapIcon.anchoredPosition = new Vector2(clampX, clampY);

        // Oyuncu yönüne göre oku döndür
        if (rotateWithPlayer)
        {
            float angle = player.eulerAngles.y;
            minimapIcon.localRotation = Quaternion.Euler(0f, 0f, -angle+180);
        }
    }
}
