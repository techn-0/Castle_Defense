using UnityEngine;

// 로그라이크식 조작 캐릭터. 이번 범위에서는 HP/피격 없음(의도적 — 적은 성/벽/함정만 공격 대상).
// 그리드 점유(IsOccupied)와 무관하게 자유 이동한다 — 벽 위도 그대로 지나간다.
public class Player : MonoBehaviour
{
    public static Player I;

    public float moveSpeed = 4f;
    public float buildRange = 3.5f;
    public float attackRange = 4f;
    public int attackDamage = 1;
    public float attackInterval = 0.5f;

    public GameObject projectilePrefab;
    public float projectileSpeed = 8f;
    public int projectileCount = 1; // 차후 업그레이드 훅 — Economy로 구매 시 이 값만 늘리면 다중 발사 동작
    public float spreadAngle = 15f; // projectileCount > 1일 때 발사 간 각도 간격

    Enemy currentTarget;
    float attackTimer;

    BuildManager buildManager;
    LineRenderer rangeIndicator;
    const int RangeSegments = 36;

    // Enemy.cs와 동일한 SPUM 연동 패턴 — Player.prefab이 SPUM 리그(UnitRoot 자식)일 때만 동작하고,
    // 순정 스프라이트 프리팹이면 spum이 null이라 전부 조용히 무시된다.
    SPUM_Prefabs spum;
    Transform spumVisual;
    bool spumMoving;
    float facing = 1f;

    void Awake()
    {
        // BuildManager.CanPlace()가 이 오브젝트의 Start()보다 먼저 Player.I를 참조할 수 있으므로 Awake에서 배정.
        I = this;
        buildManager = FindFirstObjectByType<BuildManager>();
        BuildRangeIndicator();
    }

    void Start()
    {
        spum = GetComponent<SPUM_Prefabs>();
        if (spum != null)
        {
            spum.OverrideControllerInit();
            spum.PlayAnimation(PlayerState.IDLE, 0);
            spumVisual = transform.Find("UnitRoot");
        }
    }

    void OnDestroy()
    {
        if (I == this) I = null;
    }

    void Update()
    {
        if (!GameManager.I.IsPlaying) return;

        Move();
        AutoAttack();
        UpdateRangeIndicator();
    }

    void Move()
    {
        Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (move.sqrMagnitude > 1f) move.Normalize();
        transform.position += (Vector3)(move * moveSpeed * Time.deltaTime);

        SetSpumMoving(move.sqrMagnitude > 0.0001f);
        FaceDirection(move);
    }

    void SetSpumMoving(bool moving)
    {
        if (spum == null || moving == spumMoving) return;
        spumMoving = moving;
        spum.PlayAnimation(moving ? PlayerState.MOVE : PlayerState.IDLE, 0);
    }

    // 순수 상하 이동 중엔 직전 좌우 방향을 유지 — Enemy.FaceTowards와 동일 관례.
    void FaceDirection(Vector2 move)
    {
        if (spumVisual == null || Mathf.Abs(move.x) < 0.01f) return;
        float dir = move.x > 0f ? -1f : 1f;
        if (dir == facing) return;
        facing = dir;
        var s = spumVisual.localScale;
        spumVisual.localScale = new Vector3(Mathf.Abs(s.x) * facing, s.y, s.z);
    }

    void AutoAttack()
    {
        // 매 프레임 다시 탐색 — 적이 사거리를 벗어나거나 더 가까운 적이 나타나면 즉시 갈아탄다.
        currentTarget = FindNearestEnemy();

        attackTimer -= Time.deltaTime;
        if (currentTarget != null && attackTimer <= 0f)
        {
            Fire(currentTarget);
            attackTimer = attackInterval;
        }
    }

    // Enemy.FindWallInRange()와 동일한 패턴 — 정적 리스트를 sqrMagnitude로 순회해 사거리 내 최근접 대상을 찾는다.
    Enemy FindNearestEnemy()
    {
        Enemy nearest = null;
        float bestSqr = attackRange * attackRange;
        Vector2 me = transform.position;
        foreach (var e in Enemy.All)
        {
            float d = ((Vector2)e.transform.position - me).sqrMagnitude;
            if (d < bestSqr) { bestSqr = d; nearest = e; }
        }
        return nearest;
    }

    // projectileCount > 1은 구조만 마련해둠 — 투사체가 타겟을 계속 추적(homing)하는 방식이라
    // 각도를 줘도 결국 같은 지점에 수렴하므로, 실제 "부채꼴 발사"를 구현하려면 차후 업그레이드 때
    // Projectile을 고정 방향 직선 이동으로 바꿔야 한다. 지금은 같은 타겟에 N발을 겹쳐 쏘는 정도로만 동작.
    void Fire(Enemy target)
    {
        if (projectilePrefab == null) return;

        for (int i = 0; i < projectileCount; i++)
            SpawnProjectile(target);
    }

    void SpawnProjectile(Enemy target)
    {
        var go = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        var proj = go.GetComponent<Projectile>();
        proj.Init(target, projectileSpeed, attackDamage);
    }

    void BuildRangeIndicator()
    {
        var go = new GameObject("BuildRangeIndicator");
        go.transform.SetParent(transform, false);
        rangeIndicator = go.AddComponent<LineRenderer>();
        rangeIndicator.loop = true;
        rangeIndicator.useWorldSpace = false;
        rangeIndicator.positionCount = RangeSegments;
        rangeIndicator.startWidth = 0.05f;
        rangeIndicator.endWidth = 0.05f;
        rangeIndicator.material = new Material(Shader.Find("Sprites/Default"));
        rangeIndicator.startColor = new Color(1f, 1f, 1f, 0.5f);
        rangeIndicator.endColor = new Color(1f, 1f, 1f, 0.5f);

        for (int i = 0; i < RangeSegments; i++)
        {
            float angle = 2f * Mathf.PI * i / RangeSegments;
            rangeIndicator.SetPosition(i, new Vector3(Mathf.Cos(angle) * buildRange, Mathf.Sin(angle) * buildRange, 0f));
        }

        rangeIndicator.gameObject.SetActive(false);
    }

    void UpdateRangeIndicator()
    {
        rangeIndicator.gameObject.SetActive(buildManager != null && buildManager.IsBuildMode);
    }
}
