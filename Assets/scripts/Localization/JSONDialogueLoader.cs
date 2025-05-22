using System.Collections.Generic;
using UnityEngine;

public static class JSONDialogueLoader
{
    public static DialogueGraph ConvertJSONToDialogueGraph(TextAsset jsonFile)
    {
        JSONDialogueGraph jsonGraph = JsonUtility.FromJson<JSONDialogueGraph>(jsonFile.text);

        DialogueGraph graph = ScriptableObject.CreateInstance<DialogueGraph>();
        graph.nodes = new DialogueNode[jsonGraph.nodes.Count];

        // Node'ları dönüştür
        Dictionary<string, int> nodeIdToIndex = new Dictionary<string, int>();
        for (int i = 0; i < jsonGraph.nodes.Count; i++)
        {
            var jsonNode = jsonGraph.nodes[i];
            nodeIdToIndex[jsonNode.nodeId] = i;

            graph.nodes[i] = new DialogueNode
            {
                npcText = jsonNode.npcText,
                playerOptions = new DialogueOption[jsonNode.options.Count]
            };

            for (int j = 0; j < jsonNode.options.Count; j++)
            {
                var jsonOption = jsonNode.options[j];
                graph.nodes[i].playerOptions[j] = new DialogueOption
                {
                    text = jsonOption.text,
                    tipEffect = jsonOption.tipEffect,
                    nextNodeIndex = -1 // Geçici olarak -1, sonra doldurulacak
                };
            }
        }

        // NextNodeIndex'leri doldur (EN ÖNEMLİ DEĞİŞİKLİK BURADA)
        for (int i = 0; i < jsonGraph.nodes.Count; i++)
        {
            var jsonNode = jsonGraph.nodes[i];
            for (int j = 0; j < jsonNode.options.Count; j++)
            {
                string nextNodeId = jsonNode.options[j].nextNodeId;

                // ÖZEL DURUMLAR (-1 ve -2 için)
                if (nextNodeId == "-1" || nextNodeId == "-2")
                {
                    graph.nodes[i].playerOptions[j].nextNodeIndex = int.Parse(nextNodeId);
                    Debug.Log($"Özel durum ayarlandı: {nextNodeId} → {graph.nodes[i].playerOptions[j].nextNodeIndex}");
                }
                // Normal node ID'leri
                else if (nodeIdToIndex.TryGetValue(nextNodeId, out int nextIndex))
                {
                    graph.nodes[i].playerOptions[j].nextNodeIndex = nextIndex;
                }
                else
                {
                    Debug.LogError($"Node ID bulunamadı: {nextNodeId}");
                    graph.nodes[i].playerOptions[j].nextNodeIndex = -1; // Fallback
                }
            }
        }

        // Start node indexini bul
        graph.startNodeIndex = nodeIdToIndex[jsonGraph.startNodeId];

        return graph;
    }
}