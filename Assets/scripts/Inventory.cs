using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance; // Singleton örneði

    public SCInventory playerInventory; // Oyuncunun envanteri
    public SCBagInventory bagInventory; // Çantanýn envanteri
    private InventoryUIController inventoryUIController;
    bool isSwapping;
    int tempIndex;
    Slot tempSlot;
   
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
    public void RemoveItem(string itemID)
    {
        // Oyuncunun envanterinden item'i kaldýr
        foreach (Slot slot in playerInventory.inventorySlots)
        {
            if (slot.item != null && slot.item.itemID == itemID && slot.itemCount > 0)
            {
                slot.itemCount--; // Item sayýsýný azalt
                if (slot.itemCount <= 0)
                {
                    slot.item = null; // Slotu boþalt
                    slot.isFull = false;
                }
                Debug.Log("Item removed from player inventory: " + itemID);
                return; // Item bulundu ve kaldýrýldý
            }
        }

        // Çantanýn envanterinden item'i kaldýr (eðer çanta açýksa)
        if (inventoryUIController.IsShowingBag())
        {
            foreach (Slot slot in bagInventory.inventorySlots)
            {
                if (slot.item != null && slot.item.itemID == itemID && slot.itemCount > 0)
                {
                    slot.itemCount--; // Item sayýsýný azalt
                    if (slot.itemCount <= 0)
                    {
                        slot.item = null; // Slotu boþalt
                        slot.isFull = false;
                    }
                    Debug.Log("Item removed from bag inventory: " + itemID);
                    return; // Item bulundu ve kaldýrýldý
                }
            }
        }

        Debug.LogWarning("Item not found in any inventory: " + itemID);
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
            // Çantanýn envanteri açýksa, çantanýn envanteri üzerinde slot temizleme iþlemi yap
            if (slotIndex >= 0 && slotIndex < bagInventory.inventorySlots.Count)
            {
                bagInventory.inventorySlots[slotIndex].item = null;
                bagInventory.inventorySlots[slotIndex].itemCount = 0;
                bagInventory.inventorySlots[slotIndex].isFull = false;
                DropItem(slotIndex);
                // Çantanýn envanterini güncelle
                inventoryUIController.UpdateUI(bagInventory);
            }
        }
        else
        {
            // Oyuncunun envanteri açýksa, oyuncunun envanteri üzerinde slot temizleme iþlemi yap
            if (slotIndex >= 0 && slotIndex < playerInventory.inventorySlots.Count)
            {
                playerInventory.inventorySlots[slotIndex].item = null;
                playerInventory.inventorySlots[slotIndex].itemCount = 0;
                playerInventory.inventorySlots[slotIndex].isFull = false;
                DropItem(slotIndex);

                // Oyuncunun envanterini güncelle
                inventoryUIController.UpdateUI(playerInventory);
            }
        }
    }
    public void DropItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= playerInventory.inventorySlots.Count)
            return;

        Slot slot = playerInventory.inventorySlots[slotIndex];

        // Slot boþsa iþlem yapma
        if (slot.item == null || slot.itemCount <= 0)
            return;

        // Nesneyi sahneye spawn et
        if (slot.item.itemPrefab != null)
        {
            Vector3 spawnPosition = transform.position + transform.forward * 2f; // Karakterin önünde spawn et
            Instantiate(slot.item.itemPrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Item prefab is missing!");
        }

        // Slotu temizle
        playerInventory.inventorySlots[slotIndex] = new Slot();

        // UI'ý güncelle
        GetComponent<InventoryUIController>().UpdateUI(playerInventory);
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

    // Oyuncunun envanterinde boþ slot bul
    private int FindEmptySlotInPlayer()
    {
        for (int i = 0; i < playerInventory.inventorySlots.Count; i++)
        {
            if (playerInventory.inventorySlots[i].item == null || playerInventory.inventorySlots[i].itemCount == 0)
            {
                return i;
            }
        }
        return -1; // Boþ slot yok
    }
   
}