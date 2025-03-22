using UnityEngine;

[CreateAssetMenu(fileName = "NewDay", menuName = "Days/DayData")]
public class SCDayData : ScriptableObject
{
    public string dayName; // Gün adý (örneðin, "1. Gün")
    public SCOrderData[] orders; // Bu güne ait orderlar
}