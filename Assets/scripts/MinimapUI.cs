using UnityEngine;

public class MinimapUI : MonoBehaviour
{
    [SerializeField] private GameObject minimap;
    
    private void OnEnable()
    {
        UIManager.OnUIStateChanged += UpdateMinimapState;
    }

    private void OnDisable()
    {
        UIManager.OnUIStateChanged -= UpdateMinimapState;
    }

    private void Start()
    {
        UpdateMinimapState(); // Baþlangýçta kontrol et
    }

    private void UpdateMinimapState()
    {
        if (UIManager.Instance == null) return;

        bool anyUIOpen = UIManager.Instance.IsAnyUIOpen();
        minimap.SetActive(!anyUIOpen);
       
    }
}
