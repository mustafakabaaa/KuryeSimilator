// LaptopInteraction.cs (laptop prefabýnda)
using UnityEngine;

public class LaptopInteraction : MonoBehaviour,Iinterectable
{
    public bool CanInteract()
    {
        return true;
    }

    public string GetInteractionText()
    {
        return "PC Ac (E)";
    }

    public void Interact()
    {
        // Sahnedeki PCUIController'ý bul ve tetikle
        FindObjectOfType<PCUIController>()?.TogglePCUI();
    }
}