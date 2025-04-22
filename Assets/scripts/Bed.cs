using UnityEngine;

public class Bed : MonoBehaviour, Iinterectable
{
    public SleepManager sleepManager;

    public bool CanInteract()
    {
        return true;
    }

    public string GetInteractionText()
    {
        return "UYU (E)";
    }

    public void Interact()
    {
       



        // Uyku tetikle
        if (sleepManager != null)
        {
            Debug.Log("Yataða yatýldý.");
            sleepManager.RequestSleep();
        }
    }
}
