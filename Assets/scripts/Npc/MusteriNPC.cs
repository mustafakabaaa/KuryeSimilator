using System.Collections.Generic;
using UnityEngine;

public class MusteriNPC : MonoBehaviour, Iinterectable
{
    // Etkileþim ve Sipariþ
    private SCOrderData assignedOrder;
    private string npcName = "Müþteri";

    // Yok Olma Mekaniði
    private bool orderCompleted = false;
    private Transform player;
    public float destroyDistance = 10f;
    public LayerMask obstacleLayers; // Engel layer'larý (Inspector'dan atayýn)
    private Camera playerCamera;
   
    
    private float nextCheckTime;
    private float checkInterval = 0.3f;
    
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerCamera = Camera.main; // Ana kamera referansý

    }

    public void SetOrder(SCOrderData order)
    {
        assignedOrder = order;
        npcName = order.orderName;
    }

    private void Update()
    {
        if (orderCompleted && Time.time >= nextCheckTime)
        {
            CheckForDestroyConditions();
            nextCheckTime = Time.time + checkInterval;
        }
    }

    // IInteractable arayüzü
    public void Interact()
    {
        if (assignedOrder == null) return;

        bool hasAllItems = true;
        List<string> missingItems = new List<string>();

        foreach (SCItem item in assignedOrder.requiredItems)
        {
            if (!Inventory.Instance.HasItem(item.itemID))
            {
                hasAllItems = false;
                missingItems.Add(item.itemName);
            }
        }

        if (!hasAllItems)
        {
            SayMissingItems(missingItems);
            return;
        }

        DialogueManager.Instance.SetCurrentOrderID(assignedOrder.orderID);
        DialogueUI.Instance.StartDialogue(assignedOrder.dialogueData, npcName);
    }

    public void SayMissingItems(List<string> missingItems)
    {
        string message = "Eksik ürünler: " + string.Join(", ", missingItems);
        DialogueUI.Instance.ShowSimpleMessage(message);
    }

    // Sipariþ tamamlandýðýnda OrderManager tarafýndan çaðrýlýr
    public void CompleteOrder()
    {
        orderCompleted = true;
        // Ýsteðe baðlý: Teþekkür animasyonu veya diyalog
        GetComponent<Animator>()?.SetTrigger("ThankYou");
        DialogueUI.Instance.ShowSimpleMessage("Teþekkürler!");
    }
    private bool IsVisibleToPlayer()
    {
        // 1. NPC kamera görüþ açýsýnda mý? (Viewport check)
        Vector3 viewportPoint = playerCamera.WorldToViewportPoint(transform.position);
        bool isInView = viewportPoint.z > 0 &&
                        viewportPoint.x > 0 && viewportPoint.x < 1 &&
                        viewportPoint.y > 0 && viewportPoint.y < 1;

        // 2. Eðer kamera görüþünde deðilse, zaten görünmüyor
        if (!isInView) return false;

        // 3. Kamera ile NPC arasýnda engel var mý? (Raycast)
        Vector3 cameraToNPC = transform.position - playerCamera.transform.position;
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, cameraToNPC.normalized, out hit, cameraToNPC.magnitude, obstacleLayers))
        {
            return hit.transform == transform; // Sadece NPC'ye çarptýysa görünüyor
        }

        return true; // Engel yoksa görünüyor
    }
    private void CheckForDestroyConditions()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > destroyDistance && !IsVisibleToPlayer())
        {
            Destroy(gameObject);
        }
    }

    private bool IsPlayerSeeingNPC()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        RaycastHit hit;

        // Raycast ile engel kontrolü
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, destroyDistance, obstacleLayers))
        {
            return hit.transform == player; // Sadece player'a çarptýysa görünüyor
        }
        return true; // Engel yoksa görünüyor
    }
}