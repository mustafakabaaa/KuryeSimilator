using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable/Item")]
public class SCItem : ScriptableObject
{
    public string itemName;
    public string itemDescription;
    public bool canStackable; // Stacklenebilir mi?
    public int maxStackSize = 4; // Maksimum stack boyutu (varsayýlan deðer 4)
    public Sprite itemIcon;
    public GameObject itemPrefab;
}