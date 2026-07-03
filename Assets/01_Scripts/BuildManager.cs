using System.Collections.Generic;
using UnityEngine;

public enum BuildMode { None, Wall, Spike, FireTrap, ExplosiveTrap }

public class BuildManager : MonoBehaviour
{
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

    BuildMode mode = BuildMode.None;
    TileGrid grid;

    Transform previewRoot;
    readonly List<(SpriteRenderer sr, Color baseColor)> previewSprites = new();

    void Awake()
    {
        grid = FindFirstObjectByType<TileGrid>();
        if (cam == null) cam = Camera.main;

        previewRoot = new GameObject("BuildPreview").transform;
    }

    void Update()
    {
        if (!GameManager.I.IsPlaying) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) SetMode(BuildMode.Wall);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SetMode(BuildMode.Spike);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SetMode(BuildMode.FireTrap);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) SetMode(BuildMode.ExplosiveTrap);
        else if (Input.GetKeyDown(KeyCode.Escape)) SetMode(BuildMode.None);

        if (mode == BuildMode.None) return;

        Vector3 screen = Input.mousePosition;
        screen.z = -cam.transform.position.z;
        Vector3 world = cam.ScreenToWorldPoint(screen);
        world.z = 0f;
        Vector3Int cell = grid.WorldToCell(world);
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
    public void SelectNone() => SetMode(BuildMode.None);

    GameObject PrefabFor(BuildMode m) => m switch
    {
        BuildMode.Wall => wallPrefab,
        BuildMode.Spike => spikePrefab,
        BuildMode.FireTrap => fireTrapPrefab,
        BuildMode.ExplosiveTrap => explosiveTrapPrefab,
        _ => null,
    };

    int CostFor(BuildMode m) => m switch
    {
        BuildMode.Wall => wallCost,
        BuildMode.Spike => spikeCost,
        BuildMode.FireTrap => fireTrapCost,
        BuildMode.ExplosiveTrap => explosiveTrapCost,
        _ => 0,
    };

    void SetMode(BuildMode m)
    {
        mode = m;

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
                var go = new GameObject("PreviewSprite");
                go.transform.SetParent(previewRoot, false);
                go.transform.localPosition = src.transform.position - rootPos;
                go.transform.localScale = src.transform.lossyScale;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = src.sprite;
                sr.sortingOrder = src.sortingOrder + 100;
                previewSprites.Add((sr, src.color));
            }
        }

        previewRoot.gameObject.SetActive(mode != BuildMode.None);
    }

    // 벽/함정 공용 유효성 검사.
    bool CanPlace(Vector3Int cell)
    {
        if (!grid.InBounds(cell)) return false;
        if (!grid.IsWalkable(cell)) return false;
        if (grid.IsCastle(cell) || grid.IsSpawn(cell)) return false;
        if (Spike.ByCell.ContainsKey(cell)) return false;
        if (FireTrap.ByCell.ContainsKey(cell)) return false;
        if (ExplosiveTrap.ByCell.ContainsKey(cell)) return false;
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
}
