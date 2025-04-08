using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance;

    [Header("UI Referanslarý")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI npcDialogueText;
    [SerializeField] private Transform optionsParent;
    [SerializeField] private GameObject optionButtonPrefab;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    public void StartDialogue(DialogueGraph dialogue, string npcName)
    {
        UIManager.Instance.SetDialogueState(true); // Durumu güncelle
        
        UIManager.Instance.CloseAllOpenPanels(); // Diðer UI'larý kapat
        SetCursorState(true);
        if (dialogue == null)
        {
            Debug.LogError("DialogueGraph is null!", this);
            return;
        }
       
        // ÖNCE DialogueManager'a diyalogu ata
        DialogueManager.Instance.SetCurrentDialogue(dialogue);

        npcNameText.text = npcName;
        panel.SetActive(true);

        // startNodeIndex kontrolü ekle
        if (dialogue.startNodeIndex < 0 || dialogue.startNodeIndex >= dialogue.nodes.Length)
        {
            Debug.LogError($"Invalid startNodeIndex: {dialogue.startNodeIndex}");
            return;
        }

        ShowNode(dialogue.nodes[dialogue.startNodeIndex]);
    }
    public void CloseDialogue()
    {
        // 1. Önce paneli kapat
        panel.SetActive(false);

        // 2. UI durumunu güncelle
        UIManager.Instance.SetDialogueState(false);

        // 3. Ýmleç kontrolü (Debug ekleyerek test edin)
        Debug.Log("Closing dialogue - Setting cursor: visible=false, locked");
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // 4. Input sistemini resetle (EventSystem çakýþmalarý için)
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }
    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
    private void ShowNode(DialogueNode node)
    {
        npcDialogueText.text = node.npcText;

        // Önceki seçenekleri temizle
        foreach (Transform child in optionsParent)
            Destroy(child.gameObject);

        // Yeni seçenekleri oluþtur
        foreach (var option in node.playerOptions)
        {
            GameObject button = Instantiate(optionButtonPrefab, optionsParent);
            button.GetComponentInChildren<TextMeshProUGUI>().text = option.text;
            button.GetComponent<Button>().onClick.AddListener(() => SelectOption(option.nextNodeIndex));
        }
    }
    public void ShowSimpleMessage(string message)
    {
        // Paneli aç
        panel.SetActive(true);

        // Mesajý göster
        npcDialogueText.text = message;

        // Seçenekleri temizle
        foreach (Transform child in optionsParent)
            Destroy(child.gameObject);

        // 2 saniye sonra otomatik kapat
        StartCoroutine(AutoCloseMessage());
    }

    private IEnumerator AutoCloseMessage()
    {
        yield return new WaitForSeconds(2f);
        CloseDialogue();
    }
    // DialogueUI.cs
    private void SelectOption(int nextNodeIndex)
    {
        if (nextNodeIndex == -1)
        {
            // Diyalog tamamlandýðýnda sipariþi teslim et
            string completedOrderID = DialogueManager.Instance.GetCurrentOrderID();
            if (!string.IsNullOrEmpty(completedOrderID))
            {
                OrderManager.Instance.CompleteOrder(completedOrderID);
            }
            CloseDialogue();
        }
        else
        {
            ShowNode(DialogueManager.Instance.GetNode(nextNodeIndex));
        }
    }
}