using System.Collections;
using UnityEngine;

public class SleepManager : MonoBehaviour
{
    [Header("Referanslar")]
    public LightManager lightManager;
    public OrderManager orderManager;
    public SleepEffect sleepEffect;
    public SleepStaminaSystem sleepStaminaSystem;

    [Header("Uyku Ayarları")]
    public float sleepStartTime = 20f; // Uykuya başlanabilecek saat (örn: 20.00)
    public float sleepDuration = 8f;   // Uyku süresi
    public float minWakeUpTime = 6f;   // Sabah uyanabileceği minimum saat (örn: 06.00)
    public float defaultWakeUpTime = 6f; // Gün döngüsünde kullanılacak varsayılan sabah saati

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

        sleepStaminaSystem.isSleeping = true; // Uyku başlıyor
        StartCoroutine(SleepRoutine());
    }

    private IEnumerator SleepRoutine()
    {
        sleepStaminaSystem.isSleeping = true;
        sleepEffect?.StartSleepEffect();
        yield return new WaitForSecondsRealtime(3f);

        // 1. Zamanı ileri sar
        float newWakeTime = (lightManager.GetTimeOfDay() + sleepDuration) % 24f;
        lightManager.SetTimeOfDay(newWakeTime);

        // 2. Şimdi stamina resetleniyor
        sleepStaminaSystem.ResetStamina();

        // 🛠️ Burada zamanla uyumlu hale getiriyoruz
        sleepStaminaSystem.lastCheckedTime = newWakeTime;

        // 3. Uyku bitti
        sleepStaminaSystem.isSleeping = false;
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

        // Sipariş sistemini sıfırla
        orderManager.CleanupDay();

        if (lightManager.GetTimeOfDay() >= 23.9f || lightManager.GetTimeOfDay() < 0.1f)
        {
            Debug.Log("Gece yarısı oldu, gün sonu işlemleri yapılıyor");
        }
    }

    public void StartNewDay(float newStartTime)
    {
        lightManager.SetTimeOfDay(newStartTime);
        orderManager.StartNewDay();
        
        sleepStaminaSystem.isSleeping = false; // Uyku bitti
        Debug.Log("Yeni gün başladı!");
    }


    private void HandleDayCycleCompletion()
    {
        Debug.Log("Gece 12 oldu, otomatik gün sonu işlemleri yapılıyor!");
        EndDay();
        StartNewDay(lightManager.GetTimeOfDay()); 
    }

}
