using System.Collections.Generic;
using UnityEngine;

// TileGrid 위에서 Castle 셀(들)로부터 BFS로 distToCastle을 계산한다.
// Map.ComputeDistances()와 같은 알고리즘이지만 그래프(Node.neighbors)가 아니라
// 그리드 셀(GetNeighbors4)을 순회한다. 벽이 놓이거나 파괴될 때 Recompute()를 호출하면
// 다음 프레임부터 Enemy가 우회 경로를 따라간다 (실제 배선은 Wall 쪽에서 한다).
public class Pathfinder : MonoBehaviour
{
    public static Pathfinder I;
    public TileGrid grid;
    public int wallCrossCost = 5;

    readonly Dictionary<Vector3Int, int> distToCastle = new();
    readonly Dictionary<Vector3Int, int> distThroughWalls = new();
    readonly Dictionary<Vector3Int, int> distIgnoringWalls = new();

    void Awake() { I = this; }
    void Start() { Recompute(); }

    public void Recompute()
    {
        distToCastle.Clear();
        var q = new Queue<Vector3Int>();
        foreach (var c in grid.GetCastleCells())
        {
            distToCastle[c] = 0;
            q.Enqueue(c);
        }

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            foreach (var nb in grid.GetNeighbors4(cur))
            {
                if (!grid.IsWalkable(nb) || distToCastle.ContainsKey(nb)) continue;
                distToCastle[nb] = distToCastle[cur] + 1;
                q.Enqueue(nb);
            }
        }

        RecomputeDistThroughWalls();
        RecomputeDistIgnoringWalls();
    }

    // 벽 점유 여부를 아예 무시하는 순수 BFS(가중치 없음) — 닌자 유닛이 벽을 뚫고 지나갈 때 쓴다.
    // distToCastle과 구조는 동일하고 필터만 IsWalkable 대신 HasTile(바닥 타일 존재만 확인).
    void RecomputeDistIgnoringWalls()
    {
        distIgnoringWalls.Clear();
        var q = new Queue<Vector3Int>();
        foreach (var c in grid.GetCastleCells())
        {
            distIgnoringWalls[c] = 0;
            q.Enqueue(c);
        }

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            foreach (var nb in grid.GetNeighbors4(cur))
            {
                if (!grid.HasTile(nb) || distIgnoringWalls.ContainsKey(nb)) continue;
                distIgnoringWalls[nb] = distIgnoringWalls[cur] + 1;
                q.Enqueue(nb);
            }
        }
    }

    // 성으로 가는 길이 완전히 막혀 distToCastle이 도달 불가(MaxValue)인 영역을 위한 보조 거리값.
    // 벽을 "통과 불가"가 아니라 "비용 wallCrossCost를 내면 통과 가능한 칸"으로 취급해
    // 성에서부터 가중치 최단경로를 계산한다 — 갇힌 적이 아무 벽이나 가까운 걸 부수는 게 아니라,
    // 부쉈을 때 실제로 성까지 가장 가까워지는 벽을 찾아가게 하기 위함. 그리드가 작아
    // 우선순위 큐 없이 매번 프론티어에서 최소값을 선형 탐색하는 단순 Dijkstra로 구현한다.
    void RecomputeDistThroughWalls()
    {
        distThroughWalls.Clear();
        var frontier = new List<Vector3Int>();

        foreach (var c in grid.GetCastleCells())
        {
            distThroughWalls[c] = 0;
            frontier.Add(c);
        }

        while (frontier.Count > 0)
        {
            int bestIdx = 0;
            for (int i = 1; i < frontier.Count; i++)
                if (distThroughWalls[frontier[i]] < distThroughWalls[frontier[bestIdx]]) bestIdx = i;

            var cur = frontier[bestIdx];
            frontier.RemoveAt(bestIdx);

            foreach (var nb in grid.GetNeighbors4(cur))
            {
                if (!grid.HasTile(nb)) continue;
                int stepCost = grid.IsOccupied(nb) ? wallCrossCost : 1;
                int nd = distThroughWalls[cur] + stepCost;
                if (!distThroughWalls.TryGetValue(nb, out var old) || nd < old)
                {
                    distThroughWalls[nb] = nd;
                    frontier.Add(nb);
                }
            }
        }
    }

    public int GetDist(Vector3Int cell) => distToCastle.TryGetValue(cell, out var d) ? d : int.MaxValue;
    public int GetDistThroughWalls(Vector3Int cell) => distThroughWalls.TryGetValue(cell, out var d) ? d : int.MaxValue;
    public int GetDistIgnoringWalls(Vector3Int cell) => distIgnoringWalls.TryGetValue(cell, out var d) ? d : int.MaxValue;

    // 검증용: 임의 셀의 점유 상태를 토글하고 재계산해 우회/도달불가 판정을 눈으로 확인한다.
    // 실제 클릭 배치는 Phase 3(BuildManager) 몫이라 이 필드/메서드는 그때 제거해도 된다.
    public Vector3Int debugToggleCell;

    [ContextMenu("Debug: Toggle Occupied & Recompute")]
    void DebugToggle()
    {
        grid.SetOccupied(debugToggleCell, !grid.IsOccupied(debugToggleCell));
        Recompute();
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (grid == null) return;
        foreach (var c in grid.AllCells())
        {
            int d = GetDist(c);
            if (d == int.MaxValue) continue;
            Gizmos.color = Color.Lerp(Color.green, Color.red, Mathf.Clamp01(d / 20f));
            Gizmos.DrawCube(grid.CellToWorld(c), Vector3.one * 0.6f);
            UnityEditor.Handles.Label(grid.CellToWorld(c), d.ToString());
        }
    }
#endif
}
