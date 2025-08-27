using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour, ISaveable
{
    public static Inventory Instance; // Singleton örneði

    public SCInventory playerInventory; // Oyuncunun envanteri
    public SCBagInventory bagInventory; // Çantanýn envanteri
    private InventoryUIController inventoryUIController;
    bool isSwapping;
    int tempIndex;
    Slot tempSlot;
    
    public Transform dropPoint; // Inspector'dan atayacaðýz

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        inventoryUIController = GetComponent<InventoryUIController>();
    }
    // Stacklenebilir slot bul
    private int FindStackableSlot(SCInventory inventory, SCItem item)
    {
        for (int i = 0; i < inventory.inventorySlots.Count; i++)
        {
            Slot slot = inventory.inventorySlots[i];
            if (slot.item != null && slot.item.itemName == item.itemName && slot.itemCount < item.maxStackSize)
            {
                return i; // Stacklenebilir slot bulundu
            }
        }
        return -1; // Stacklenebilir slot yok
    }
    public bool HasItem(string itemID)
    {
        // Oyuncunun envanterini kontrol et
        foreach (Slot slot in playerInventory.inventorySlots)
        {
            if (slot.item != null && slot.item.itemID == itemID && slot.itemCount > 0)
            {
                return true; // Item bulundu
            }
        }

        // Çantanýn envanterini kontrol et (eðer çanta açýksa)
        if (inventoryUIController.IsShowingBag())
        {
            foreach (Slot slot in bagInventory.inventorySlots)
            {
                if (slot.item != null && slot.item.itemID == itemID && slot.itemCount > 0)
                {
                    return true; // Item bulundu
                }
            }
        }

        return false; // Item bulunamadý
    }
    public void RemoveItem(string itemID, int amount)
    {
        int remainingToRemove = amount;

        // Önce oyuncu envanterinden sil
        foreach (Slot slot in playerInventory.inventorySlots)
        {
            if (slot.item != null && slot.item.itemID == itemID && slot.itemCount > 0)
            {
                int removeCount = Mathf.Min(slot.itemCount, remainingToRemove);
                slot.itemCount -= removeCount;
                remainingToRemove -= removeCount;

                if (slot.itemCount <= 0)
                {
                    slot.item = null;
                    slot.isFull = false;
                }

                if (remainingToRemove <= 0)
                    break;
            }
        }

        // Eðer hala silinecek varsa ve çanta açýksa, çantadan da sil
        if (remainingToRemove > 0 && inventoryUIController.IsShowingBag())
        {
            foreach (Slot slot in bagInventory.inventorySlots)
            {
                if (slot.item != null && slot.item.itemID == itemID && slot.itemCount > 0)
                {
                    int removeCount = Mathf.Min(slot.itemCount, remainingToRemove);
                    slot.itemCount -= removeCount;
                    remainingToRemove -= removeCount;

                    if (slot.itemCount <= 0)
                    {
                        slot.item = null;
                        slot.isFull = false;
                    }

                    if (remainingToRemove <= 0)
                        break;
                }
            }
        }

        if (remainingToRemove > 0)
        {
            Debug.LogWarning($"Yeterli miktarda {itemID} bulunamadý, {remainingToRemove} eksik kaldý.");
        }

        inventoryUIController.UpdateUI(playerInventory);
        inventoryUIController.UpdateUI(bagInventory);
    }

    // Stack'e ekle
    private bool AddToStack(SCInventory inventory, SCItem item, int amount)
    {
        int stackableSlotIndex = FindStackableSlot(inventory, item);
        if (stackableSlotIndex != -1)
        {
            Slot slot = inventory.inventorySlots[stackableSlotIndex];
            int spaceAvailable = item.maxStackSize - slot.itemCount;
            int amountToAdd = Mathf.Min(amount, spaceAvailable);

            slot.itemCount += amountToAdd;
            inventoryUIController.UpdateUI(playerInventory);

            inventoryUIController.UpdateUI(bagInventory);
            return true; // Stack'e eklendi
        }
        return false; // Stack'e eklenemedi
    }
    // Swap iþlemi
    public void SwapItem(int index)
    {
        if (inventoryUIController.IsShowingBag())
        {
            if (isSwapping == false)
            {
                tempIndex = index;
                tempSlot = bagInventory.inventorySlots[tempIndex];
                isSwapping = true;
            }
            else if (isSwapping == true)
            {
                bagInventory.inventorySlots[tempIndex] = bagInventory.inventorySlots[index];
                bagInventory.inventorySlots[index] = tempSlot;
                isSwapping = false;

                // EKLENEN SATIR: Seçimi sýfýrla
                EventSystem.current.SetSelectedGameObject(null);
            }
            inventoryUIController.UpdateUI(bagInventory);
        }
        else
        {
            if (isSwapping == false)
            {
                tempIndex = index;
                tempSlot = playerInventory.inventorySlots[tempIndex];
                isSwapping = true;
            }
            else if (isSwapping == true)
            {
                playerInventory.inventorySlots[tempIndex] = playerInventory.inventorySlots[index];
                playerInventory.inventorySlots[index] = tempSlot;
                isSwapping = false;

                // EKLENEN SATIR: Seçimi sýfýrla
                EventSystem.current.SetSelectedGameObject(null);
            }
            inventoryUIController.UpdateUI(playerInventory);
        }
    }

    public void ClearSlot(int slotIndex)
    {
        if (inventoryUIController.IsShowingBag())
        {
            if (slotIndex >= 0 && slotIndex < bagInventory.inventorySlots.Count)
            {
                DropItem(slotIndex, true, true); // Çantadan tüm stack'i býrak
                bagInventory.inventorySlots[slotIndex].item = null;
                bagInventory.inventorySlots[slotIndex].itemCount = 0;
                bagInventory.inventorySlots[slotIndex].isFull = false;
                inventoryUIController.UpdateUI(bagInventory);
            }
        }
        else
        {
            if (slotIndex >= 0 && slotIndex < playerInventory.inventorySlots.Count)
            {
                DropItem(slotIndex, false, true); // Oyuncudan tüm stack'i býrak
                playerInventory.inventorySlots[slotIndex].item = null;
                playerInventory.inventorySlots[slotIndex].itemCount = 0;
                playerInventory.inventorySlots[slotIndex].isFull = false;
                inventoryUIController.UpdateUI(playerInventory);
            }
        }
    }
    public void DropItem(int slotIndex, bool fromBag, bool dropAllStack = false)
    {
        Slot slot;

        if (fromBag)
        {
            if (slotIndex < 0 || slotIndex >= bagInventory.inventorySlots.Count) return;
            slot = bagInventory.inventorySlots[slotIndex];
        }
        else
        {
            if (slotIndex < 0 || slotIndex >= playerInventory.inventorySlots.Count) return;
            slot = playerInventory.inventorySlots[slotIndex];
        }

        // Slot boþsa iþlem yapma
        if (slot.item == null || slot.itemCount <= 0) return;

        // Kaç adet býrakýlacak? (Tüm stack veya 1 adet)
        int dropCount = dropAllStack ? slot.itemCount : 1;

        // Nesneyi sahneye spawn et
        if (slot.item.itemPrefab != null && dropPoint != null)
        {
            for (int i = 0; i < dropCount; i++)
            {
                GameObject droppedItem = Instantiate(
                    slot.item.itemPrefab,
                    dropPoint.position + Random.insideUnitSphere * 0.3f, // Küçük bir rastgele offset
                    dropPoint.rotation
                );

                // Item component'i yoksa ekle
                if (!droppedItem.GetComponent<Item>())
                {
                    Item itemComponent = droppedItem.AddComponent<Item>();
                    itemComponent.item = slot.item;
                    itemComponent.inventory = playerInventory;
                }

                // Fizik ekle (nesnenin düþmesi için)
                Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(dropPoint.forward * 2f + Vector3.up * 1f, ForceMode.Impulse);
                }
            }

            // Envanterden düþürülen miktarý azalt
            slot.itemCount -= dropCount;
            if (slot.itemCount <= 0)
            {
                slot.item = null;
                slot.isFull = false;
            }

            // UI'ý güncelle
            inventoryUIController.UpdateUI(fromBag ? bagInventory : playerInventory);
        }
        else
        {
            Debug.LogError("Item prefab or drop point is missing!");
        }
    }

    public void ResetSwap()
    {
        isSwapping = false;
        tempIndex = -1;
        tempSlot = null;
        EventSystem.current.SetSelectedGameObject(null);

        Debug.Log("Swap iþlemi sýfýrlandý.");
    }

    // Eþyayý oyuncudan çantaya taþý
    public void MoveItemFromPlayerToBag(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= playerInventory.inventorySlots.Count)
            return;

        Slot playerSlot = playerInventory.inventorySlots[slotIndex];

        // Oyuncunun slotunda eþya yoksa iþlem yapma
        if (playerSlot.item == null || playerSlot.itemCount <= 0)
            return;

        SCItem item = playerSlot.item;
        int amount = playerSlot.itemCount;

        // Stacklenebilir mi kontrol et
        if (item.canStackable)
        {
            // Çantada stacklenebilir slot bul ve ekle
            bool addedToStack = AddToStack(bagInventory, item, amount);

            if (addedToStack)
            {
                // Stack'e eklendi, oyuncunun slotunu boþalt
                playerInventory.inventorySlots[slotIndex] = new Slot();
            }
            else
            {
                // Stack'e eklenemedi, yeni bir slota ekle
                int emptySlotIndex = FindEmptySlotInBag();
                if (emptySlotIndex != -1)
                {
                    bagInventory.inventorySlots[emptySlotIndex] = playerSlot;
                    playerInventory.inventorySlots[slotIndex] = new Slot();
                }
                else
                {
                    Debug.Log("Çantada boþ slot yok!");
                    return;
                }
            }
        }
        else
        {
            // Stacklenebilir deðilse, doðrudan boþ bir slota ekle
            int emptySlotIndex = FindEmptySlotInBag();
            if (emptySlotIndex != -1)
            {
                bagInventory.inventorySlots[emptySlotIndex] = playerSlot;
                playerInventory.inventorySlots[slotIndex] = new Slot();
            }
            else
            {
                Debug.Log("Çantada boþ slot yok!");
                return;
            }
        }

        // UI'ý güncelle
        inventoryUIController.UpdateUI(playerInventory);
        inventoryUIController.UpdateUI(bagInventory);
    }

    // Eþyayý çantadan oyuncuya taþý
    public void MoveItemFromBagToPlayer(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= bagInventory.inventorySlots.Count)
            return;

        Slot bagSlot = bagInventory.inventorySlots[slotIndex];

        // Çantanýn slotunda eþya yoksa iþlem yapma
        if (bagSlot.item == null || bagSlot.itemCount <= 0)
            return;

        SCItem item = bagSlot.item;
        int amount = bagSlot.itemCount;

        // Stacklenebilir mi kontrol et
        if (item.canStackable)
        {
            // Oyuncunun envanterinde stacklenebilir slot bul ve ekle
            bool addedToStack = AddToStack(playerInventory, item, amount);

            if (addedToStack)
            {
                // Stack'e eklendi, çantanýn slotunu boþalt
                bagInventory.inventorySlots[slotIndex] = new Slot();
            }
            else
            {
                // Stack'e eklenemedi, yeni bir slota ekle
                int emptySlotIndex = FindEmptySlotInPlayer();
                if (emptySlotIndex != -1)
                {
                    playerInventory.inventorySlots[emptySlotIndex] = bagSlot;
                    bagInventory.inventorySlots[slotIndex] = new Slot();
                }
                else
                {
                    Debug.Log("Oyuncunun envanterinde boþ slot yok!");
                    return;
                }
            }
        }
        else
        {
            // Stacklenebilir deðilse, doðrudan boþ bir slota ekle
            int emptySlotIndex = FindEmptySlotInPlayer();
            if (emptySlotIndex != -1)
            {
                playerInventory.inventorySlots[emptySlotIndex] = bagSlot;
                bagInventory.inventorySlots[slotIndex] = new Slot();
            }
            else
            {
                Debug.Log("Oyuncunun envanterinde boþ slot yok!");
                return;
            }
        }

        // UI'ý güncelle
        inventoryUIController.UpdateUI(playerInventory);
        inventoryUIController.UpdateUI(bagInventory);
    }
    // Çantada boþ slot bul
    private int FindEmptySlotInBag()
    {
        for (int i = 0; i < bagInventory.inventorySlots.Count; i++)
        {
            if (bagInventory.inventorySlots[i].item == null || bagInventory.inventorySlots[i].itemCount == 0)
            {
                return i;
            }
        }
        return -1; // Boþ slot yok
    }
    public int GetItemCount(string itemID)
    {
        int count = 0;
        foreach (Slot slot in playerInventory.inventorySlots)
        {
            if (slot.item != null && slot.item.itemID == itemID)
            {
                count += slot.itemCount;
            }
        }
        return count;
    }
    // Oyuncunun envanterinde boþ slot bul
    private int FindEmptySlotInPlayer()
    {
        // Sadece açýk olan slotlarda ara
        for (int i = 0; i < playerInventory.maxUnlockedSlots; i++)
        {
            if (i >= playerInventory.inventorySlots.Count) break;

            if (playerInventory.inventorySlots[i].item == null ||
                playerInventory.inventorySlots[i].itemCount == 0)
            {
                return i;
            }
        }
        return -1; // Boþ slot yok
    }

    public void SaveData(GameData data)
    {
        if (data.inventorySaveData == null)
        {
            data.inventorySaveData = new InventorySaveData();
        }

        // Oyuncu envanterini kaydet
        data.inventorySaveData.playerInventorySlots = new List<SlotSaveData>();
        foreach (Slot slot in playerInventory.inventorySlots)
        {
            SlotSaveData slotData = new SlotSaveData
            {
                itemName = slot.item?.itemName ?? "",
                itemCount = slot.itemCount,
                isFull = slot.isFull
            };
            data.inventorySaveData.playerInventorySlots.Add(slotData);
        }

        // Çanta envanterini kaydet
        data.inventorySaveData.bagInventorySlots = new List<SlotSaveData>();
        foreach (Slot slot in bagInventory.inventorySlots)
        {
            SlotSaveData slotData = new SlotSaveData
            {
                itemName = slot.item?.itemName ?? "",
                itemCount = slot.itemCount,
                isFull = slot.isFull
            };
            data.inventorySaveData.bagInventorySlots.Add(slotData);
        }
    }

    public void LoadData(GameData data)
    {
        if (data.inventorySaveData == null) return;

        // Oyuncu envanterini yükle
        for (int i = 0; i < data.inventorySaveData.playerInventorySlots.Count && i < playerInventory.inventorySlots.Count; i++)
        {
            SlotSaveData slotData = data.inventorySaveData.playerInventorySlots[i];

            if (!string.IsNullOrEmpty(slotData.itemName))
            {
                // ScriptableObject'ten item'ý bul
                SCItem item = Resources.Load<SCItem>($"Items/{slotData.itemName}");
                if (item != null)
                {
                    playerInventory.inventorySlots[i].item = item;
                    playerInventory.inventorySlots[i].itemCount = slotData.itemCount;
                    playerInventory.inventorySlots[i].isFull = slotData.isFull;
                }
            }
            else
            {
                // Boþ slot
                playerInventory.inventorySlots[i] = new Slot();
            }
        }

        // Çanta envanterini yükle
        for (int i = 0; i < data.inventorySaveData.bagInventorySlots.Count && i < bagInventory.inventorySlots.Count; i++)
        {
            SlotSaveData slotData = data.inventorySaveData.bagInventorySlots[i];

            if (!string.IsNullOrEmpty(slotData.itemName))
            {
                // ScriptableObject'ten item'ý bul
                SCItem item = Resources.Load<SCItem>($"Items/{slotData.itemName}");
                if (item != null)
                {
                    bagInventory.inventorySlots[i].item = item;
                    bagInventory.inventorySlots[i].itemCount = slotData.itemCount;
                    bagInventory.inventorySlots[i].isFull = slotData.isFull;
                }
            }
            else
            {
                // Boþ slot
                bagInventory.inventorySlots[i] = new Slot();
            }
        }

        // UI'ý güncelle
        if (inventoryUIController != null)
        {
            inventoryUIController.UpdateUI(playerInventory);
            inventoryUIController.UpdateUI(bagInventory);
        }
    }
}