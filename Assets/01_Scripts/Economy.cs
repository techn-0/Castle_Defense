using UnityEngine;

public class Economy : MonoBehaviour
{
    public static Economy I;
    public int gold = 50;

    void Awake() => I = this;

    public void AddGold(int amount) => gold += amount;

    public bool TrySpend(int amount)
    {
        if (gold < amount) return false;
        gold -= amount;
        return true;
    }
}
