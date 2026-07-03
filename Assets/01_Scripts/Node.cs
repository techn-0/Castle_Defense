using System.Collections.Generic;
using UnityEngine;

// 경로 그래프의 노드. Inspector에서 neighbors 리스트로 인접 노드를 연결한다.
// 성인 노드는 isCastle = true.
public class Node : MonoBehaviour
{
    public List<Node> neighbors = new();
    public bool isCastle;

    // Map이 성으로부터 BFS로 채워 넣는 값. 적은 이 값이 작은 이웃으로 이동한다.
    [HideInInspector] public int distToCastle = int.MaxValue;

    public Vector2 Pos => transform.position;

    void OnDrawGizmos()
    {
        Gizmos.color = isCastle ? Color.yellow : Color.white;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        Gizmos.color = new Color(0.4f, 0.4f, 0.4f);
        if (neighbors == null) return;
        foreach (var n in neighbors)
            if (n != null) Gizmos.DrawLine(transform.position, n.transform.position);
    }
}
