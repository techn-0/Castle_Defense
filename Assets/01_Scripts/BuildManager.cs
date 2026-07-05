using System.Collections.Generic;
using UnityEngine;

public enum BuildMode { None, Wall, Spike, FireTrap, ExplosiveTrap, Demolish }

public class BuildManager : MonoBehaviour
{
    public static BuildManager I;

    public GameObject wallPrefab;
    public GameObject spikePrefab;
    public GameObject fireTrapPrefab;
    public GameObject explosiveTrapPrefab;
    public Transform placedParent;
    public Camera cam;
    public int wallCost = 10;
    public int spikeCost = 15;
    public int fireTrapCost = 20;
    public int explosiveTrapCost = 25;
    public float costMultiplier = 1f; // 강화: 건설 비용 할인

    public static readonly Color LureRangeColor = new Color(1f, 0.55f, 0.1f, 0.5f);
    public static readonly Color ExplosionRangeColor = new Color(1f, 0.2f, 0.1f, 0.5f);

    BuildMode mode = BuildMode.None;
    TileGrid grid;

    Transform previewRoot;
    readonly List<(SpriteRenderer sr, Color baseColor)> previewSprites = new();

    GameObject demolishHighlightTarget;
    readonly List<(SpriteRenderer sr, Color baseColor)> demolishHighlightSprites = new();

    void Awake()
    {
        I = this;
        grid = FindFirstObjectByType<TileGrid>();
        if (cam == null) cam = Camera.main;

        previewRoot = new GameObject("BuildPreview").transform;
    }

    void OnDestroy()
    {
        if (I == this) I = null;
    }

    // PauseManager가 LateUpdate에서 확인해, 건축 모드 취소와 일시정지 패널이 같은 Escape
    // 입력에 동시에 반응하지 않도록 한다.
    public bool ConsumedEscape { get; private set; }

    void Update()
    {
        ConsumedEscape = false;

        // Time.timeScale == 0f: 웨이브 클리어 후 강화 선택 패널이 떠 있는 동안(UpgradeChoiceManager) —
        // GameManager.IsPlaying은 여전히 true이므로 별도로 막아야 한다.
        if (!GameManager.I.IsPlaying || Time.timeScale == 0f) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) SetMode(BuildMode.Wall);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SetMode(BuildMode.Spike);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SetMode(BuildMode.FireTrap);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) SetMode(BuildMode.ExplosiveTrap);
        else if (Input.GetKeyDown(KeyCode.Alpha5)) SetMode(BuildMode.Demolish);
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (mode != BuildMode.None) ConsumedEscape = true;
            SetMode(BuildMode.None);
        }

        if (mode == BuildMode.None) return;

        // 우클릭은 어떤 모드에서든 건축 모드 자체를 취소한다.
        if (Input.GetMouseButtonDown(1))
        {
            SetMode(BuildMode.None);
            return;
        }

        Vector3 screen = Input.mousePosition;
        screen.z = -cam.transform.position.z;
        Vector3 world = cam.ScreenToWorldPoint(screen);
        world.z = 0f;
        Vector3Int cell = grid.WorldToCell(world);

        if (mode == BuildMode.Demolish)
        {
            bool canDemolish = CanDemolish(cell);
            UpdateDemolishHighlight(canDemolish ? StructureAt(cell) : null);
            if (canDemolish && Input.GetMouseButtonDown(0)) Demolish(cell);
            return;
        }

        bool valid = CanPlace(cell);

        previewRoot.position = grid.CellToWorld(cell);
        Color tint = valid ? Color.white : new Color(1f, 0.3f, 0.3f);
        foreach (var (sr, baseColor) in previewSprites)
            sr.color = new Color(baseColor.r * tint.r, baseColor.g * tint.g, baseColor.b * tint.b, 0.5f);

        if (valid && Input.GetMouseButtonDown(0)) Place(cell);
    }

    public void SelectWall() => SetMode(BuildMode.Wall);
    public void SelectSpike() => SetMode(BuildMode.Spike);
    public void SelectFireTrap() => SetMode(BuildMode.FireTrap);
    public void SelectExplosiveTrap() => SetMode(BuildMode.ExplosiveTrap);
    public void SelectDemolish() => SetMode(BuildMode.Demolish);
    public void SelectNone() => SetMode(BuildMode.None);

    GameObject PrefabFor(BuildMode m) => m switch
    {
        BuildMode.Wall => wallPrefab,
        BuildMode.Spike => spikePrefab,
        BuildMode.FireTrap => fireTrapPrefab,
        BuildMode.ExplosiveTrap => explosiveTrapPrefab,
        _ => null,
    };

    public int CostFor(BuildMode m)
    {
        int baseCost = m switch
        {
            BuildMode.Wall => wallCost,
            BuildMode.Spike => spikeCost,
            BuildMode.FireTrap => fireTrapCost,
            BuildMode.ExplosiveTrap => explosiveTrapCost,
            _ => 0,
        };
        return Mathf.RoundToInt(baseCost * costMultiplier);
    }

    void SetMode(BuildMode m)
    {
        mode = m;

        UpdateDemolishHighlight(null);

        foreach (Transform child in previewRoot) Destroy(child.gameObject);
        previewSprites.Clear();

        var prefab = PrefabFor(mode);
        if (prefab != null)
        {
            // 스프라이트 하나만 베껴서는(이전 방식) 자식이 여러 개인 프리팹(중첩된 소품 등)에서
            // 일부가 안 보이고, 루트 스케일도 무시돼 크기가 어긋났다. 자식 SpriteRenderer를
            // 전부 순회해 프리팹 루트 기준 상대 위치·최종 스케일 그대로 복제한다
            // (이 프로젝트의 배치 프롭들은 전부 회전 없이(identity) 만들어져 있어 위치는 단순 뺄셈으로 충분).
            Vector3 rootPos = prefab.transform.position;
            foreach (var src in prefab.GetComponentsInChildren<SpriteRenderer>(true))
            {
                // 비활성화된 SpriteRenderer(예: 안 쓰는 예전 스프라이트가 꺼진 채 남아있는 경우)는
                // 실제 설치 시에도 보이지 않으므로 미리보기에도 그리지 않는다.
                if (!src.enabled) continue;

                var go = new GameObject("PreviewSprite");
                go.transform.SetParent(previewRoot, false);
                go.transform.localPosition = src.transform.position - rootPos;
                go.transform.localScale = src.transform.lossyScale;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = src.sprite;
                sr.sortingOrder = src.sortingOrder + 100;
                previewSprites.Add((sr, src.color));
            }

            // 유인형 함정(FireTrap)만 미리보기에 유인 범위 원을 함께 그린다.
            var fireTrap = prefab.GetComponent<FireTrap>();
            if (fireTrap != null)
                RangeIndicatorUtil.CreateCircle(previewRoot, fireTrap.lureRadius, LureRangeColor, "LurePreview", startActive: true);

            // 폭탄(ExplosiveTrap)은 미리보기에 폭발 범위 원을 함께 그린다.
            var explosiveTrap = prefab.GetComponent<ExplosiveTrap>();
            if (explosiveTrap != null)
                RangeIndicatorUtil.CreateCircle(previewRoot, explosiveTrap.explosionRadius, ExplosionRangeColor, "ExplosionPreview", startActive: true);
        }

        previewRoot.gameObject.SetActive(mode != BuildMode.None);
    }

    public bool IsBuildMode => mode != BuildMode.None;

    // 벽/함정 공용 유효성 검사.
    bool CanPlace(Vector3Int cell)
    {
        if (!grid.InBounds(cell)) return false;
        if (!grid.IsWalkable(cell)) return false;
        if (grid.IsCastle(cell) || grid.IsSpawn(cell)) return false;
        if (Spike.ByCell.ContainsKey(cell)) return false;
        if (FireTrap.ByCell.ContainsKey(cell)) return false;
        if (ExplosiveTrap.ByCell.ContainsKey(cell)) return false;
        foreach (var e in Enemy.All)
            if (e.CurrentCell == cell) return false;
        if (Player.I != null)
        {
            float sqr = ((Vector2)grid.CellToWorld(cell) - (Vector2)Player.I.transform.position).sqrMagnitude;
            if (sqr > Player.I.buildRange * Player.I.buildRange) return false;
        }
        if (Economy.I.gold < CostFor(mode)) return false;
        return true;
    }

    void Place(Vector3Int cell)
    {
        if (!Economy.I.TrySpend(CostFor(mode))) return;

        Instantiate(PrefabFor(mode), grid.CellToWorld(cell), Quaternion.identity, placedParent);

        // Wall.Awake()는 점유 등록만 하고 재계산은 안 하므로(씬 시작 시점 벽과 구분하기 위해),
        // 런타임 배치는 이 콜사이트에서 명시적으로 재계산을 트리거해야 한다.
        if (mode == BuildMode.Wall) Pathfinder.I.Recompute();
    }

    GameObject StructureAt(Vector3Int cell)
    {
        if (Wall.ByCell.TryGetValue(cell, out var wall)) return wall.gameObject;
        if (Spike.ByCell.TryGetValue(cell, out var spike)) return spike.gameObject;
        if (FireTrap.ByCell.TryGetValue(cell, out var fireTrap)) return fireTrap.gameObject;
        if (ExplosiveTrap.ByCell.TryGetValue(cell, out var explosiveTrap)) return explosiveTrap.gameObject;
        return null;
    }

    bool CanDemolish(Vector3Int cell)
    {
        if (!grid.InBounds(cell)) return false;
        if (Player.I != null)
        {
            float sqr = ((Vector2)grid.CellToWorld(cell) - (Vector2)Player.I.transform.position).sqrMagnitude;
            if (sqr > Player.I.buildRange * Player.I.buildRange) return false;
        }
        return StructureAt(cell) != null;
    }

    // 철거 대상 위에 커서가 있을 때 빨갛게 표시한다 — 별도 프리팹 없이 대상 오브젝트의
    // 스프라이트 색을 그대로 덧칠했다가(target이 바뀌거나 모드를 벗어나면) 원래 색으로 되돌린다.
    void UpdateDemolishHighlight(GameObject target)
    {
        if (target == demolishHighlightTarget) return;

        foreach (var (sr, baseColor) in demolishHighlightSprites)
            if (sr != null) sr.color = baseColor;
        demolishHighlightSprites.Clear();
        demolishHighlightTarget = target;

        if (target == null) return;
        foreach (var sr in target.GetComponentsInChildren<SpriteRenderer>(true))
        {
            demolishHighlightSprites.Add((sr, sr.color));
            sr.color = new Color(1f, 0.3f, 0.3f, sr.color.a);
        }
    }

    // 철거는 환불 없이 즉시 파괴만 한다 — 각 구조물의 OnDestroy가 타일 점유 해제/
    // 경로 재계산/유인 해제 등 자기 몫의 정리를 알아서 처리한다.
    void Demolish(Vector3Int cell)
    {
        var target = StructureAt(cell);
        if (target == null) return;
        UpdateDemolishHighlight(null);
        Destroy(target);
    }
}
