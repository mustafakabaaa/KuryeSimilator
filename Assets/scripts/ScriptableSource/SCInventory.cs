using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Inventory", menuName = "SC/Scriptable/Inventory")]
public class SCInventory : ScriptableObject
{
    public List<Slot> inventorySlots = new List<Slot>();
    public int maxUnlockedSlots = 8;
    public int maxTotalSlots = 24; // Maksimum slot limiti
    public int stackLimit = 4;

    private void OnEnable()
    {
        InitializeSlots();
    }

    private void InitializeSlots()
    {
        // Baþlangýçta tüm slotlarý oluþtur
        while (inventorySlots.Count < maxTotalSlots)
        {
            inventorySlots.Add(new Slot());
        }
    }

    public bool AddItem(SCItem item)
    {
        // 1. Önce stacklenebilir slotlara bak (sadece açýk slotlarda)
        if (item.canStackable)
        {
            for (int i = 0; i < maxUnlockedSlots; i++)
            {
                if (i >= inventorySlots.Count) break;

                Slot slot = inventorySlots[i];
                if (slot.item == item && slot.itemCount < stackLimit)
                {
                    slot.itemCount++;
                    if (slot.itemCount >= stackLimit)
                    {
                        slot.isFull = true;
                    }
                    return true;
                }
            }
        }

        // 2. Boþ slot ara (sadece açýk slotlarda)
        for (int i = 0; i < maxUnlockedSlots; i++)
        {
            if (i >= inventorySlots.Count) break;

            Slot slot = inventorySlots[i];
            if (slot.item == null || slot.itemCount == 0)
            {
                slot.AddItemToSlot(item);
                return true;
            }
        }

        Debug.LogWarning("Envanter dolu veya yeterli slot açýk deðil!");
        return false;
    }



    public bool IsSlotUnlocked(int slotIndex)
    {
        return slotIndex < maxUnlockedSlots && slotIndex < maxTotalSlots;
    }

    public void UnlockAdditionalSlots(int count)
    {
        maxUnlockedSlots = Mathf.Min(maxUnlockedSlots + count, maxTotalSlots); // sýnýr aþýlmýyor. 

        // Yeni açýlan slotlar için boþ slot oluþtur
        for (int i = inventorySlots.Count; i < maxUnlockedSlots; i++)
        {
            inventorySlots.Add(new Slot());
        }
    }
}
[System.Serializable]

public class Slot
{

    public bool isFull;
    public int itemCount;
    public SCItem item;

    public void AddItemToSlot(SCItem item) 
    {
        this.item = item;   
        if(item.canStackable==false)
        {
            isFull = true;

        }
        itemCount++;
    }
}