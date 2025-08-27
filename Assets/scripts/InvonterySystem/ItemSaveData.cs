using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemSaveData
{
    public List<ItemState> items = new List<ItemState>();
}

[Serializable]
public class ItemState
{
    public string itemID;
    public string itemName;
    public bool isPickedUp;
    public Vector3 position;
    public Quaternion rotation;
}