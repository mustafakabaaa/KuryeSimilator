using UnityEngine;
using System.Collections.Generic;

public class NPCPool : MonoBehaviour
{
    public static NPCPool Instance { get; private set; }

    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private int initialPoolSize = 5;

    private Queue<NPCController> npcPool = new Queue<NPCController>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewNPC();
        }
    }

    private void CreateNewNPC()
    {
        GameObject npcObj = Instantiate(npcPrefab);
        npcObj.SetActive(false);
        NPCController npc = npcObj.GetComponent<NPCController>();
        npcPool.Enqueue(npc);
    }

    public NPCController GetNPC()
    {
        if (npcPool.Count == 0)
        {
            CreateNewNPC();
        }

        NPCController npc = npcPool.Dequeue();
        npc.gameObject.SetActive(true);
        npc.SetActiveState(true);
        return npc;
    }

    public void ReturnNPC(NPCController npc)
    {
        npc.SetActiveState(false);
        npc.gameObject.SetActive(false);
        npcPool.Enqueue(npc);
    }
    // NPCPool scriptine yeni bir fonksiyon ekleyin:
    public NPCController GetNPC(Vector3 spawnPosition)
    {
        if (npcPool.Count == 0)
        {
            CreateNewNPC();
        }

        NPCController npc = npcPool.Dequeue();
        npc.transform.position = spawnPosition; // Pozisyonu burada ayarla
        npc.gameObject.SetActive(true);
        npc.SetActiveState(true);
        return npc;
    }
}