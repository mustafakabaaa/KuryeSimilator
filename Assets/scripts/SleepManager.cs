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

    public bool RequestSleep(System.Action onSleepEnd = null)
{
    if (!CanSleep())
    {
            ToastManager.Instance.ShowToast("Uyuyamazsın: Saat uygun değil, siparişler tamam değil veya uykun yok.", 3f);
        return false;
    }

    TriggerSleep(onSleepEnd);
    return true;
}


    private void TriggerSleep(System.Action onSleepEnd)
    {
        if (!CanSleep()) return;

        sleepStaminaSystem.isSleeping = true;
        StartCoroutine(SleepRoutine(onSleepEnd));
    }

    private IEnumerator SleepRoutine(System.Action onSleepEnd)
    {
        sleepStaminaSystem.isSleeping = true;
        sleepEffect?.StartSleepEffect();
        yield return new WaitForSecondsRealtime(3f);

        float newWakeTime = (lightManager.GetTimeOfDay() + sleepDuration) % 24f;
        lightManager.SetTimeOfDay(newWakeTime);

        sleepStaminaSystem.ResetStamina();
        sleepStaminaSystem.lastCheckedTime = newWakeTime;
        sleepStaminaSystem.isSleeping = false;

        onSleepEnd?.Invoke(); // 🟢 Yatak tekrar etkileşime açılıyor
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
