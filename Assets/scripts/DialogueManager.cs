using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    private DialogueGraph currentDialogue;
    private string _currentOrderID;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SetCurrentDialogue(DialogueGraph dialogue)
    {
        currentDialogue = dialogue;
        Debug.Log($"Current dialogue set to: {dialogue.name}");
    }
    public void SetCurrentOrderID(string orderID)
    {
        _currentOrderID = orderID;
    }

    public string GetCurrentOrderID()
    {
        return _currentOrderID;
    }
    public DialogueNode GetNode(int index)
    {
        if (currentDialogue == null)
        {
            Debug.LogError("Current dialogue is null! Did you call SetCurrentDialogue()?");
            return null;
        }

        if (index == -1) return null; // Diyaloðu bitir

        if (index < 0 || index >= currentDialogue.nodes.Length)
        {
            Debug.LogError($"Invalid node index: {index}. Dialogue has {currentDialogue.nodes.Length} nodes.");
            return null;
        }

        return currentDialogue.nodes[index];
    }
}