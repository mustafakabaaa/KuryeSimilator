using System;
using System.Collections.Generic;

[Serializable]
public class InventorySaveData
{
    public List<SlotSaveData> playerInventorySlots = new List<SlotSaveData>();
    public List<SlotSaveData> bagInventorySlots = new List<SlotSaveData>();
}

[Serializable]
public class SlotSaveData
{
    public string itemName;
    public int itemCount;
    public bool isFull;
}