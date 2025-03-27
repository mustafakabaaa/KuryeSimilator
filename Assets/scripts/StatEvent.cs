using System;

public static class StatEvents
{
    public static event Action<CharacterStat, float> OnStatUpdated;

    public static void TriggerStatUpdate(CharacterStat stat, float newValue)
    {
        OnStatUpdated?.Invoke(stat, newValue);
    }
}