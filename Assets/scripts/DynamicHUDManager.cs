using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicHUDManager : MonoBehaviour
{

    [SerializeField] private GameObject minimap;
    [SerializeField] private GameObject staminaBar;
    [SerializeField] private GameObject walletAndClock;

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
        staminaBar.SetActive(!anyUIOpen);
        walletAndClock.SetActive(!anyUIOpen);

    }
}
