using System.Collections;
using UnityEngine;

public class SleepManager : MonoBehaviour, ISaveable
{
    [Header("Referanslar")]
    public LightManager lightManager;
    public OrderManager orderManager;
    public SleepEffect sleepEffect;
    public SleepStaminaSystem sleepStaminaSystem;

    [Header("Uyku Ayarları")]
    public float sleepStartTime = 20f;
    public float sleepDuration = 8f;
    public float minWakeUpTime = 6f;
    public float defaultWakeUpTime = 6f;

    // Debug için ek değişkenler
    [Header("Debug Info")]
    [SerializeField] private float debugCurrentTime;
    [SerializeField] private float debugLastSleepTime;
    [SerializeField] private bool debugIsSleeping;

    private void Awake()
    {
        // SaveManager'a kayıt ol
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterSystem(this);
            Debug.Log("SleepManager SaveManager'a kaydedildi");
        }
        else
        {
            // SaveManager henüz hazır değilse, Start'ta tekrar dene
            StartCoroutine(RegisterLater());
        }
    }

    private IEnumerator RegisterLater()
    {
        yield return new WaitUntil(() => SaveManager.Instance != null);
        SaveManager.Instance.RegisterSystem(this);
        Debug.Log("SleepManager SaveManager'a geç kayıt yapıldı");
    }

    private void Update()
    {
        // Debug bilgilerini güncelle
        if (lightManager != null)
            debugCurrentTime = lightManager.GetTimeOfDay();

        if (sleepStaminaSystem != null)
        {
            debugLastSleepTime = sleepStaminaSystem.lastCheckedTime;
            debugIsSleeping = sleepStaminaSystem.isSleeping;
        }
    }

    private void OnEnable()
    {
        LightManager.OnDayCycleCompleted += HandleDayCycleCompletion;
    }

    private void OnDisable()
    {
        LightManager.OnDayCycleCompleted -= HandleDayCycleCompletion;
    }

    // ISaveable interface implementation
    public void SaveData(GameData data)
    {
        if (data.sleepData == null)
        {
            data.sleepData = new SleepSaveData();
        }

        // Null check'ler ekle
        float currentTime = lightManager != null ? lightManager.GetTimeOfDay() : 0f;
        float lastSleep = sleepStaminaSystem != null ? sleepStaminaSystem.lastCheckedTime : 0f;
        bool isSleeping = sleepStaminaSystem != null ? sleepStaminaSystem.isSleeping : false;

        data.sleepData.currentTimeOfDay = currentTime;
        data.sleepData.lastSleepTime = lastSleep;
        data.sleepData.isSleeping = isSleeping;

        Debug.Log($"[SleepManager] SaveData çağrıldı:");
        Debug.Log($"  - Current Time: {currentTime}");
        Debug.Log($"  - Last Sleep Time: {lastSleep}");
        Debug.Log($"  - Is Sleeping: {isSleeping}");
        Debug.Log($"  - LightManager null? {lightManager == null}");
        Debug.Log($"  - SleepStaminaSystem null? {sleepStaminaSystem == null}");
    }

    public void LoadData(GameData data)
    {
        if (data.sleepData != null)
        {
            Debug.Log($"[SleepManager] LoadData çağrıldı:");
            Debug.Log($"  - Loading Time: {data.sleepData.currentTimeOfDay}");
            Debug.Log($"  - Loading Last Sleep: {data.sleepData.lastSleepTime}");
            Debug.Log($"  - Loading Is Sleeping: {data.sleepData.isSleeping}");

            if (lightManager != null)
            {
                lightManager.SetTimeOfDay(data.sleepData.currentTimeOfDay);
                Debug.Log($"  - LightManager time set to: {data.sleepData.currentTimeOfDay}");
            }
            else
            {
                Debug.LogError("[SleepManager] LightManager null, time set edilemedi!");
            }

            if (sleepStaminaSystem != null)
            {
                sleepStaminaSystem.lastCheckedTime = data.sleepData.lastSleepTime;
                sleepStaminaSystem.isSleeping = data.sleepData.isSleeping;
                Debug.Log($"  - SleepStaminaSystem values updated");
            }
            else
            {
                Debug.LogError("[SleepManager] SleepStaminaSystem null, values set edilemedi!");
            }
        }
        else
        {
            Debug.Log("[SleepManager] LoadData: sleepData null");
        }
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

        Debug.Log("[SleepManager] Uyku başlıyor...");
        if (sleepStaminaSystem != null)
        {
            sleepStaminaSystem.isSleeping = true;
            Debug.Log($"  - isSleeping set to true");
        }

        StartCoroutine(SleepRoutine(onSleepEnd));
    }

    private IEnumerator SleepRoutine(System.Action onSleepEnd)
    {
        sleepStaminaSystem.isSleeping = true;
        sleepEffect?.StartSleepEffect();

        yield return new WaitForSecondsRealtime(3f);

        float oldTime = lightManager.GetTimeOfDay();
        float newWakeTime = (oldTime + sleepDuration) % 24f;
        bool crossedMidnight = (oldTime + sleepDuration) >= 24f;

        Debug.Log($"[SleepManager] Uyku tamamlandı:");
        Debug.Log($"  - Old Time: {oldTime}");
        Debug.Log($"  - New Wake Time: {newWakeTime}");
        Debug.Log($"  - Crossed Midnight: {crossedMidnight}");

        lightManager.SetTimeOfDay(newWakeTime);

        if (sleepStaminaSystem != null)
        {
            sleepStaminaSystem.ResetStamina();
            sleepStaminaSystem.lastCheckedTime = newWakeTime;
            sleepStaminaSystem.isSleeping = false;

            Debug.Log($"  - LastCheckedTime updated to: {newWakeTime}");
            Debug.Log($"  - isSleeping set to false");
        }

        if (crossedMidnight)
        {
            Debug.Log("Uyku sırasında gece yarısı geçildi, yeni güne geçiliyor...");
            AdvanceToNextDay();
        }

        // Save işleminden önce değerleri kontrol et
        Debug.Log($"[SleepManager] Save öncesi son değerler:");
        Debug.Log($"  - Current Time: {lightManager.GetTimeOfDay()}");
        Debug.Log($"  - Last Sleep Time: {sleepStaminaSystem?.lastCheckedTime}");
        Debug.Log($"  - Is Sleeping: {sleepStaminaSystem?.isSleeping}");

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
            Debug.Log("[SleepManager] Game saved after sleep");
        }

        onSleepEnd?.Invoke();
    }

    private bool CanSleep()
    {
        float currentTime = lightManager.GetTimeOfDay();
        bool isNightTime = currentTime >= sleepStartTime || currentTime < minWakeUpTime;
        bool ordersComplete = orderManager.AreAllOrdersCompleted();
        bool needsSleep = sleepStaminaSystem.CanSleep;

        return isNightTime || ordersComplete || needsSleep;
    }

    private void AdvanceToNextDay()
    {
        orderManager.OnDayCompleted();
        Debug.Log("Yeni güne geçildi (uyku ile)!");
    }

    private void EndDay()
    {
        foreach (var npc in FindObjectsOfType<MusteriNPC>())
        {
            Destroy(npc.gameObject);
        }

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

        if (sleepStaminaSystem != null)
        {
            sleepStaminaSystem.isSleeping = false;
        }

        Debug.Log("Yeni gün başladı!");
    }

    private void HandleDayCycleCompletion()
    {
        Debug.Log("Gece 12 oldu, otomatik gün sonu işlemleri yapılıyor!");
        EndDay();
        StartNewDay(lightManager.GetTimeOfDay());
    }

    // Debug için test metodu
    [ContextMenu("Test Save Values")]
    public void TestSaveValues()
    {
        Debug.Log("=== CURRENT VALUES TEST ===");
        Debug.Log($"LightManager Time: {lightManager?.GetTimeOfDay()}");
        Debug.Log($"SleepStaminaSystem LastChecked: {sleepStaminaSystem?.lastCheckedTime}");
        Debug.Log($"SleepStaminaSystem IsSleeping: {sleepStaminaSystem?.isSleeping}");
        Debug.Log($"SleepStaminaSystem CanSleep: {sleepStaminaSystem?.CanSleep}");
    }
}