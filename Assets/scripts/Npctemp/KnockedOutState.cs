using UnityEngine;

public class KnockedOutState : IState
{
    private NPCController npc;
    private float knockoutDuration = 5f; // Bayýlma süresi
    private float timer;

    public KnockedOutState(NPCController npc)
    {
        this.npc = npc;
    }

    public void Enter()
    {
        Debug.Log("Entering Knocked Out State");
        timer = 0f;
    }

    public void Update()
    {
        timer += Time.deltaTime;

        // Bayýlma süresi dolduðunda WalkingState'e geç
        if (timer >= knockoutDuration)
        {
            npc.ResetHealth(); // Caný yenile
            npc.ChangeState(new WalkingState(npc, npc.pathList, npc.walkingSpeed));
        }
    }

    public void Exit()
    {
        Debug.Log("Exiting Knocked Out State");
    }

}