using System.Collections.Generic;
using UnityEngine;

public enum EnemyKind { Melee, Ranged, Ninja, Boss }

// 근접/원거리/닌자 공통 스크립트. kind 하나로 벽 반응만 분기한다.
//   근접: 열린 이웃 셀이 있으면 그리로. 모두 막혔을 때만 앞의 벽 공격.
//   원거리: 이동 중에도 사거리 내 벽이 감지되면 멈추지 않고 계속 이동하면서 그 벽을 공격.
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

    // 원거리 전용 공격 이펙트 — 데미지는 TakeDamage 호출 시점에 이미 즉시 적용되므로
    // 이 투사체는 순수 시각효과(EnemyProjectile)로, 목표까지 날아가면 스스로 파괴된다.
    public GameObject projectilePrefab;
    public float projectileSpeed = 8f;

    public AudioClip hitSfx;
    public AudioClip deathSfx;
    SpriteRenderer sr;
    Color baseColor;
    HealthBar healthBar;
    HitFlash hitFlash;

    // 슬로우 중엔 초록빛으로 물든다(Spike.SlowTintColor와 동일 색 — 슬로우 강화가 해금된
    // 가시함정 표시와 시각 언어를 통일). SPUM 캐릭터는 스프라이트가 여러 부위로 쪼개져 있어
    // 부위별 원래 색을 곱연산으로 유지한 채 틴트만 입힌다(HealthBar 스프라이트는 spumVisual
    // 하위가 아니라 루트에 붙으므로 자연히 제외된다).
    readonly List<(SpriteRenderer sr, Color baseColor)> visualRenderers = new();
    bool slowTinted;

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
    // 유인 이동 전용 BFS 거리장 — 함정 칸을 0으로 두고 실제로 걸어갈 수 있는 경로만 따라 채운다.
    // 단순 유클리드 거리 최솟값으로 다음 칸을 고르면 벽에 막힌 통로에서 "더 가까워질 이웃이 없다"가
    // 되어(실제로는 닿지 않았는데도) 도착 판정을 내려버리는 문제가 있어 GetLured 시점에 한 번 계산한다.
    readonly Dictionary<Vector3Int, int> lureDist = new();

    int hp;
    TileGrid grid;
    Vector3Int currentCell;
    Vector3Int? targetCell;
    Wall lockedWall;
    float attackTimer;

    float slowMultiplier = 1f;
    float slowRemaining = 0f;

    public GameObject burnEffectPrefab;
    public Vector3 burnEffectOffset = Vector3.zero;
    GameObject burnEffectInstance;

    int burnDamage;
    float burnTickInterval;
    float burnTickTimer;
    float burnRemaining = 0f;

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

        baseColor = kind switch
        {
            EnemyKind.Ranged => new Color(0.4f, 0.7f, 1f),
            EnemyKind.Ninja => new Color(0.25f, 0.25f, 0.3f),
            EnemyKind.Boss => new Color(0.6f, 0.1f, 0.75f),
            _ => new Color(1f, 0.6f, 0.6f),
        };
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = baseColor;

        if (spumVisual != null)
        {
            foreach (var r in spumVisual.GetComponentsInChildren<SpriteRenderer>(true))
                visualRenderers.Add((r, r.color));
        }
        else if (sr != null)
        {
            visualRenderers.Add((sr, sr.color));
        }

        // HitFlash는 UnitRoot(SPUM 스프라이트 전부) 하위에 붙여서, HealthBar가 루트에 직접
        // 만들어 붙이는 체력바 스프라이트(HPBar_BG/Fill)는 점멸/펀치 대상에서 자연히 제외한다.
        var flashHost = spumVisual != null ? spumVisual.gameObject : gameObject;
        hitFlash = flashHost.GetComponent<HitFlash>();
        if (hitFlash == null) hitFlash = flashHost.AddComponent<HitFlash>();

        PickNext();
    }

    void Update()
    {
        if (slowRemaining > 0f)
        {
            slowRemaining -= Time.deltaTime;
            if (slowRemaining <= 0f) slowMultiplier = 1f;
        }
        UpdateSlowTint();

        if (burnRemaining > 0f)
        {
            burnRemaining -= Time.deltaTime;
            burnTickTimer -= Time.deltaTime;
            if (burnTickTimer <= 0f)
            {
                TakeDamage(burnDamage);
                if (hp <= 0) return; // 이번 프레임에 죽었으면 이후 이동/공격 로직을 더 돌릴 필요 없음
                burnTickTimer = burnTickInterval;
            }
            if (burnRemaining <= 0f) ClearBurnEffect();
        }

        if (luredBy != null)
        {
            // 이동 중 목표 칸에 새 벽이 세워지면 무효화 — 일반 이동(#30 버그 수정)과 동일한 이유로,
            // 유인 이동도 매 프레임 재검사해야 한다(닌자는 원래 벽을 무시하므로 예외).
            if (targetCell != null && kind != EnemyKind.Ninja && grid.IsOccupied(targetCell.Value))
                targetCell = null;

            if (targetCell == null)
            {
                PickNextLureStep();
                if (targetCell == null)
                {
                    bool actuallyAtTrap = lureDist.TryGetValue(currentCell, out var d) && d == 0;
                    if (!actuallyAtTrap)
                    {
                        // 벽 등에 막혀 더는 다가갈 수 없을 뿐, 실제로 함정에 닿은 게 아니다 —
                        // 여기서 도착 판정을 내리면 닿지 않았는데 체류만 채우고 풀려나는 버그가 된다.
                        // 즉시 유인을 포기하고 원래 이동으로 복귀한다.
                        ReleaseLure();
                        return;
                    }
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

        if (kind == EnemyKind.Ranged)
        {
            // 원거리는 잠긴 벽이 죽었거나 사거리를 벗어나면 즉시 풀어준다 — 이동을 멈추지 않으므로
            // 계속 멀어질 수 있고, 그 경우 더는 공격 대상이 아니다.
            if (lockedWall != null && (lockedWall.hp <= 0 || !InRangedScanRadius(lockedWall)))
                lockedWall = null;
            if (lockedWall == null)
                lockedWall = FindWallInRange();

            if (lockedWall != null)
            {
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0f)
                {
                    int thorn = lockedWall.thornDamage;
                    lockedWall.TakeDamage(wallDamage);
                    attackTimer = attackInterval;
                    SpawnProjectile(lockedWall.transform.position);
                    if (spum != null)
                    {
                        spum.PlayAnimation(PlayerState.ATTACK, 0);
                        // PlayAnimation(ATTACK)이 "1_Move" 애니메이터 bool을 false로 꺼버린다.
                        // 이동 중엔 계속 걷고 있으므로, spumMoving 캐시를 무효화해서 아래 이동 로직의
                        // SetSpumMoving(true) 호출이 다시 걸리도록(=Move bool을 true로 복원) 만든다.
                        // 그렇지 않으면 공격 애니메이션이 끝난 뒤에도 Idle 자세로 그냥 걸어가 버린다.
                        spumMoving = false;
                    }
                    if (UpgradeManager.I != null && UpgradeManager.I.WallThornUnlocked)
                        TakeDamage(thorn);
                    if (hp <= 0) return;
                }
            }
            // 정지하지 않고 아래 일반 이동 로직으로 이어서 진행한다.
        }
        else if (lockedWall != null)
        {
            // 근접/닌자/보스: 사방이 막혔을 때만 벽을 잠그고, 그 자리에 멈춰서 공격한다.
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

    // 유인 이동도 그리드 칸 단위로 진행한다 — 일반 이동과 마찬가지로 벽 점유(IsOccupied)를
    // 존중해 벽을 뚫고 지나가지 않는다(닌자는 원래 벽을 무시하는 설계라 예외). lureDist(BFS 거리장)에서
    // 더 작은 값을 가진 이웃으로만 이동한다 — 단순 유클리드 거리였다면 벽으로 굽은 통로에서
    // "더 가까워질 이웃이 없다"는 잘못된 판단(실제로는 도착 전인데 도착으로 오인)이 나올 수 있었다.
    void PickNextLureStep()
    {
        int curDist = lureDist.TryGetValue(currentCell, out var cd) ? cd : int.MaxValue;
        Vector3Int? best = null;
        int bestDist = curDist;
        foreach (var nb in grid.GetNeighbors4(currentCell))
        {
            if (kind != EnemyKind.Ninja && grid.IsOccupied(nb)) continue;
            if (!lureDist.TryGetValue(nb, out var d)) continue;
            if (d < bestDist) { best = nb; bestDist = d; }
        }
        targetCell = best;
    }

    // 함정 칸을 거리 0으로 두고 실제로 걸어서 갈 수 있는 칸만 BFS로 채운다(닌자는 벽을 무시하므로
    // HasTile만, 나머지는 IsWalkable로 벽 점유 칸을 제외). GetLured 시점에 한 번만 계산해서 쓴다.
    void ComputeLureDistances(Vector3 lureWorldPos)
    {
        lureDist.Clear();
        var destCell = grid.WorldToCell(lureWorldPos);
        var q = new Queue<Vector3Int>();
        lureDist[destCell] = 0;
        q.Enqueue(destCell);
        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            foreach (var nb in grid.GetNeighbors4(cur))
            {
                bool passable = kind == EnemyKind.Ninja ? grid.HasTile(nb) : grid.IsWalkable(nb);
                if (!passable || lureDist.ContainsKey(nb)) continue;
                lureDist[nb] = lureDist[cur] + 1;
                q.Enqueue(nb);
            }
        }
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
        ComputeLureDistances(lureSource.position);
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

    void UpdateSlowTint()
    {
        bool slowActive = slowRemaining > 0f;
        if (slowActive == slowTinted) return;
        slowTinted = slowActive;

        Color mul = slowActive ? Spike.SlowTintColor : Color.white;
        foreach (var (r, baseC) in visualRenderers)
        {
            if (r == null) continue;
            r.color = new Color(baseC.r * mul.r, baseC.g * mul.g, baseC.b * mul.b, baseC.a);
        }
    }

    // 화염 함정 접촉이 끝나도(트랩을 벗어나도) 남은 시간 동안 독자적으로 도트 데미지를 계속 준다.
    // 재적용 시 데미지/주기는 최신 값으로 덮어쓰고, 남은 시간은 더 긴 쪽으로 갱신한다(ApplySlow와 동일 관례).
    public void ApplyBurn(int tickDamage, float tickInterval, float duration)
    {
        burnDamage = tickDamage;
        burnTickInterval = tickInterval;
        if (burnRemaining <= 0f) burnTickTimer = tickInterval;
        burnRemaining = Mathf.Max(burnRemaining, duration);

        // 이미 불이 붙어 있으면 이펙트를 새로 만들지 않고 지속 시간만 갱신한다.
        if (burnEffectInstance == null && burnEffectPrefab != null)
            burnEffectInstance = Instantiate(burnEffectPrefab, transform.position + burnEffectOffset, Quaternion.identity, transform);
    }

    // 이 적의 자식으로 붙여뒀으므로(Instantiate의 parent 인자) 적이 죽어 Destroy될 때는
    // 유니티가 자동으로 함께 파괴해준다 — 여기서는 지속 시간이 다 됐을 때만 직접 정리한다.
    void ClearBurnEffect()
    {
        if (burnEffectInstance != null) Destroy(burnEffectInstance);
        burnEffectInstance = null;
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
        if (hitFlash != null) hitFlash.Flash();
        healthBar.SetFraction((float)hp / maxHp);
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

    void SpawnProjectile(Vector3 target)
    {
        if (projectilePrefab == null) return;
        var go = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        // Player의 Projectile.prefab을 그대로 재사용해서 연결한 경우, 물리 충돌 판정용
        // Projectile 컴포넌트/Collider2D가 함께 따라온다. 이 발사체는 스폰 위치가 곧 이 적
        // 자신의 위치라 스폰되자마자 자기 자신의 콜라이더와 겹쳐 즉시 트리거가 발동 →
        // TakeDamage(0) 호출 후 그대로 파괴돼 화살이 보이지도 않고 사라지는 버그가 있었다.
        // 이 발사체는 순수 시각효과이므로 그 판정 부품들을 걷어내고 EnemyProjectile만 남긴다.
        var hitDetector = go.GetComponent<Projectile>();
        if (hitDetector != null) Destroy(hitDetector);
        var col = go.GetComponent<Collider2D>();
        if (col != null) Destroy(col);

        var proj = go.GetComponent<EnemyProjectile>();
        if (proj == null) proj = go.AddComponent<EnemyProjectile>();
        proj.Init(target, projectileSpeed);
    }

    bool InRangedScanRadius(Wall w)
    {
        float d = ((Vector2)w.transform.position - (Vector2)transform.position).sqrMagnitude;
        return d <= rangedScanRadius * rangedScanRadius;
    }
}
