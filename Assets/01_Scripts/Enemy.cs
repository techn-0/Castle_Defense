using System.Collections.Generic;
using UnityEngine;

public enum EnemyKind { Melee, Ranged }

// 근접/원거리 공통 스크립트. kind 하나로 벽 반응만 분기한다.
//   근접: 열린 이웃 셀이 있으면 그리로. 모두 막혔을 때만 앞의 벽 공격.
//   원거리: 이동 중에도 사거리 내 벽이 감지되면 멈춰서 그 벽을 공격.
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
        if (sr != null) sr.color = kind == EnemyKind.Ranged
            ? new Color(0.4f, 0.7f, 1f) : new Color(1f, 0.6f, 0.6f);

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
        if (grid.IsCastle(currentCell)) { Destroy(gameObject); return; }
        PickNext();
    }

    // 열린 이웃 중 성 방향으로 최단인 셀을 고른다. 모두 막혔으면 (첫 번째로 발견한) 벽을 공격 상태로 진입.
    // bestDist를 int.MaxValue가 아니라 현재 셀의 거리값으로 시작해야, 앞이 막혔을 때 온 길(뒤쪽,
    // 거리값이 더 큼)로 되돌아가며 앞뒤로 무한 진동하지 않고 제대로 "막힘" 상태로 판정된다.
    // 성으로 가는 길 자체가 완전히 끊겨 distToCastle이 도달 불가면, 대신 distToWall이 감소하는
    // 쪽으로 움직여 가장 가까운 벽을 찾아가 부순다 — 그렇지 않으면 봉쇄된 적이 영원히 얼어붙는다.
    void PickNext()
    {
        int castleDist = Pathfinder.I.GetDist(currentCell);
        bool useCastle = castleDist != int.MaxValue;

        Vector3Int? best = null;
        int bestDist = useCastle ? castleDist : Pathfinder.I.GetDistToWall(currentCell);
        Vector3Int? blockedFirst = null;

        foreach (var nb in grid.GetNeighbors4(currentCell))
        {
            if (!grid.IsWalkable(nb))
            {
                if (grid.IsOccupied(nb) && blockedFirst == null) blockedFirst = nb;
                continue;
            }
            int d = useCastle ? Pathfinder.I.GetDist(nb) : Pathfinder.I.GetDistToWall(nb);
            if (d < bestDist) { best = nb; bestDist = d; }
        }

        targetCell = best;
        if (best == null && blockedFirst.HasValue)
            Wall.ByCell.TryGetValue(blockedFirst.Value, out lockedWall);
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
