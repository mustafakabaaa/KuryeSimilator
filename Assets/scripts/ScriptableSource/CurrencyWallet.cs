using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CurrencyWallet", menuName = "SC/Wallet")]
public class CurrencyWallet : ScriptableObject
{
    public int balance = 0;

    public event Action OnBalanceChanged;

    public void Add(int amount)
    {
        balance += amount;
        OnBalanceChanged?.Invoke();
    }

    public bool Spend(int amount)
    {
        if (balance < amount) return false;
        balance -= amount;
        OnBalanceChanged?.Invoke();
        return true;
    }
}