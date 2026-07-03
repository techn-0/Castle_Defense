using UnityEngine;

// Day 1: 벽 없음. 매 노드 도착 시 distToCastle이 가장 작은 이웃으로 이동.
// Day 2에 IsBlocked 필터가 PickNext에 한 줄 추가되면 근접 유닛의 회피 로직이 된다.
public class Enemy : MonoBehaviour
{
    public float speed = 2f;
    public int damage = 1;

    // Spawner가 스폰 직후 채워 넣는 시작 노드.
    public Node currentNode;

    Node target;

    void Start()
    {
        transform.position = currentNode.Pos;
        PickNext();
    }

    void Update()
    {
        if (target == null) return;
        transform.position = Vector2.MoveTowards(
            transform.position, target.Pos, speed * Time.deltaTime);

        if ((Vector2)transform.position == target.Pos) OnArrive();
    }

    void OnArrive()
    {
        currentNode = target;
        if (currentNode.isCastle)
        {
            Castle.I.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
        PickNext();
    }

    void PickNext()
    {
        Node best = null;
        int bestDist = int.MaxValue;
        foreach (var n in currentNode.neighbors)
        {
            if (n == null) continue;
            if (n.distToCastle < bestDist) { best = n; bestDist = n.distToCastle; }
        }
        target = best;
    }
}
