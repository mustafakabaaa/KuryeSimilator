using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    public List<SlotUI> uiList = new List<SlotUI>(); // Slot UI elementleri
    public SCInventory playerInventory; // Oyuncunun envanteri
    public SCBagInventory bagInventory; // �antan�n envanteri
    private bool isShowingBag = false; // �anta envanteri mi g�steriliyor?
    public GameObject inventoryGameobject;
    public TextMeshProUGUI interactText; // Etkilesim metni

    private void Start()
    {
        playerInventory.maxUnlockedSlots = 8;
        
        UpdateUI(playerInventory); // Ba�lang��ta oyuncunun envanterini g�ster
                                   // Fareyi kilitle
        
        
        Cursor.lockState = CursorLockMode.Locked;
        interactText.gameObject.SetActive(false);


        inventoryGameobject.SetActive(false);
        Cursor.visible = false;

    }
    private void Update()
    {
        InventoryOpenAndClose();
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
    public void InventoryOpenAndClose()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            UpdateUI(playerInventory);
            

            if (inventoryGameobject.activeSelf)
            {
                // Envanteri kapat
                inventoryGameobject.SetActive(false);
                UIManager.Instance.SetInventoryState(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
              
            }
            else if (!UIManager.Instance.IsAnyUIOpen())
            {
                // Envanteri aç
                inventoryGameobject.SetActive(true);
                UIManager.Instance.SetInventoryState(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
               


            }
        }
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

    public void UpdateUI(SCInventory inventory)
    {
        Debug.Log($"UI Güncelleniyor. Açık slot sayısı: {inventory.maxUnlockedSlots}");

        for (int i = 0; i < uiList.Count; i++)
        {
            bool slotUnlocked = inventory.IsSlotUnlocked(i);
            uiList[i].gameObject.SetActive(slotUnlocked);

            if (slotUnlocked && i < inventory.inventorySlots.Count)
            {
                Slot slot = inventory.inventorySlots[i];
                bool hasItem = slot.item != null && slot.itemCount > 0;

                uiList[i].itemImage.sprite = hasItem ? slot.item.itemIcon : null;
                uiList[i].itemCountText.text = hasItem ? slot.itemCount.ToString() : "";

                uiList[i].DroppedButton.SetActive(hasItem);
                if (uiList[i].ChangeButton != null)
                {
                    uiList[i].ChangeButton.gameObject.SetActive(hasItem);
                }

                if (hasItem && !slot.item.canStackable)
                {
                    uiList[i].itemCountText.text = "";
                }
            }
            else
            {
                uiList[i].itemImage.sprite = null;
                uiList[i].itemCountText.text = "";
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
    public bool IsInventoryOpen()
    {
        return inventoryGameobject.activeSelf;
    }
}