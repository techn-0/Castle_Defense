using UnityEngine;

public class Economy : MonoBehaviour
{
    public static Economy I;
    public int gold = 50;
    public float goldMultiplier = 1f; // 강화: 골드 획득 증가

    void Awake() => I = this;

    public void AddGold(int amount) => gold += Mathf.RoundToInt(amount * goldMultiplier);

    public bool TrySpend(int amount)
    {
        if (gold < amount) return false;
        gold -= amount;
        return true;
    }
}
