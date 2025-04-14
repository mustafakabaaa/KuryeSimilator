using UnityEngine;

public class Bag : MonoBehaviour, Iinterectable
{
    public SCBagInventory bagInventory; // Çantanýn envanteri
    public GameObject inventoryGameobject;
    public float closeDistance = 3f; // Envanterin kapanacaðý maksimum mesafe
    private Transform playerTransform; // Oyuncunun transform bileþeni
    private bool isInventoryOpen = false; // Envanter açýk mý?

    private void Start()
    {
        // Oyuncunun transform bileþenini bul (Player tag'ine sahip olduðunu varsayýyoruz)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        // Eðer envanter açýksa ve oyuncu transformu varsa mesafeyi kontrol et
        if (isInventoryOpen && playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance > closeDistance)
            {
                CloseInventory();
            }
        }
    }

    public void Interact()
    {
        // Çantanýn envanterini aç/kapat
        InventoryUIController inventoryUIController = FindObjectOfType<InventoryUIController>();
        if (inventoryUIController != null)
        {
            if (!isInventoryOpen)
            {
                OpenInventory(inventoryUIController);
            }
            else
            {
                CloseInventory();
            }
        }
    }

    private void OpenInventory(InventoryUIController inventoryUIController)
    {
        inventoryUIController.SwitchToBagInventory(bagInventory);
        inventoryGameobject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        inventoryUIController.ClearSelectedButton();
        isInventoryOpen = true;
    }

    private void CloseInventory()
    {
        InventoryUIController inventoryUIController = FindObjectOfType<InventoryUIController>();
        if (inventoryUIController != null)
        {
            inventoryGameobject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            inventoryUIController.ClearSelectedButton();
            isInventoryOpen = false;
        }
    }
}