using UnityEngine;

public class SleepStaminaSystem : MonoBehaviour
{
    [Range(0, 100)] public float currentStamina = 100f;
    public float staminaDecreaseRate = 2f; // Saat baþýna azalýþ
    public float penaltyThreshold = 30f; // Ceza seviyesi
    public float sleepThreshold = 70f; // Uyumaya izin verilen sýnýr
    [Header("Movement Stamina Cost")]
    public float walkingStaminaCost = 1f; // Saniyede kaybedeceði stamina miktarý
    public bool IsPenaltyActive => currentStamina <= penaltyThreshold;
    public bool CanSleep => currentStamina <= sleepThreshold;

    private LightManager lightManager;
    private float lastCheckedTime;

    private void Start()
    {
        lightManager = FindObjectOfType<LightManager>();
        lastCheckedTime = lightManager.GetTimeOfDay();
    }
    public void HandleMovementStamina(bool isMoving, bool isRunning)
    {
        if (!isMoving) return;

        float cost = isRunning ? walkingStaminaCost : walkingStaminaCost;
        currentStamina = Mathf.Max(0, currentStamina - cost * Time.deltaTime);
    }
    private void Update()
    {
        float currentTime = lightManager.GetTimeOfDay();

        if (currentTime != lastCheckedTime)
        {
            float timePassed = Mathf.Abs(currentTime - lastCheckedTime);
            currentStamina = Mathf.Max(0f, currentStamina - staminaDecreaseRate * timePassed);
            lastCheckedTime = currentTime;
        }
    }

    public void ResetStamina()
    {
        currentStamina = 100f;
    }
}
