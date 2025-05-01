using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0f, 20f, 0f);

    void LateUpdate()
    {
        if (player == null) return;
        transform.position = player.position + offset;
        transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Yukarýdan düz bakýþ, sabit yön
    }
}
