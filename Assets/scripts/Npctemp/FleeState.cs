using UnityEngine;

public class FleeState : IState
{
    private NPCController npc;
    private Transform playerTransform;
    private float fleeSpeed = 5f;
    private float fleeDuration = 5f; // Kaçma süresi
    private float timer;

    public FleeState(NPCController npc, Transform playerTransform)
    {
        this.npc = npc;
        this.playerTransform = playerTransform;
    }

    public void Enter()
    {
        Debug.Log("Entering Flee State");
        timer = 0f; // Sayaç sýfýrlandý
        npc.GetComponent<Animator>().SetBool("isWalking", true);
    }

    public void Update()
    {
        // NPC, oyuncudan uzaklaþacak
        Vector3 fleeDirection = (npc.transform.position - playerTransform.position).normalized;
        npc.transform.position += fleeDirection * fleeSpeed * Time.deltaTime;

        // Sayaç güncelleniyor
        timer += Time.deltaTime;

        // Belirli bir süre sonra WalkingState'e geç
        if (timer >= fleeDuration)
        {
            npc.ChangeState(new WalkingState(npc, npc.pathList, npc.walkingSpeed));
        }
    }

    public void Exit()
    {
        Debug.Log("Exiting Flee State");
        npc.GetComponent<Animator>().SetBool("isWalking", false);
    }
}