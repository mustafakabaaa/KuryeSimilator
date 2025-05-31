using UnityEngine;

public class KnockedOutState : IState
{
    private NPCController npc;
    private float knockoutDuration = 5f;
    private float timer;
    private Animator animator;
    private bool hasTriggeredGetUp = false;

    public KnockedOutState(NPCController npc)
    {
        this.npc = npc;
        animator = npc.GetComponent<Animator>();
    }

    public void Enter()
    {
        Debug.Log("NPC bayýldý!");
        timer = 0f;
        hasTriggeredGetUp = false;
        npc.isInvulnerable = true; // Hasar alamaz yap
        npc.currentKnockoutState = this;

        if (animator != null)
        {
            animator.SetTrigger("KnockOut");
            animator.SetBool("isWalking", false);
            animator.SetBool("isIdle", false);
        }

        npc.navMeshAgent.isStopped = true;
    }

    public void Update()
    {
        timer += Time.deltaTime;

        if (timer >= knockoutDuration && !hasTriggeredGetUp)
        {
            if (animator != null)
            {
                animator.SetTrigger("GetUpTrigger");
                hasTriggeredGetUp = true;
            }
            npc.ResetHealth();
        }
    }

    public void OnGetUpComplete()
    {
        npc.isInvulnerable = false; // Tekrar hasar alabilir yap
        npc.ChangeState(new WalkingState(npc, npc.pathList, npc.walkingSpeed));
    }

    public void Exit()
    {
        npc.currentKnockoutState = null;
        npc.isInvulnerable = false; // Güvenlik önlemi
        npc.navMeshAgent.isStopped = false;
    }
}