using UnityEngine;

public class WalkingState : IState
{
    private NPCController npc;
    private PathList pathList;
    private float walkingSpeed;
    private int currentWaypointIndex = 0;
    private float timer = 0;
    private float waitingWayPointTime = 2;
    public WalkingState(NPCController npc, PathList pathList, float walkingSpeed)
    {
        this.npc = npc;
        this.pathList = pathList;
        this.walkingSpeed = walkingSpeed;
    }

    public void Enter()
    {
        npc.navMeshAgent.speed = walkingSpeed;
        npc.navMeshAgent.SetDestination(pathList.waypoints[currentWaypointIndex].position);
        npc.GetComponent<Animator>().SetBool("isWalking", true);
    }

    public void Update()
    {
        if (!npc.navMeshAgent.pathPending && npc.navMeshAgent.remainingDistance <= 0.2f)
        {
            timer += Time.deltaTime;
            npc.GetComponent<Animator>().SetBool("isWalking", false);

            if (timer > waitingWayPointTime)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % pathList.waypoints.Count;
                npc.navMeshAgent.SetDestination(pathList.waypoints[currentWaypointIndex].position);
                npc.SetWaypointIndex(currentWaypointIndex); // Güncelleme burada
                timer = 0;
                npc.GetComponent<Animator>().SetBool("isWalking", true);
            }
        }
    }


    private float CalculateXZDistance(Vector3 pos1, Vector3 pos2)
    {
        // Yükseklik (y) farkýný göz ardý et, sadece x ve z eksenlerindeki farký hesapla
        float dx = pos1.x - pos2.x;
        float dz = pos1.z - pos2.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }
    public void Exit()
    {
        Debug.Log("Exiting Walking State");
    }
}