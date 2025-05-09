using UnityEngine;

public class LimitedHeadLook : MonoBehaviour
{
    public Transform cameraPivot; // Bisikletle birlikte hareket eden pivot
    public float sensitivity = 2f;
    public float maxYaw = 45f;  // saða/sola sýnýrlý bakýþ
    public float maxPitch = 20f; // yukarý/aþaðý sýnýrlý bakýþ

    private float yaw = 0f;
    private float pitch = 0f;

    void Update()
    {
        //herhangý býr uý nesnesý acýldýgýnda cameranýn hareketýný durdurur.
        if (UIManager.Instance.IsAnyUIOpen() || Time.timeScale == 0f)
            return;

        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        yaw += mouseX;
        pitch -= mouseY;

        yaw = Mathf.Clamp(yaw, -maxYaw, maxYaw);
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void LateUpdate()
    {
        // Kamera bisikletle hareket eden pivotun pozisyonunda kalýr
        transform.position = cameraPivot.position;
    }
}
