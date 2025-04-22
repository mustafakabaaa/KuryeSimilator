using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour, Iinterectable
{
    public SCItem item;
    public SCInventory inventory;
    public bool canBePickedUp = true; // Belki bazý durumlarda alýnamaz

    public void Interact()
    {
        if (!canBePickedUp) return;

        if (inventory.AddItem(item)) // this.gameObject.GetComponent<Item>() yazmana gerek yok
        {
            Destroy(this.gameObject);
        }
    }

    public string GetInteractionText()
    {
        if (!canBePickedUp)
            return ""; // veya "ALINAMAZ"

        return "AL (E)";
    }

    public bool CanInteract()
    {
        return canBePickedUp; // Duruma göre deðiþebilir
    }
}
