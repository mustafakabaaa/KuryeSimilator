using UnityEngine;

public class IdleState : IState
{
    private NPCController npc;
    private Animator animator;
    private float idleDuration = 2f; // Bekleme süresi (saniye)
    private float timer;

    public IdleState(NPCController npc, float idleDuration = 2f)
    {
        this.npc = npc;
        this.idleDuration = idleDuration;
    }

    public void Enter()
    {
        Debug.Log("Entering Idle State");
        animator = npc.GetComponent<Animator>();
        timer = 0f;

        if (animator != null)
        {
            animator.SetBool("isIdle", true);
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
        }
    }

    public void Update()
    {
        timer += Time.deltaTime;
        if (timer >= idleDuration)
        {
            npc.ChangeState(new WalkingState(npc, npc.pathList, npc.walkingSpeed));
        }
    }

    public void Exit()
    {
        Debug.Log("Exiting Idle State");
        if (animator != null)
        {
            animator.SetBool("isIdle", false);
        }
    }
}
