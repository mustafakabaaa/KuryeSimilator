using UnityEngine;

public class Bed : MonoBehaviour, Iinterectable
{
    public SleepManager sleepManager;


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
