using UnityEngine;
using UnityEngine.UI;

public class SlotButtonHandler : MonoBehaviour
{
    public int slotIndex; // Bu butonun baðlý olduðu slot index'i
    public Button changeButton; // Change butonu
    public Button dropButton; // Drop butonu

    private InventoryUIController inventoryUIController;

    private void Start()
    {
        // InventoryUIController'ý bul
        inventoryUIController = FindObjectOfType<InventoryUIController>();

        // Butonlara onClick olaylarýný dinamik olarak ata
        if (changeButton != null)
        {
            changeButton.onClick.AddListener(OnChangeButtonClicked);
        }

        if (dropButton != null)
        {
            dropButton.onClick.AddListener(OnDropButtonClicked);
        }
    }

    // Change butonu týklandýðýnda çaðrýlacak fonksiyon
    private void OnChangeButtonClicked()
    {
        inventoryUIController.OnChangeButtonClicked(slotIndex);
    }

    // Drop butonu týklandýðýnda çaðrýlacak fonksiyon
    private void OnDropButtonClicked()
    {
        inventoryUIController.OnDropButtonClicked(slotIndex);
    }
}