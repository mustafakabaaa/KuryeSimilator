using UnityEngine;

[CreateAssetMenu(fileName = "New Stat", menuName = "SC/Stats/CharacterStat")]
public class CharacterStat : ScriptableObject
{
    public string statID; // Örn: "speed", "jump", vs.

    public string statName;
    public Sprite icon;
    public float baseValue;
}