using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingState : INPCState
{
    public void EnterState(NPCController npc)
    {
        Debug.Log("Entering Walking State");
        npc.animator.SetBool("IsWalking", true);
    }

    public void UpdateState(NPCController npc)
    {
        float distanceToPlayer = Vector3.Distance(npc.transform.position, npc.player.position);

        if (distanceToPlayer <= npc.attackRange)
        {
            npc.TransitionToState(new AttackingState());
        }
        else if (distanceToPlayer > npc.detectionRange)
        {
            npc.TransitionToState(new IdleState());
        }
        else
        {
            npc.MoveTowardsPlayer(npc.walkSpeed);
        }
    }

    public void ExitState(NPCController npc)
    {
        Debug.Log("Exiting Walking State");
        npc.animator.SetBool("IsWalking", false);
    }
}
