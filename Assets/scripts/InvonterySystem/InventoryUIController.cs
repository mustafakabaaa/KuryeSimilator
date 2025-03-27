using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    public List<SlotUI> uiList = new List<SlotUI>(); // Slot UI elementleri
    public SCInventory playerInventory; // Oyuncunun envanteri
    public SCBagInventory bagInventory; // �antan�n envanteri
    private bool isShowingBag = false; // �anta envanteri mi g�steriliyor?

    private void Start()
    {
        playerInventory.maxUnlockedSlots = 8;
        
        UpdateUI(playerInventory); // Ba�lang��ta oyuncunun envanterini g�ster
    }

    public void OnChangeButtonClicked(int slotIndex)
    {
        if (isShowingBag)
        {
            // �antan�n envanteri a��ksa, �antadan oyuncuya ta��
            GetComponent<Inventory>().MoveItemFromBagToPlayer(slotIndex);
        }
        else
        {
            // Oyuncunun envanteri a��ksa, oyuncudan �antaya ta��
            GetComponent<Inventory>().MoveItemFromPlayerToBag(slotIndex);
        }

        // Envanter UI's�n� g�ncelle
        UpdateUI(isShowingBag ? bagInventory : playerInventory);
    }

    public void OnDropButtonClicked(int slotIndex)
    {
        if (isShowingBag)
        {
            // cantanin envanteri aciksa, cantadan slotu temizle
            GetComponent<Inventory>().ClearSlot(slotIndex);
        }
        else
        {
            // Oyuncunun envanteri a��ksa, oyuncunun slotunu temizle
            GetComponent<Inventory>().ClearSlot(slotIndex);
        }

        // Envanter UI's�n� g�ncelle
        UpdateUI(isShowingBag ? bagInventory : playerInventory);
    }
   
    // �antan�n envanterine ge�
    public void SwitchToBagInventory(SCBagInventory bagInventory)
    {
        this.bagInventory = bagInventory;
        isShowingBag = true;
        UpdateUI(bagInventory);
    }

    // Oyuncunun envanterine geri d�n
    public void SwitchToPlayerInventory()
    {
        isShowingBag = false;
        UpdateUI(playerInventory);
    }

    // UI'� g�ncelle (SCInventory i�in)
    public void UpdateUI(SCInventory inventory)
    {
        Debug.Log($"UI Güncelleniyor. Açık slot sayısı: {inventory.maxUnlockedSlots}");

        for (int i = 0; i < uiList.Count; i++)
        {
            bool slotUnlocked = inventory.IsSlotUnlocked(i);
            uiList[i].gameObject.SetActive(slotUnlocked); // Slotun kilidini kontrol et

            if (slotUnlocked && i < inventory.inventorySlots.Count && inventory.inventorySlots[i].itemCount > 0)
            {
                uiList[i].itemImage.sprite = inventory.inventorySlots[i].item.itemIcon;
                uiList[i].itemCountText.text = inventory.inventorySlots[i].itemCount.ToString();

                // Drop ve Change butonlar�n� g�ster
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

                // Drop ve Change butonlar�n� gizle
                uiList[i].DroppedButton.SetActive(false);
                if (uiList[i].ChangeButton != null)
                {
                    uiList[i].ChangeButton.gameObject.SetActive(false);
                }
            }
        }
    }

    // UI'� g�ncelle (SCBagInventory i�in)
    public void UpdateUI(SCBagInventory bagInventory)
    {

        for (int i = 0; i < uiList.Count; i++)
        {
            if (i < bagInventory.inventorySlots.Count && bagInventory.inventorySlots[i].itemCount > 0)
            {
                uiList[i].itemImage.sprite = bagInventory.inventorySlots[i].item.itemIcon;
                uiList[i].itemCountText.text = bagInventory.inventorySlots[i].itemCount.ToString();

                // Drop ve Change butonlar�n� g�ster
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

                // Drop ve Change butonlar�n� gizle
                uiList[i].DroppedButton.SetActive(false);
                if (uiList[i].ChangeButton != null)
                {
                    uiList[i].ChangeButton.gameObject.SetActive(false);
                }
            }
        }
    }

    // Hangi envanterin a��k oldu�unu d�nd�r
    public bool IsShowingBag()
    {
        return isShowingBag;
    }

    // buradaki fonksiyonun ismi farklı olabilir dikkat et COMMIT sırasında değiştirildi.
    public bool IsShowBagingFNC(bool isShow)
    {
        isShowingBag=isShow; 
        return isShowingBag;
    }
    // �antan�n envanterini d�nd�r
    public SCBagInventory GetBagInventory()
    {
        return bagInventory;
    }

    // Oyuncunun envanterini a�
    public void OpenPlayerInventory()
    {
       
        UpdateUI(playerInventory); // Oyuncunun envanterini g�ncelle
    }

    // Butonlar�n Selected durumunu s�f�rla
    public void ClearSelectedButton()
    {
        EventSystem.current.SetSelectedGameObject(null); // Se�ili butonu s�f�rla
    }
}