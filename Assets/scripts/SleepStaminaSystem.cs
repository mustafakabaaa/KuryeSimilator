using System;
using UnityEngine;
using UnityEngine.UI;

public class SleepStaminaSystem : MonoBehaviour
{
     public float currentStamina = 100f;
    public float staminaDecreaseRate = 2f;
    public float penaltyThreshold = 30f;
    public float sleepThreshold = 70f;

    [Header("Movement Stamina Cost")]
    public float walkingStaminaCost = 1f;

    public bool IsPenaltyActive => currentStamina <= penaltyThreshold;
    public bool CanSleep => currentStamina <= sleepThreshold;

    private LightManager lightManager;
    public float lastCheckedTime;

    public Slider staminabar;
    public Image fillImage;
    public Color highStaminaColor = Color.green;
    public Color lowStaminaColor = Color.red;

    [HideInInspector] public bool isSleeping = false;

    [Header("Dependencies")]
    public GameDataSO gameData;
    public CharacterStat staminaStat;


    public event Action OnStaminaUpgraded;
    private void Start()
    {
        lightManager = FindObjectOfType<LightManager>();
        lastCheckedTime = lightManager.GetTimeOfDay();

        if (fillImage == null && staminabar != null)
        {
            fillImage = staminabar.fillRect.GetComponent<Image>();
        }

        if (gameData != null && staminaStat != null)
        {
            float maxStamina = gameData.GetCurrentStatValue(staminaStat);
            currentStamina = maxStamina;

            if (staminabar != null)
            {
                staminabar.maxValue = maxStamina;
            }
        }
        if (gameData != null)
        {
            gameData.OnStatUpgraded += HandleStatUpgrade;
        }

        UpdateStaminaBar();
    }
    private void OnDestroy()
    {
        if (gameData != null)
        {
            gameData.OnStatUpgraded -= HandleStatUpgrade;
        }
    }

    private void HandleStatUpgrade(CharacterStat upgradedStat)
    {
        if (upgradedStat == staminaStat)
        {
            Debug.Log("[SleepStaminaSystem] Stat upgrade detected, resetting stamina.");
            ResetStamina();
        }
    }


    public void HandleMovementStamina(bool isMoving, bool isRunning)
    {
        if (!isMoving) return;

        float cost = isRunning ? walkingStaminaCost : walkingStaminaCost;
        currentStamina = Mathf.Max(0, currentStamina - cost * Time.deltaTime);
        UpdateStaminaBar();
    }

    private void FixedUpdate()
    {
        if (isSleeping) return;

        float currentTime = lightManager.GetTimeOfDay();

        if (currentTime != lastCheckedTime)
        {
            float timePassed;

            if (currentTime >= lastCheckedTime)
            {
                timePassed = currentTime - lastCheckedTime;
            }
            else
            {
                timePassed = (24f - lastCheckedTime) + currentTime;
            }

            currentStamina = Mathf.Max(0f, currentStamina - staminaDecreaseRate * timePassed);
            lastCheckedTime = currentTime;
            UpdateStaminaBar();
        }
    }

    private void UpdateStaminaBar()
{
    if (staminabar != null)
    {
        staminabar.value = currentStamina;
    }

    if (fillImage != null)
    {
        fillImage.color = (currentStamina <= penaltyThreshold) ? lowStaminaColor : highStaminaColor;
    }
}


    public void ResetStamina()
    {
        float maxStamina;

        if (gameData != null && staminaStat != null)
        {
            maxStamina = gameData.GetCurrentStatValue(staminaStat);
            Debug.Log($"[ResetStamina] GameData'dan alınan MaxStamina: {maxStamina}");
        }
        else
        {
            maxStamina = 100f;
            Debug.LogWarning("[ResetStamina] GameData veya StaminaStat atanmamış, varsayılan 100 kullanılıyor.");
        }

        currentStamina = maxStamina;
        Debug.Log($"[ResetStamina] CurrentStamina set: {currentStamina}");

        if (staminabar != null)
        {
            staminabar.maxValue = maxStamina;
            staminabar.value = maxStamina;
        }

        UpdateStaminaBar();
    }


}
