using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyKind { Melee, Ranged, Ninja, Boss }

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

    public AudioClip hitSfx;
    public AudioClip deathSfx;
    SpriteRenderer sr;
    Color baseColor;
    Coroutine flashRoutine;
    HealthBar healthBar;

    SPUM_Prefabs spum;
    Transform spumVisual;
    bool spumMoving;
    float facing = 1f; // +1 = 오른쪽, -1 = 왼쪽. 순수 상하 이동일 땐 마지막 방향을 유지한다.

    public Vector3Int CurrentCell => currentCell;
    public Transform LuredBy => luredBy;
    Transform luredBy;
    float lureDwellDuration;
    float lureDwellRemaining = -1f; // -1 = 아직 함정에 도착 못함(체류 시작 전)
    readonly HashSet<Transform> visitedLures = new();

    int hp;
    TileGrid grid;
    Vector3Int currentCell;
    Vector3Int? targetCell;
    Wall lockedWall;
    float attackTimer;

    float slowMultiplier = 1f;
    float slowRemaining = 0f;

    // grid는 Awake에서 바로 채워둔다 — Start까지 미루면, 같은 프레임에 다른 오브젝트의
    // Update(예: FireTrap.Update가 Enemy.All을 순회)가 이 적의 Start보다 먼저 실행될 때
    // grid가 아직 null이라 NullReferenceException이 난다.
    void Awake()
    {
        All.Add(this);
        grid = Pathfinder.I.grid;
    }
    void OnDestroy() { All.Remove(this); }

    void Start()
    {
        currentCell = grid.WorldToCell(transform.position);
        transform.position = grid.CellToWorld(currentCell);
        hp = maxHp;

        healthBar = GetComponent<HealthBar>();
        if (healthBar == null) healthBar = gameObject.AddComponent<HealthBar>();

        // SPUM 캐릭터는 애니메이터가 클립을 자동으로 안 갈아끼워준다 — 프리팹에 이미 구워진
        // IDLE_List/MOVE_List를 바탕으로 오버라이드 컨트롤러를 한 번 만들어줘야 PlayAnimation()으로
        // Idle<->Move 전환이 가능해진다. 순정 Enemy.prefab처럼 SPUM_Prefabs가 없으면 그냥 무시.
        spum = GetComponent<SPUM_Prefabs>();
        if (spum != null)
        {
            spum.OverrideControllerInit();
            spum.PlayAnimation(PlayerState.IDLE, 0);
            spumVisual = transform.Find("UnitRoot");
        }

        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            baseColor = kind switch
            {
                EnemyKind.Ranged => new Color(0.4f, 0.7f, 1f),
                EnemyKind.Ninja => new Color(0.25f, 0.25f, 0.3f),
                EnemyKind.Boss => new Color(0.6f, 0.1f, 0.75f),
                _ => new Color(1f, 0.6f, 0.6f),
            };
            sr.color = baseColor;
        }

        PickNext();
    }

    void Update()
    {
        if (slowRemaining > 0f)
        {
            slowRemaining -= Time.deltaTime;
            if (slowRemaining <= 0f) slowMultiplier = 1f;
        }

        if (luredBy != null)
        {
            if (targetCell == null)
            {
                PickNextLureStep();
                if (targetCell == null)
                {
                    // 더 가까워질 이웃이 없다 = 함정에 도착. 체류 타이머를 여기서 자체적으로 돌린다 —
                    // 함정 쪽 트리거(OnTriggerStay2D)가 어떤 이유로 안 불리더라도(콜라이더 설정 등)
                    // 반드시 시간이 지나면 풀리도록, 해제를 함정에 의존하지 않고 Enemy가 직접 보장한다.
                    SetSpumMoving(false);
                    if (lureDwellRemaining < 0f) lureDwellRemaining = lureDwellDuration;
                    lureDwellRemaining -= Time.deltaTime;
                    if (lureDwellRemaining <= 0f) ReleaseLure();
                    return;
                }
            }
            SetSpumMoving(true);
            Vector3 lureDest = grid.CellToWorld(targetCell.Value);
            FaceTowards(lureDest);
            transform.position = Vector2.MoveTowards(transform.position, lureDest, speed * slowMultiplier * Time.deltaTime);
            if (((Vector2)transform.position - (Vector2)lureDest).sqrMagnitude < 0.0001f)
            {
                currentCell = targetCell.Value;
                targetCell = null;
            }
            return;
        }

        if (kind == EnemyKind.Ranged && lockedWall == null)
            lockedWall = FindWallInRange();

        if (lockedWall != null)
        {
            if (lockedWall.hp <= 0) { lockedWall = null; PickNext(); return; }
            SetSpumMoving(false);
            FaceTowards(lockedWall.transform.position);
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                int thorn = lockedWall.thornDamage;
                lockedWall.TakeDamage(wallDamage);
                attackTimer = attackInterval;
                if (spum != null) spum.PlayAnimation(PlayerState.ATTACK, 0);
                if (UpgradeManager.I != null && UpgradeManager.I.WallThornUnlocked)
                    TakeDamage(thorn);
            }
            return;
        }

        // Pathfinder.Recompute()가 Start()에서 도는데, 오브젝트 간 Start() 순서는 보장되지 않는다.
        // 이 Enemy의 Start()가 먼저 돌면 첫 PickNext()가 빈 거리값을 보고 실패할 수 있으니 매 프레임 재시도한다.
        if (targetCell == null) { PickNext(); if (targetCell == null) return; }

        // 이동 중(아직 도착 전)에 목표 칸에 새 벽이 놓이면, 그 칸을 향해 계속 걸어가
        // 벽을 뚫고 지나가 버린다. 닌자는 원래 벽을 무시하므로 예외로 둔다.
        if (kind != EnemyKind.Ninja && grid.IsOccupied(targetCell.Value))
        {
            targetCell = null;
            PickNext();
            if (targetCell == null) return;
        }

        SetSpumMoving(true);
        Vector3 dest = grid.CellToWorld(targetCell.Value);
        FaceTowards(dest);
        transform.position = Vector2.MoveTowards(transform.position, dest, speed * slowMultiplier * Time.deltaTime);
        if (((Vector2)transform.position - (Vector2)dest).sqrMagnitude < 0.0001f) OnArrive();
    }

    void SetSpumMoving(bool moving)
    {
        if (spum == null || moving == spumMoving) return;
        spumMoving = moving;
        spum.PlayAnimation(moving ? PlayerState.MOVE : PlayerState.IDLE, 0);
    }

    // 목적지가 좌/우로 뚜렷이 갈릴 때만 방향을 바꾼다 — 순수 상하 이동 중엔 직전 좌우 방향을 유지.
    void FaceTowards(Vector3 destWorld)
    {
        if (spumVisual == null) return;
        float dx = destWorld.x - transform.position.x;
        if (Mathf.Abs(dx) < 0.01f) return;
        float dir = dx > 0f ? -1f : 1f;
        if (dir == facing) return;
        facing = dir;
        var s = spumVisual.localScale;
        spumVisual.localScale = new Vector3(Mathf.Abs(s.x) * facing, s.y, s.z);
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

    // 유인 이동도 그리드 칸 단위로 진행한다 — 벽 점유는 무시하지만(닌자와 동일하게 HasTile만
    // 확인) 실제 타일이 없는 칸(맵 바깥/틈)까지 가로지르지는 않는다. 매 칸 도착마다 유인 대상
    // 쪽으로 더 가까워지는 이웃을 다시 고르는 단순 그리디 방식이라, 아주 복잡한 지형에서는
    // 최적 경로가 아닐 수 있지만 함정은 보통 기존 통로 근처에 놓이므로 충분하다.
    void PickNextLureStep()
    {
        Vector3 destWorld = luredBy.position;
        Vector3Int? best = null;
        float bestSqr = ((Vector2)grid.CellToWorld(currentCell) - (Vector2)destWorld).sqrMagnitude;
        foreach (var nb in grid.GetNeighbors4(currentCell))
        {
            if (!grid.HasTile(nb)) continue;
            float d = ((Vector2)grid.CellToWorld(nb) - (Vector2)destWorld).sqrMagnitude;
            if (d < bestSqr) { best = nb; bestSqr = d; }
        }
        targetCell = best;
    }

    // 유인형 함정(FireTrap 등)이 유인을 걸 때 이 메서드를 통해서만 호출한다 — luredBy를
    // 직접 대입하지 않고 상태(현재 셀, 이번 이동 목표, 방문 기록)를 함께 정리해준다.
    // visitedLures에 한 번 추가된 함정은 이후 다시는 그 개체를 유인 대상으로 보지 않는다
    // (같은 함정에 반복해서 잡혀가는 것을 원천 차단).
    public void GetLured(Transform lureSource, float dwellDuration)
    {
        luredBy = lureSource;
        lureDwellDuration = dwellDuration;
        lureDwellRemaining = -1f;
        visitedLures.Add(lureSource);
        currentCell = grid.WorldToCell(transform.position);
        targetCell = null;
    }

    public bool HasVisitedLure(Transform lureSource) => visitedLures.Contains(lureSource);

    // 유인 중엔 그리드 밖 임의 위치로 이동했을 수 있으므로 Start()와 같은 방식으로 셀에 스냅한 뒤
    // 경로찾기를 재개한다. 함정 쪽(FireTrap 등)에서 체류 시간이 다 됐을 때 호출한다.
    public void ReleaseLure()
    {
        luredBy = null;
        lureDwellRemaining = -1f;
        lockedWall = null; // 유인 전에 걸려 있던 벽 공격 상태가 있었다면 깨끗이 지우고 재개
        currentCell = grid.WorldToCell(transform.position);
        transform.position = grid.CellToWorld(currentCell);
        targetCell = null;
        PickNext();
    }

    // 더 강한 슬로우가 겹치면 그쪽을 우선하고(Min), 타이머는 항상 더 긴 쪽으로 갱신한다(Max) —
    // 여러 함정에 연달아 밟혀도 효과가 서로 깎아먹지 않도록.
    public void ApplySlow(float multiplier, float duration)
    {
        slowMultiplier = Mathf.Min(slowMultiplier, multiplier);
        slowRemaining = Mathf.Max(slowRemaining, duration);
    }

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        if (hp <= 0)
        {
            if (deathSfx != null) AudioSource.PlayClipAtPoint(deathSfx, transform.position);
            EffectsUtil.SpawnBurst(transform.position, baseColor);
            Economy.I.AddGold(goldReward);
            Destroy(gameObject);
            return;
        }
        if (hitSfx != null) AudioSource.PlayClipAtPoint(hitSfx, transform.position);
        if (sr != null)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine());
        }
        healthBar.SetFraction((float)hp / maxHp);
    }

    IEnumerator FlashRoutine()
    {
        sr.color = Color.white;
        yield return new WaitForSeconds(0.08f);
        sr.color = baseColor;
        flashRoutine = null;
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
