using UnityEngine;

public class Bed : MonoBehaviour, Iinterectable
{
    public SleepManager sleepManager;
    

    public void Interact()
    {
        Debug.Log("Yataða yatýldý.");

        

        // Uyku tetikle
        if (sleepManager != null)
        {
            sleepManager.TriggerSleep();
        }
    }
}