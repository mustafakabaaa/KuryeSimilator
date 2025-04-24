using UnityEngine;

public class IdleState : IState
{
    private NPCController npc;

    public IdleState(NPCController npc)
    {
        this.npc = npc;
    }

    public void Enter()
    {
        Debug.Log("Entering Idle State");
    }

    public void Update()
    {
        // Idle durumunda yapýlacak iþlemler
    }

    public void Exit()
    {
        Debug.Log("Exiting Idle State");
    }
}