using UnityEngine;

[System.Serializable]
public class NPCData
{
    public Vector3 position;
    public int health;
    public int maxHealth;
    public int currentWaypointIndex;

    // Ýstersen baþka bilgiler de ekleyebilirsin (örn. hangi state’deydi, zamanlayýcý deðerleri vs.)

    public NPCData(Vector3 pos, int health, int maxHealth, int waypointIndex)
    {
        this.position = pos;
        this.health = health;
        this.maxHealth = maxHealth;
        this.currentWaypointIndex = waypointIndex;
    }
}
