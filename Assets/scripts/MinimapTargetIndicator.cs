using UnityEngine;

public class MinimapTargetIndicator : MonoBehaviour
{
    [Header("References")]
    public Transform target; // Görev nesnesi
    public Transform player; // Oyuncu
    public RectTransform minimapRect;
    public RectTransform indicatorIcon;

    [Header("Settings")]
    public float margin = 20f; // Minimap kenarýndan boþluk

    void Update()
    {
        if (target == null || player == null || minimapRect == null || indicatorIcon == null) return;

        // Dünya üzerindeki yön vektörü
        Vector3 toTarget = (target.position - player.position).normalized;
        float angleToTarget = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;

        // Oyuncunun bakýþ yönü (y ekseni rotasyonu)
        float playerYaw = player.eulerAngles.y;

        // Göreli açý
        float relativeAngle = Mathf.DeltaAngle(playerYaw, angleToTarget);
        float rad = relativeAngle * Mathf.Deg2Rad;

        // Yarýçap hesapla
        float radius = (minimapRect.rect.width / 2f) - margin;

        // Pozisyonu ayarla
        float x = Mathf.Sin(rad) * radius;
        float y = Mathf.Cos(rad) * radius;
        indicatorIcon.anchoredPosition = new Vector2(x, y);

        // (Ýsteðe baðlý) ikonun yönünü oyuncuya göre döndür
        indicatorIcon.localEulerAngles = new Vector3(0, 0, -relativeAngle);
    }
}
