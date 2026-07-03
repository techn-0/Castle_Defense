using UnityEngine;

// 성 HP를 관리한다. isCastle=true인 Node와 같은 GameObject에 붙인다.
public class Castle : MonoBehaviour
{
    public static Castle I;
    public int hp = 10;

    void Awake() { I = this; }

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        Debug.Log($"[Castle] HP: {hp}");
        if (hp <= 0) Debug.Log("[Castle] GAME OVER");
    }
}
