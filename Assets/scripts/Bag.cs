using UnityEngine;

public class Bag : MonoBehaviour, Iinterectable
{
    public SCBagInventory bagInventory; // Çantanýn envanteri
    public GameObject inventoryGameobject;
    public void Interact()
    {
        // Çantanýn envanterini aç
        InventoryUIController inventoryUIController = FindObjectOfType<InventoryUIController>();
        if (inventoryUIController != null)
        {
            if(inventoryGameobject.activeSelf==false) 
            {
                inventoryUIController.SwitchToBagInventory(bagInventory);
                inventoryGameobject.SetActive(true);
                Cursor.lockState = CursorLockMode.None; // Ýmleci serbest býrak
                Cursor.visible = true; // Ýmleci göster
                inventoryUIController.ClearSelectedButton(); // Butonlarýn Selected durumunu sýfýrla

            }
            else { inventoryGameobject.SetActive(false);

                // Ýmleci kilitle ve gizle
                Cursor.lockState = CursorLockMode.Locked; // Ýmleci kilitle
                Cursor.visible = false; // Ýmleci gizle
                inventoryUIController.ClearSelectedButton(); // Butonlarýn Selected durumunu sýfýrla

            }

            inventoryUIController.SwitchToBagInventory(bagInventory);
        }
    }
}