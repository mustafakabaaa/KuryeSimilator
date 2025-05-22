using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusteriNPC : MonoBehaviour, Iinterectable
{
    // Etkileşim ve Sipariş
    private SCOrderData assignedOrder;
    private string npcName = "Müşteri";
    private bool isTalking = false;

    // Yok Olma Mekaniği
    private bool orderCompleted = false;
    private Transform player;
    public float destroyDistance = 10f;
    public LayerMask obstacleLayers; // Engel layer'ları (Inspector'dan atayın)
    private Camera playerCamera;

    private bool isRotating = false; // NPC dönüyor mu?
    private float nextCheckTime;
    private float checkInterval = 0.3f;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerCamera = Camera.main; // Ana kamera referansı

    }

    public void SetOrder(SCOrderData order)
    {
        assignedOrder = order;
        npcName = order.orderName;
    }
    public void SetUITextBool(bool textSee)
    {
        isTalking = textSee;
    }

    private void Update()
    {
        if (orderCompleted && Time.time >= nextCheckTime)
        {
            CheckForDestroyConditions();
            nextCheckTime = Time.time + checkInterval;
        }
    }

    // MusteriNPC.cs'de Interact metodunu güncelle
    public void Interact()
    {
        if (assignedOrder == null || orderCompleted) return;

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

        if (!isRotating)
        {
            StartCoroutine(LookAtPlayerSmoothly());
        }
        isTalking = true;

        DialogueManager.Instance.SetCurrentOrderID(assignedOrder.orderID);

        // JSON veya ScriptableObject kontrolü
        if (assignedOrder.dialogueJson != null)
        {
            DialogueGraph jsonGraph = JSONDialogueLoader.ConvertJSONToDialogueGraph(assignedOrder.dialogueJson);
            DialogueUI.Instance.StartDialogue(jsonGraph, npcName, this.transform);
        }
        
    }

    public void SayMissingItems(List<string> missingItems)
    {
        string message = "Eksik ürünler: " + string.Join(", ", missingItems);
        DialogueUI.Instance.ShowSimpleMessage(message);
    }
    private IEnumerator LookAtPlayerSmoothly()
    {
        isRotating = true;

        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0; // Yükseklik farkını yok say

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            float rotationSpeed = 5f;

            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
                yield return null; // Her frame'de güncelle
            }
        }

        isRotating = false;
    }
    // Sipariş tamamlandığında OrderManager tarafından çağrılır
    public void CompleteOrder()
    {
        orderCompleted = true;
        // İsteğe bağlı: Teşekkür animasyonu veya diyalog
        GetComponent<Animator>()?.SetTrigger("ThankYou");
        DialogueUI.Instance.ShowSimpleMessage("Teşekkürler!");
    }
    private bool IsVisibleToPlayer()
    {
        // 1. NPC kamera görüş açısında mı? (Viewport check)
        Vector3 viewportPoint = playerCamera.WorldToViewportPoint(transform.position);
        bool isInView = viewportPoint.z > 0 &&
                        viewportPoint.x > 0 && viewportPoint.x < 1 &&
                        viewportPoint.y > 0 && viewportPoint.y < 1;

        // 2. Eğer kamera görüşünde değilse, zaten görünmüyor
        if (!isInView) return false;

        // 3. Kamera ile NPC arasında engel var mı? (Raycast)
        Vector3 cameraToNPC = transform.position - playerCamera.transform.position;
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, cameraToNPC.normalized, out hit, cameraToNPC.magnitude, obstacleLayers))
        {
            return hit.transform == transform; // Sadece NPC'ye çarptıysa görünüyor
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
            return hit.transform == player; // Sadece player'a çarptıysa görünüyor
        }
        return true; // Engel yoksa görünüyor
    }

    public string GetInteractionText()
    {
        return "KONUS (E)";
    }

    public bool CanInteract()
    {
        return !orderCompleted && !isTalking;

    }
    public void ResetTalkingState()
    {
        isTalking = false;
    }
    private void OnEnable()
    {
        DialogueUI.OnDialogueEnded += HandleDialogueEnded;
    }

    private void OnDisable()
    {
        DialogueUI.OnDialogueEnded -= HandleDialogueEnded;
    }

    private void HandleDialogueEnded()
    {
        isTalking = false;
    }

}
