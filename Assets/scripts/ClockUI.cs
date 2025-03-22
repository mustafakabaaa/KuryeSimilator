using UnityEngine;
using TMPro;

public class ClockUI : MonoBehaviour
{
    public TextMeshProUGUI clockText; // Saati ve dakikayý gösterecek TextMeshPro
    public LightManager lightManager; // TimeOfDay deðerini almak için

    private int lastHour = -1; // Son güncellenen saat deðeri
    private int lastMinute = -1; // Son güncellenen dakika deðeri

    private void Update()
    {
        // TimeOfDay deðerinden saat ve dakika kýsýmlarýný al
        float timeOfDay = lightManager.GetTimeOfDay();
        int currentHour = Mathf.FloorToInt(timeOfDay); // Saat kýsmý
        int currentMinute = Mathf.FloorToInt((timeOfDay - currentHour) * 60f); // Dakika kýsmý

        // Eðer saat veya dakika deðeri deðiþtiyse
        if (currentHour != lastHour || currentMinute != lastMinute)
        {
            lastHour = currentHour; // Son güncellenen saat deðerini güncelle
            lastMinute = currentMinute; // Son güncellenen dakika deðerini güncelle
            UpdateClockText(currentHour, currentMinute); // Text'i güncelle
        }
    }

    private void UpdateClockText(int hour, int minute)
    {
        // Saati ve dakikayý TextMeshPro'ya yaz
        clockText.text = hour.ToString("00") + ":" + minute.ToString("00"); // Örneðin, "09:30" veya "12:45"
    }
}