using TMPro;
using UnityEngine;

public class MotorHUD : MonoBehaviour
{
    [SerializeField] private MotorcycleVehicle motor;
    [SerializeField] private TextMeshProUGUI hizGostergesi;
    [SerializeField] private GameObject hudPanel;

    [Header("Görüntü Ayarlarý")]
    [SerializeField] private bool kmhGoster = true;
    [SerializeField] private string hizFormat = "0";

    private void Update()
    {
        // Motor referansý kontrolü
        if (motor == null)
        {
            Debug.LogWarning("Motor referansý atanmamýþ!");
            hudPanel.SetActive(false);
            return;
        }

        // Oyuncu motorda mý kontrolü
        if (!motor.IsPlayerOnBoard)
        {
            hudPanel.SetActive(false);
            return;
        }

        // HUD'u göster
        hudPanel.SetActive(true);

        // Hýz bilgisini güncelle
        float hiz = motor.CurrentSpeed;
        hizGostergesi.text = kmhGoster ?
            $"{hiz.ToString(hizFormat)} km/s" :
            $"{(hiz * 0.621371f).ToString(hizFormat)} mph";
    }
}