using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INPCState 
{
    void EnterState(NPCController npc);
    void UpdateState(NPCController npc);
    void ExitState(NPCController npc);
}
