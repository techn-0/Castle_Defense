using System.Collections.Generic;
using UnityEngine;

public enum EnemyKind { Melee, Ranged, Ninja }

// 근접/원거리/닌자 공통 스크립트. kind 하나로 벽 반응만 분기한다.
//   근접: 열린 이웃 셀이 있으면 그리로. 모두 막혔을 때만 앞의 벽 공격.
//   원거리: 이동 중에도 사거리 내 벽이 감지되면 멈춰서 그 벽을 공격.
//   닌자: 벽 점유를 아예 무시하고 최단 경로로 그대로 통과.
public class Enemy : MonoBehaviour
{
    // Wall.All과 동일한 패턴 — WaveManager가 "이번 웨이브 적이 다 사라졌는지" 판정하는 데 쓴다.
    public static readonly List<Enemy> All = new();

    public EnemyKind kind = EnemyKind.Melee;
    public float speed = 2f;
    public int damage = 1;
    public int maxHp = 3;
    public int goldReward = 2;

    public int wallDamage = 1;
    public float attackInterval = 0.6f;
    public float rangedScanRadius = 2.5f;

    int hp;
    TileGrid grid;
    Vector3Int currentCell;
    Vector3Int? targetCell;
    Wall lockedWall;
    float attackTimer;

    void Awake() { All.Add(this); }
    void OnDestroy() { All.Remove(this); }

    void Start()
    {
        grid = Pathfinder.I.grid;
        currentCell = grid.WorldToCell(transform.position);
        transform.position = grid.CellToWorld(currentCell);
        hp = maxHp;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = kind switch
        {
            EnemyKind.Ranged => new Color(0.4f, 0.7f, 1f),
            EnemyKind.Ninja => new Color(0.25f, 0.25f, 0.3f),
            _ => new Color(1f, 0.6f, 0.6f),
        };

        PickNext();
    }

    void Update()
    {
        if (kind == EnemyKind.Ranged && lockedWall == null)
            lockedWall = FindWallInRange();

        if (lockedWall != null)
        {
            if (lockedWall.hp <= 0) { lockedWall = null; PickNext(); return; }
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f) { lockedWall.TakeDamage(wallDamage); attackTimer = attackInterval; }
            return;
        }

        // Pathfinder.Recompute()가 Start()에서 도는데, 오브젝트 간 Start() 순서는 보장되지 않는다.
        // 이 Enemy의 Start()가 먼저 돌면 첫 PickNext()가 빈 거리값을 보고 실패할 수 있으니 매 프레임 재시도한다.
        if (targetCell == null) { PickNext(); if (targetCell == null) return; }
        Vector3 dest = grid.CellToWorld(targetCell.Value);
        transform.position = Vector2.MoveTowards(transform.position, dest, speed * Time.deltaTime);
        if (((Vector2)transform.position - (Vector2)dest).sqrMagnitude < 0.0001f) OnArrive();
    }

    void OnArrive()
    {
        currentCell = targetCell.Value;
        if (grid.IsCastle(currentCell))
        {
            CastleHealth.I.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
        PickNext();
    }

    // 성으로 가는 길이 열려 있으면(distToCastle 도달 가능) 열린 이웃 중 최단인 셀로 이동한다 —
    // BFS 불변식상 이 경우 항상 더 가까운 열린 이웃이 존재하므로 벽 공격으로 빠질 일이 없다.
    // 완전히 막혀 있으면 벽을 통과 가능한 비용 칸으로 보는 distThroughWalls를 대신 따라간다:
    // 아직 그 값이 줄어드는 열린 이웃이 있으면 그리로 걷고, 없으면(=사실상 봉쇄) 막힌 이웃 중
    // distThroughWalls가 가장 작은(=부쉈을 때 성까지 가장 가까워지는) 벽을 공격 상태로 잠근다.
    void PickNext()
    {
        if (kind == EnemyKind.Ninja)
        {
            Vector3Int? nextCell = null;
            int nextDist = Pathfinder.I.GetDistIgnoringWalls(currentCell);
            foreach (var nb in grid.GetNeighbors4(currentCell))
            {
                if (!grid.HasTile(nb)) continue;
                int d = Pathfinder.I.GetDistIgnoringWalls(nb);
                if (d < nextDist) { nextCell = nb; nextDist = d; }
            }
            targetCell = nextCell;
            return;
        }

        int castleDist = Pathfinder.I.GetDist(currentCell);
        bool useCastle = castleDist != int.MaxValue;

        if (useCastle)
        {
            Vector3Int? best = null;
            int bestDist = castleDist;
            foreach (var nb in grid.GetNeighbors4(currentCell))
            {
                if (!grid.IsWalkable(nb)) continue;
                int d = Pathfinder.I.GetDist(nb);
                if (d < bestDist) { best = nb; bestDist = d; }
            }
            targetCell = best;
            return;
        }

        Vector3Int? bestOpen = null;
        int bestThrough = Pathfinder.I.GetDistThroughWalls(currentCell);
        Vector3Int? bestWall = null;
        int bestWallThrough = int.MaxValue;

        foreach (var nb in grid.GetNeighbors4(currentCell))
        {
            if (grid.IsWalkable(nb))
            {
                int d = Pathfinder.I.GetDistThroughWalls(nb);
                if (d < bestThrough) { bestOpen = nb; bestThrough = d; }
            }
            else if (grid.IsOccupied(nb))
            {
                int d = Pathfinder.I.GetDistThroughWalls(nb);
                if (d < bestWallThrough) { bestWall = nb; bestWallThrough = d; }
            }
        }

        targetCell = bestOpen;
        if (bestOpen == null && bestWall.HasValue)
            Wall.ByCell.TryGetValue(bestWall.Value, out lockedWall);
    }

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        if (hp <= 0)
        {
            Economy.I.AddGold(goldReward);
            Destroy(gameObject);
        }
    }

    Wall FindWallInRange()
    {
        Wall nearest = null;
        float bestSqr = rangedScanRadius * rangedScanRadius;
        Vector2 me = transform.position;
        foreach (var w in Wall.All)
        {
            float d = ((Vector2)w.transform.position - me).sqrMagnitude;
            if (d < bestSqr) { bestSqr = d; nearest = w; }
        }
        return nearest;
    }
}
