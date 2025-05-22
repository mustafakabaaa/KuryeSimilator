using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "SC/NPC/Dialogue Graph")]
public class DialogueGraph : ScriptableObject
{
    public DialogueNode[] nodes;
    public int startNodeIndex = 0;
}