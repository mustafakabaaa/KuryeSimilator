using UnityEngine;

[CreateAssetMenu(fileName = "New Stat", menuName = "SC/Stats/CharacterStat")]
public class CharacterStat : ScriptableObject
{
    public string statName;
    public Sprite icon;
    public float baseValue;
}