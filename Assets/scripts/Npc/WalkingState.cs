using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingState : INPCState
{
    public void EnterState(NPCController npc)
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

        // Waypoint'e do�ru hareket et
        npc.transform.position = Vector3.MoveTowards(npc.transform.position, targetWaypoint.position, walkingSpeed * Time.deltaTime);

        // X ve Z eksenlerindeki mesafeyi hesapla
        float distanceXZ = CalculateXZDistance(npc.transform.position, targetWaypoint.position);
        //Debug.Log("XZ Distance: " + distanceXZ);

        // Waypoint'e ula��ld���nda bir sonraki waypoint'e ge�
        if (distanceXZ <= 0.2f) 
        {
            timer += Time.deltaTime;
            if (timer > waitingWayPointTime)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % pathList.waypoints.Count;
                timer = 0;
            }
        }
    }
    private float CalculateXZDistance(Vector3 pos1, Vector3 pos2)
    {
        // Y�kseklik (y) fark�n� g�z ard� et, sadece x ve z eksenlerindeki fark� hesapla
        float dx = pos1.x - pos2.x;
        float dz = pos1.z - pos2.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }
    public void Exit()
    {
        Debug.Log("Exiting Walking State");
        npc.animator.SetBool("IsWalking", false);
    }
}
