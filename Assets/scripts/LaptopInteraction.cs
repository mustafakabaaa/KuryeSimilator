// LaptopInteraction.cs (laptop prefab�nda)
using UnityEngine;

public class LaptopInteraction : MonoBehaviour,Iinterectable
{
    public bool CanInteract()
    {
        return true;
    }

    public string GetInteractionText()
    {
        return "ui.interact_pc_open";
    }

    public void Interact()
    {
        // Sahnedeki PCUIController'� bul ve tetikle
        FindObjectOfType<PCUIController>()?.TogglePCUI();
    }
}