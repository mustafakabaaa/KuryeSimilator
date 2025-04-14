using System.Collections;
using UnityEngine;

public class SleepManager : MonoBehaviour
{
    [Header("Referanslar")]
    public LightManager lightManager;
    public OrderManager orderManager;
    public SleepEffect sleepEffect;
    public SleepStaminaSystem sleepStaminaSystem;

    [Header("Uyku Ayarlarý")]
    public float sleepStartTime = 20f; // Uykuya baþlanabilecek saat (örn: 20.00)
    public float sleepDuration = 8f;   // Uyku süresi
    public float minWakeUpTime = 6f;   // Sabah uyanabileceði minimum saat (örn: 06.00)
    public float defaultWakeUpTime = 6f; // Gün döngüsünde kullanýlacak varsayýlan sabah saati

    private void OnEnable()
    {
        LightManager.OnDayCycleCompleted += HandleDayCycleCompletion;
    }

    private void OnDisable()
    {
        LightManager.OnDayCycleCompleted -= HandleDayCycleCompletion;
    }

    public void RequestSleep()
    {
        TriggerSleep();
    }

    private void TriggerSleep()
    {
        if (!CanSleep()) return;

        StartCoroutine(SleepRoutine());
    }

    private IEnumerator SleepRoutine()
    {
        float currentTime = lightManager.GetTimeOfDay();

        // Uyku efektini baþlat
        sleepEffect?.StartSleepEffect();

        // Bir süre bekle (örneðin animasyon için)
        yield return new WaitForSecondsRealtime(3f);

        // Uyanma saatini hesapla
        float newWakeTime = (currentTime + sleepDuration) % 24f;

        // Saat güncellensin
        lightManager.SetTimeOfDay(newWakeTime);

        // Gün deðiþimi gerekiyorsa iþlemleri yap
        if (currentTime + sleepDuration >= 24f)
        {
            EndDay();
            StartNewDay(newWakeTime);
        }

        Debug.Log($"Uyandýnýz! Saat: {newWakeTime:00.00}");
    }

    private bool CanSleep()
    {
        float currentTime = lightManager.GetTimeOfDay();

        bool isNightTime = currentTime >= sleepStartTime || currentTime < minWakeUpTime;
        bool ordersComplete = orderManager.AreAllOrdersCompleted();
        bool needsSleep = sleepStaminaSystem.CanSleep;

        return isNightTime || ordersComplete || needsSleep;
    }

    private void EndDay()
    {
        // Tüm aktif NPC'leri temizle
        foreach (var npc in FindObjectsOfType<MusteriNPC>())
        {
            Destroy(npc.gameObject);
        }

        // Sipariþ sistemini sýfýrla
        orderManager.CleanupDay();

        if (lightManager.GetTimeOfDay() >= 23.9f || lightManager.GetTimeOfDay() < 0.1f)
        {
            Debug.Log("Gece yarýsý oldu, gün sonu iþlemleri yapýlýyor");
        }
    }

    public void StartNewDay(float newStartTime)
    {
        // Yeni gün baþlatýlýrken saat ayarlanýr
        lightManager.SetTimeOfDay(newStartTime);
        orderManager.StartNewDay();
        sleepStaminaSystem.ResetStamina();

        Debug.Log("Yeni gün baþladý!");
    }

    private void HandleDayCycleCompletion()
    {
        Debug.Log("Gece 12 oldu, otomatik gün sonu iþlemleri yapýlýyor!");
        EndDay();
        StartNewDay(defaultWakeUpTime); // Gün döngüsü sonlandýðýnda varsayýlan uyanýþ saati
    }
}
