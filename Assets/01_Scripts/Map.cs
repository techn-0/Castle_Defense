using System.Collections.Generic;
using UnityEngine;

// 씬의 모든 노드를 모으고, 성으로부터 BFS로 각 노드의 distToCastle을 계산한다.
// 적은 이 거리 값만 보고 다음 노드를 결정하므로 A* 등이 필요 없다.
public class Map : MonoBehaviour
{
    public static Map I;

    public Node[] AllNodes { get; private set; }
    public Node Castle { get; private set; }

    void Awake()
    {
        I = this;
        AllNodes = FindObjectsByType<Node>(FindObjectsSortMode.None);
        Castle = System.Array.Find(AllNodes, n => n.isCastle);
        if (Castle == null) { Debug.LogError("[Map] 성 노드(isCastle=true)가 없다."); return; }
        ComputeDistances();
    }

    void ComputeDistances()
    {
        var q = new Queue<Node>();
        Castle.distToCastle = 0;
        q.Enqueue(Castle);
        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            foreach (var nb in cur.neighbors)
            {
                if (nb == null || nb.distToCastle != int.MaxValue) continue;
                nb.distToCastle = cur.distToCastle + 1;
                q.Enqueue(nb);
            }
        }
    }
}
