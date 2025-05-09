using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    public Vector3 offset = new Vector3(0f, 20f, 0f);

    void LateUpdate()
    {
        if (MinimapTargetManager.Instance == null || MinimapTargetManager.Instance.currentTarget == null)
            return;

        Transform target = MinimapTargetManager.Instance.currentTarget;
        transform.position = target.position + offset;
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
