using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class IdleState : INPCState
{
    public void EnterState(NPCController npc)
    {
        Debug.Log("Enter idle state");
        // npc animator
    }

    public void ExitState(NPCController npc)
    {
        float distanceToPlayer = Vector3.Distance(npc.transform.position, npc.player.position);

        if (distanceToPlayer <= npc.detectionRange)
        {
            if (distanceToPlayer > npc.attackRange)
            {
                npc.TransitionToState(new WalkingState());
            }
            else
            {
                npc.TransitionToState(new AttackingState());
            }
        }
    }
    
    public void UpdateState(NPCController npc)
    {
        Debug.Log("Exiting Idle State");
    }
}
  
