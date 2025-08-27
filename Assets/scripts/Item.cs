using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour, Iinterectable, ISaveable
{
    public SCItem item;
    public SCInventory inventory;
    public bool canBePickedUp = true;
    [SerializeField] private string itemID;

    private void Awake()
    {
        // Unique ID oluþtur
        if (string.IsNullOrEmpty(itemID))
        {
            itemID = System.Guid.NewGuid().ToString();
        }
    }

    public void Interact()
    {
        if (!canBePickedUp) return;

        if (inventory.AddItem(item))
        {
            canBePickedUp = false;
            Destroy(gameObject); // ? Sipariþ tabanlý sistem için uygun

        }
    }

    public string GetInteractionText()
    {
        if (!canBePickedUp)
            return "";

        return "AL (E)";
    }

    public bool CanInteract()
    {
        return canBePickedUp;
    }

    // ? DÜZELT: SaveData metodu
    public void SaveData(GameData data)
    {
        if (data.itemData == null)
        {
            data.itemData = new ItemSaveData();
        }

        ItemState itemState = new ItemState
        {
            itemID = itemID,
            itemName = item.itemName,
            isPickedUp = !canBePickedUp,
            position = transform.position,
            rotation = transform.rotation
        };

        data.itemData.items.Add(itemState);
    }

    // ? DÜZELT: LoadData metodu
    public void LoadData(GameData data)
    {
        if (data.itemData == null) return;

        ItemState itemState = data.itemData.items.Find(x => x.itemID == itemID);

        if (itemState != null)
        {
            canBePickedUp = !itemState.isPickedUp;

            if (itemState.isPickedUp)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
                transform.position = itemState.position;
                transform.rotation = itemState.rotation;
            }
        }
    }
}