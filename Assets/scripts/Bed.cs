using UnityEngine;

public class Bed : MonoBehaviour, Iinterectable
{
    public SleepManager sleepManager;
    private bool isSleeping = false;

    public bool CanInteract()
    {
        return !isSleeping;
    }

    public string GetInteractionText()
    {
        return "UYU (E)";
    }

    public void Interact()
    {
        if (isSleeping) return;

        if (sleepManager != null)
        {
            Debug.Log("Yataða yatýlmaya çalýþýlýyor.");
            isSleeping = true;

            bool sleepStarted = sleepManager.RequestSleep(() =>
            {
                isSleeping = false; // Uyku bittiðinde tekrar aç
            });

            if (!sleepStarted)
            {
                isSleeping = false; // Uyku baþlayamadýysa tekrar aç
            }
        }
    }

}
