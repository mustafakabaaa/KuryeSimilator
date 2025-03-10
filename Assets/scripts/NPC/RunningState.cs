using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunningState : INPCState
{
    public void EnterState(NPCController npc)
    {
        Debug.Log("Entering Running State");
        npc.animator.SetBool("IsRunning", true);
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
            npc.MoveTowardsPlayer(npc.runSpeed);
        }
    }

    public void ExitState(NPCController npc)
    {
        Debug.Log("Exiting Running State");
        npc.animator.SetBool("IsRunning", false);
    }
}
