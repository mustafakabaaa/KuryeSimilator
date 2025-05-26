using UnityEngine;
using System;

public class MotorEventManager : MonoBehaviour
{
    public static MotorEventManager Instance;

    public event Action<MotorcycleVehicle> OnMotorcycleMounted;
    public event Action OnMotorcycleDismounted;

    // MotorEventManager.cs
    private void Awake()
    {
        Debug.Log("MotorEventManager Awake called!"); // Bu log görünmeli
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("MotorEventManager initialized!", this);
        }
    }

    public void TriggerMountEvent(MotorcycleVehicle motor)
    {
        OnMotorcycleMounted?.Invoke(motor);
    }

    public void TriggerDismountEvent()
    {
        OnMotorcycleDismounted?.Invoke();
    }
}