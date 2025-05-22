// JSONDialogueSystem.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class JSONDialogueNode
{
    public string nodeId;
    public string npcText;
    public List<JSONDialogueOption> options;
}

[Serializable]
public class JSONDialogueOption
{
    public string text;
    public string nextNodeId;
    public int tipEffect;
}

[Serializable]
public class JSONDialogueGraph
{
    public string startNodeId;
    public List<JSONDialogueNode> nodes;
}