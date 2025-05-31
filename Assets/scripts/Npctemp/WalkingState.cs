using UnityEngine;
using UnityEngine.AI;

public class WalkingState : IState
{
    private NPCController npc;
    private PathList pathList;
    private int currentIndex;
    private Animator animator;
    private float stoppingDistance = 0.2f;
    private float idleDelay = 2f;

    public WalkingState(NPCController npc, PathList pathList, float walkingSpeed)
    {
        this.npc = npc;
        this.pathList = pathList;
        npc.navMeshAgent.speed = walkingSpeed;
    }

    public void Enter()
    {
        currentIndex = npc.CurrentWaypointIndex;
        animator = npc.GetComponent<Animator>();

        if (animator != null)
        {
            animator.ResetTrigger("KnockOut");
            animator.ResetTrigger("GetUpTrigger");
            animator.SetBool("isWalking", true);
            animator.SetBool("isIdle", false);
            animator.SetBool("isRunning", false);
        }

        npc.navMeshAgent.isStopped = false;
        MoveToNextWaypoint();
    }

    public void Update()
    {
        if (!npc.navMeshAgent.pathPending && npc.navMeshAgent.remainingDistance <= stoppingDistance)
        {
            npc.SetWaypointIndex((currentIndex + 1) % pathList.waypoints.Count);
            npc.ChangeState(new IdleState(npc, idleDelay));
        }
    }

    public void Exit()
    {
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
        }
    }

    private void MoveToNextWaypoint()
    {
        if (pathList != null && pathList.waypoints.Count > 0 && npc.navMeshAgent.isOnNavMesh)
        {
            Transform target = pathList.waypoints[currentIndex];
            npc.navMeshAgent.SetDestination(target.position);
        }
    }
}