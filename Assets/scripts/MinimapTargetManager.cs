using UnityEngine;

public class MinimapTargetManager : MonoBehaviour
{
    public static MinimapTargetManager Instance { get; private set; }

    public Transform currentTarget;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget;
    }
}
