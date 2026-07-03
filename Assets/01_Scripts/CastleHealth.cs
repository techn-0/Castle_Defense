using UnityEngine;

public class CastleHealth : MonoBehaviour
{
    public static CastleHealth I;
    public int maxHp = 20;
    int hp;

    void Awake() { I = this; hp = maxHp; }
    public int Hp => hp;

    public void TakeDamage(int dmg)
    {
        if (hp <= 0) return;
        hp -= dmg;
        if (hp <= 0) GameManager.I.GameOver();
    }
}
