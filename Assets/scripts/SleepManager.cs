using UnityEngine;

public class SleepManager : MonoBehaviour
{
    public LightManager lightManager;
    public OrderManager orderManager;
    public float sleepTime = 22.0f; // Uyku saati

    public float wakeUpTime = 6f; // Sabah uyanma saati
    public float sleepStartTime = 20f; // Uyku baþlangýç saati

    public SleepEffect sleepEffect; // SleepEffect script'i
    //private void Update()
    //{
    //    // Uyku saati kontrolü (akþam 20:00'den sonra)
    //    if (lightManager.GetTimeOfDay() >= sleepStartTime)
    //    {
    //        Debug.Log("Uyku saati! Yataða gitmek için bir yere týklayýn.");
    //        // Uyku tetikleme mekaniði burada olacak
    //    }
    //}

    public void TriggerSleep()
    {
        // Uyku saatinde mi kontrol et
        if (lightManager.GetTimeOfDay() < sleepStartTime&&!orderManager.AreAllOrdersCompleted())
        {
            Debug.Log("Henüz uyku saati deðil! Akþam 20:00'den önce uyuyamazsýnýz.");

            Debug.Log("Tüm görevler tamamlanmadý! Uyuyamazsýnýz.");
            return;
        }


        Debug.Log("Oyuncu uyuyor...");
        EndDay();
        StartNewDay();
    }

    private void EndDay()
    {
        // Tamamlanmamýþ görevleri kontrol et
        foreach (var order in orderManager.GetActiveOrders())
        {
            Debug.Log($"Görev tamamlanmadý: {order.orderName}");
            // Ceza veya ödül uygula
        }

        // Aktif görevleri temizle
        orderManager.GetActiveOrders().Clear();
        // Uyku efektini baþlat
        if (sleepEffect != null)
        {
            sleepEffect.StartSleepEffect();
            Debug.Log("efekt yapildi");
        }
    }

    private void StartNewDay()
    {
        // Yeni gün baþlangýcý
        lightManager.SetTimeOfDay(6f); // Sabah 6'da uyan
        orderManager.StartNewDay(); // Yeni görevler oluþtur
        Debug.Log("Yeni bir gün baþladý! Sabah 6:00.");
    }
}