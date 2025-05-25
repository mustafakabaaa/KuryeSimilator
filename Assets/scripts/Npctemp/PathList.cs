using System.Collections.Generic;
using UnityEngine;

public class PathList : MonoBehaviour
{
    public string pathID; // Benzersiz ID, inspector'dan verilebilir
    public List<Transform> waypoints = new List<Transform>();
}
