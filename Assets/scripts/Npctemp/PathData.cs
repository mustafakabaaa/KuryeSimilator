using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PathData
{
    public PathList pathList;
    public int maxNPCCount = 3;

    [HideInInspector] public List<NPCController> spawnedNPCs = new List<NPCController>();
}
