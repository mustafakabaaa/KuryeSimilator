//using UnityEngine;
//using System.IO;

//public class DialogueLoader : MonoBehaviour
//{
//    public static DialogueLoader Instance;
//    public DialogueDatabase dialogueDB;

//    private void Awake()
//    {
//        if (Instance == null)
//            Instance = this;
//        else
//            Destroy(gameObject);

//        LoadDialogueFromJSON();
//    }

//    private void LoadDialogueFromJSON()
//    {
//        TextAsset jsonText = Resources.Load<TextAsset>("Dialogues/dialogue_data");
//        dialogueDB = JsonUtility.FromJson<DialogueDatabase>(jsonText.text);
//    }

//    public DialogueData GetDialogueByID(string id)
//    {
//        foreach (var dialogue in dialogueDB.dialogues)
//        {
//            if (dialogue.dialogueID == id)
//                return dialogue;
//        }

//        Debug.LogWarning("Dialogue ID not found: " + id);
//        return null;
//    }
//}
