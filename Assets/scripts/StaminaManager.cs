using UnityEngine;

[System.Serializable]
public class StaminaSettings
{
    public float maxStamina = 100f;
    public float staminaDrainRate = 1f; // Dakikada kaybedeceði stamina
    public float lowStaminaThreshold = 30f; // Düþük stamina eþiði
    public float criticalStaminaThreshold = 10f; // Kritik stamina eþiði
    public float staminaPenaltyMultiplier = 0.5f; // Düþük stamina ceza çarpaný
}

public class StaminaManager : MonoBehaviour
{
    public StaminaSettings settings;
    public float currentStamina;
    private CharrController playerController;
    private bool isPenaltyApplied = false;

    private void Awake()
    {
        playerController = GetComponent<CharrController>();
        currentStamina = settings.maxStamina;
    }

    private void Update()
    {
        // Zamanla stamina azalt
        currentStamina -= settings.staminaDrainRate * Time.deltaTime / 60f; // Saniyede azalma

        // Minimum stamina kontrolü
        currentStamina = Mathf.Clamp(currentStamina, 0f, settings.maxStamina);

        // Stamina durumunu kontrol et
        CheckStaminaStatus();
    }

    private void CheckStaminaStatus()
    {
        // Kritik stamina seviyesi altýnda
        if (currentStamina <= settings.criticalStaminaThreshold && !isPenaltyApplied)
        {
            ApplyStaminaPenalty();
        }
        // Stamina normale döndüðünde
        else if (currentStamina > settings.lowStaminaThreshold && isPenaltyApplied)
        {
            RemoveStaminaPenalty();
        }
    }

    private void ApplyStaminaPenalty()
    {
        if (playerController != null)
        {
            playerController.moveSpeed *= settings.staminaPenaltyMultiplier;
            isPenaltyApplied = true;
            Debug.Log("Düþük stamina! Hareket hýzý yarýya düþtü.");
        }
    }

    private void RemoveStaminaPenalty()
    {
        if (playerController != null)
        {
            playerController.moveSpeed /= settings.staminaPenaltyMultiplier;
            isPenaltyApplied = false;
            Debug.Log("Stamina normale döndü, ceza kalktý.");
        }
    }

    public bool CanSleep()
    {
        // %70'in altýnda ise uyuyabilir
        return currentStamina < settings.maxStamina * 0.7f;
    }

    public void RestoreStamina()
    {
        currentStamina = settings.maxStamina;
        RemoveStaminaPenalty();
    }
}