using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    public List<SlotUI> uiList = new List<SlotUI>(); // Slot UI elementleri
    public SCInventory playerInventory; // Oyuncunun envanteri
    public SCBagInventory bagInventory; // Çantanýn envanteri
    private bool isShowingBag = false; // Çanta envanteri mi gösteriliyor?

    private void Start()
    {
        
        UpdateUI(playerInventory); // Baþlangýçta oyuncunun envanterini göster
    }

    public void OnChangeButtonClicked(int slotIndex)
    {
        if (isShowingBag)
        {
            // Çantanýn envanteri açýksa, çantadan oyuncuya taþý
            GetComponent<Inventory>().MoveItemFromBagToPlayer(slotIndex);
        }
        else
        {
            // Oyuncunun envanteri açýksa, oyuncudan çantaya taþý
            GetComponent<Inventory>().MoveItemFromPlayerToBag(slotIndex);
        }

        // Envanter UI'sýný güncelle
        UpdateUI(isShowingBag ? bagInventory : playerInventory);
    }

    public void OnDropButtonClicked(int slotIndex)
    {
        if (isShowingBag)
        {
            // Çantanýn envanteri açýksa, çantadan slotu temizle
            GetComponent<Inventory>().ClearSlot(slotIndex);
        }
        else
        {
            // Oyuncunun envanteri açýksa, oyuncunun slotunu temizle
            GetComponent<Inventory>().ClearSlot(slotIndex);
        }

        // Envanter UI'sýný güncelle
        UpdateUI(isShowingBag ? bagInventory : playerInventory);
    }
    private void Update()
    {
        Debug.Log(isShowingBag);
    }
    // Çantanýn envanterine geç
    public void SwitchToBagInventory(SCBagInventory bagInventory)
    {
        this.bagInventory = bagInventory;
        isShowingBag = true;
        UpdateUI(bagInventory);
    }

    // Oyuncunun envanterine geri dön
    public void SwitchToPlayerInventory()
    {
        isShowingBag = false;
        UpdateUI(playerInventory);
    }

    // UI'ý güncelle (SCInventory için)
    public void UpdateUI(SCInventory inventory)
    {
        for (int i = 0; i < uiList.Count; i++)
        {
            if (i < inventory.inventorySlots.Count && inventory.inventorySlots[i].itemCount > 0)
            {
                uiList[i].itemImage.sprite = inventory.inventorySlots[i].item.itemIcon;
                uiList[i].itemCountText.text = inventory.inventorySlots[i].itemCount.ToString();

                // Drop ve Change butonlarýný göster
                uiList[i].DroppedButton.SetActive(true);
                if (uiList[i].ChangeButton != null)
                {
                    uiList[i].ChangeButton.gameObject.SetActive(true);
                }

                if (!inventory.inventorySlots[i].item.canStackable)
                {
                    uiList[i].itemCountText.text = "";
                }
            }
            else
            {
                uiList[i].itemImage.sprite = null;
                uiList[i].itemCountText.text = "";

                // Drop ve Change butonlarýný gizle
                uiList[i].DroppedButton.SetActive(false);
                if (uiList[i].ChangeButton != null)
                {
                    uiList[i].ChangeButton.gameObject.SetActive(false);
                }
            }
        }
    }

    // UI'ý güncelle (SCBagInventory için)
    public void UpdateUI(SCBagInventory bagInventory)
    {
        for (int i = 0; i < uiList.Count; i++)
        {
            if (i < bagInventory.inventorySlots.Count && bagInventory.inventorySlots[i].itemCount > 0)
            {
                uiList[i].itemImage.sprite = bagInventory.inventorySlots[i].item.itemIcon;
                uiList[i].itemCountText.text = bagInventory.inventorySlots[i].itemCount.ToString();

                // Drop ve Change butonlarýný göster
                uiList[i].DroppedButton.SetActive(true);
                if (uiList[i].ChangeButton != null)
                {
                    uiList[i].ChangeButton.gameObject.SetActive(true);
                }

                if (!bagInventory.inventorySlots[i].item.canStackable)
                {
                    uiList[i].itemCountText.text = "";
                }
            }
            else
            {
                uiList[i].itemImage.sprite = null;
                uiList[i].itemCountText.text = "";

                // Drop ve Change butonlarýný gizle
                uiList[i].DroppedButton.SetActive(false);
                if (uiList[i].ChangeButton != null)
                {
                    uiList[i].ChangeButton.gameObject.SetActive(false);
                }
            }
        }
    }

    // Hangi envanterin açýk olduðunu döndür
    public bool IsShowingBag()
    {
        return isShowingBag;
    }
    public bool IsShowBagýngFNC(bool isShow)
    {
        isShowingBag=isShow; 
        return isShowingBag;
    }
    // Çantanýn envanterini döndür
    public SCBagInventory GetBagInventory()
    {
        return bagInventory;
    }

    // Oyuncunun envanterini aç
    public void OpenPlayerInventory()
    {
       
        UpdateUI(playerInventory); // Oyuncunun envanterini güncelle
    }

    // Butonlarýn Selected durumunu sýfýrla
    public void ClearSelectedButton()
    {
        EventSystem.current.SetSelectedGameObject(null); // Seçili butonu sýfýrla
    }
}