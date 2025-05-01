using UnityEngine;

public class TargetManager : MonoBehaviour
{
    public MinimapTargetIndicator indicator;
    public GameObject gorevNoktasi;

    void Start()
    {
        indicator.target = gorevNoktasi.transform;
    }
}
