using UnityEngine;

public class ObjectivePosition : MonoBehaviour
{
    private void Start()
    {
        FindObjectOfType<MarkerHolder>().AddObjectiveMarker(this);
    }

    private void OnDestroy()
    {
        MarkerHolder holder = FindObjectOfType<MarkerHolder>();
        if (holder != null)
            holder.RemoveObjectiveMarker(this);
    }
}
