using UnityEngine;
using UnityEngine.AI;

public class FleeState : IState
{
    private NPCController npc;
    private Transform playerTransform;
    private float fleeDuration = 3f; // Kaçma süresi
    private float timer;
    private float runSpeed = 4f; // Koþma hýzý

    public FleeState(NPCController npc, Transform playerTransform)
    {
        this.npc = npc;
        this.playerTransform = playerTransform;
    }

    public void Enter()
    {
        Animator anim = npc.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetBool("isRunning", true);
            anim.SetBool("isWalking", false);
            anim.SetBool("isIdle", false);
        }

        npc.navMeshAgent.speed = runSpeed;
        npc.navMeshAgent.updateRotation = true; // DÖNMEYÝ NAVMESHAGENT YAPSIN
        npc.navMeshAgent.isStopped = false;

        MoveAwayFromPlayer();
    }

    public void Update()
    {
        timer += Time.deltaTime;

        // Süre dolduysa WalkingState'e dön
        if (timer >= fleeDuration)
        {
            npc.ChangeState(new WalkingState(npc, npc.pathList, npc.walkingSpeed));
            return;
        }

        // Hedefe ulaþtýysa yeni kaçýþ yönü belirle
        if (!npc.navMeshAgent.pathPending && npc.navMeshAgent.remainingDistance < 0.5f)
        {
            MoveAwayFromPlayer();
        }
    }

    public void Exit()
    {
        Animator anim = npc.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetBool("isRunning", false);
        }

        npc.navMeshAgent.speed = npc.walkingSpeed;
    }

    private void MoveAwayFromPlayer()
    {
        Vector3 fleeDirection = (npc.transform.position - playerTransform.position).normalized;
        Vector3 targetPos = npc.transform.position + fleeDirection * 5f; // 5 birim uzaða git

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 2f, NavMesh.AllAreas))
        {
            npc.navMeshAgent.SetDestination(hit.position);
        }
    }
}
