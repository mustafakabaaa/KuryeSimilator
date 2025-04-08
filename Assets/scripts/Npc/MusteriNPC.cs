using System.Collections.Generic;
using UnityEngine;

public class MusteriNPC : MonoBehaviour, Iinterectable
{
    private SCOrderData assignedOrder;
    private string npcName = "Müþteri";

    public void SetOrder(SCOrderData order)
    {
        assignedOrder = order;
        npcName = order.orderName;
    }

    public void Interact()
    {
        if (assignedOrder == null) return;

        bool hasAllItems = true;
        List<string> missingItems = new List<string>();

        foreach (SCItem item in assignedOrder.requiredItems)
        {
            if (!Inventory.Instance.HasItem(item.itemID))
            {
                hasAllItems = false;
                missingItems.Add(item.itemName);
            }
        }

        if (!hasAllItems)
        {
            SayMissingItems(missingItems);
            return;
        }

        DialogueManager.Instance.SetCurrentOrderID(assignedOrder.orderID);
        DialogueUI.Instance.StartDialogue(assignedOrder.dialogueData, npcName);
    }

    public void SayMissingItems(List<string> missingItems)
    {
        string message = "Eksik ürünler: " + string.Join(", ", missingItems);
        Debug.Log(message);

        // MEVCUT DÝYALOG SÝSTEMÝNÝ KULLANARAK MESAJ GÖSTER
        DialogueUI.Instance.ShowSimpleMessage(message);

        // VEYA alternatif olarak direkt diyalog baþlatmak isterseniz:
        // DialogueManager.Instance.SetCurrentOrderID(assignedOrder.orderID);
        // DialogueUI.Instance.StartDialogue(assignedOrder.missingItemsDialogue, npcName);
    }
}