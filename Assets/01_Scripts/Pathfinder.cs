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

    readonly Dictionary<Vector3Int, int> distToCastle = new();
    readonly Dictionary<Vector3Int, int> distToWall = new();

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

        RecomputeDistToWall();
    }

    // 성으로 가는 길이 완전히 막혀 distToCastle이 도달 불가(MaxValue)인 영역을 위한 보조 거리값.
    // 벽에 인접한 걸을 수 있는 칸을 거리 0으로 시드해 바깥으로 BFS한다 — 갇힌 적이
    // "가장 가까운 벽" 방향으로 이동해 부수러 갈 수 있게 한다.
    void RecomputeDistToWall()
    {
        distToWall.Clear();
        var q = new Queue<Vector3Int>();
        foreach (var wall in Wall.All)
        {
            foreach (var nb in grid.GetNeighbors4(wall.Cell))
            {
                if (!grid.IsWalkable(nb) || distToWall.ContainsKey(nb)) continue;
                distToWall[nb] = 0;
                q.Enqueue(nb);
            }
        }

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            foreach (var nb in grid.GetNeighbors4(cur))
            {
                if (!grid.IsWalkable(nb) || distToWall.ContainsKey(nb)) continue;
                distToWall[nb] = distToWall[cur] + 1;
                q.Enqueue(nb);
            }
        }
    }

    public int GetDist(Vector3Int cell) => distToCastle.TryGetValue(cell, out var d) ? d : int.MaxValue;
    public int GetDistToWall(Vector3Int cell) => distToWall.TryGetValue(cell, out var d) ? d : int.MaxValue;

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
