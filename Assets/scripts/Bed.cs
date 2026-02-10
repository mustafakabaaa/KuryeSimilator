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
        return "ui.interact_sleep";
    }

    public void Interact()
    {
        if (isSleeping) return;

        if (sleepManager != null)
        {
            Debug.Log("Yata�a yat�lmaya �al���l�yor.");
            isSleeping = true;

            bool sleepStarted = sleepManager.RequestSleep(() =>
            {
                isSleeping = false; // Uyku bitti�inde tekrar a�
            });

            if (!sleepStarted)
            {
                isSleeping = false; // Uyku ba�layamad�ysa tekrar a�
            }
        }
    }

}
