using TMPro;
using UnityEngine;

public class MotorHUD : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI hizGostergesi;
    [SerializeField] private TextMeshProUGUI kilometreGostergesi;
    [SerializeField] private GameObject hudPanel;

    [Header("Display Settings")]
    [SerializeField] private bool kmhGoster = true;
    [SerializeField] private string hizFormat = "0";

    private MotorcycleVehicle currentMotor;

    private void OnEnable()
    {
        // Null kontrol� ekledik
        if (MotorEventManager.Instance == null)
        {
            Debug.LogError("MotorEventManager bulunamad�! Sahneye ekleyin.");
            return;
        }

        MotorEventManager.Instance.OnMotorcycleMounted += HandleMotorcycleMounted;
        MotorEventManager.Instance.OnMotorcycleDismounted += HandleMotorcycleDismounted;
    }

    private void OnDisable()
    {
        // Null kontrol� ekledik
        if (MotorEventManager.Instance == null) return;

        MotorEventManager.Instance.OnMotorcycleMounted -= HandleMotorcycleMounted;
        MotorEventManager.Instance.OnMotorcycleDismounted -= HandleMotorcycleDismounted;
    }

    private void HandleMotorcycleMounted(MotorcycleVehicle motor)
    {
        currentMotor = motor;
        if (hudPanel != null) hudPanel.SetActive(true);
    }

  
    public void ForceCloseHUD()
    {
        Debug.Log("ForceCloseHUD called");
        currentMotor = null;
        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
            Debug.Log("HUD forcefully closed");
        }
        else
        {
            Debug.LogError("HUD Panel reference is null!");
        }
    }

    private void HandleMotorcycleDismounted()
    {
        Debug.Log("HandleMotorcycleDismounted called");
        ForceCloseHUD();
    }
    private void Update()
    {
        if (currentMotor == null || !currentMotor.IsPlayerOnBoard) return;

        float hiz = currentMotor.CurrentSpeed;
        hizGostergesi.text = kmhGoster ?
            $"{hiz.ToString(hizFormat)} km/s" :
            $"{(hiz * 0.621371f).ToString(hizFormat)} mph";

        if (kilometreGostergesi != null)
        {
            kilometreGostergesi.text = $"{currentMotor.TotalKilometre:F1} km";
        }
    }
}