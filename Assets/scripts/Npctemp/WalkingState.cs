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
        Debug.Log("Entering Walking State");
    }

    public void Update()
    {
        if (pathList.waypoints.Count == 0)
        {
            Debug.LogWarning("No waypoints assigned!");
            return;
        }

        Transform targetWaypoint = pathList.waypoints[currentWaypointIndex];
        if (targetWaypoint == null)
            return;

        // X ve Z eksenlerindeki mesafeyi hesapla
        float distanceXZ = CalculateXZDistance(npc.transform.position, targetWaypoint.position);


        if (distanceXZ > 0.2f) // Hedefe yürüyorsa
        {
            // Hareket
            npc.transform.position = Vector3.MoveTowards(npc.transform.position, targetWaypoint.position, walkingSpeed * Time.deltaTime);

            // Dönüþ
            Vector3 direction = (targetWaypoint.position - npc.transform.position);
            direction.y = 0f;
            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation, targetRotation, 5f * Time.deltaTime);
            }

            // Animator'a "isWalking = true"
            npc.GetComponent<Animator>().SetBool("isWalking", true);
        }
        else // Bekleme zamaný
        {
            timer += Time.deltaTime;

            // Animator'a "isWalking = false"
            npc.GetComponent<Animator>().SetBool("isWalking", false);

            if (timer > waitingWayPointTime)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % pathList.waypoints.Count;
                timer = 0;
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