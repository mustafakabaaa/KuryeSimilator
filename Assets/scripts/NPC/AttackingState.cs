using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackingState : INPCState
{
    public void EnterState(NPCController npc)
    {
        Debug.Log("Entering Attacking State");
        npc.animator.SetTrigger("Attack");
    }

    public void UpdateState(NPCController npc)
    {
        float distanceToPlayer = Vector3.Distance(npc.transform.position, npc.player.position);

        if (distanceToPlayer > npc.attackRange)
        {
            npc.TransitionToState(new WalkingState());
        }
        else
        {
            npc.AttackPlayer();
        }
    }

    public void ExitState(NPCController npc)
    {
        Debug.Log("Exiting Attacking State");
        npc.animator.ResetTrigger("Attack");
    }
}
