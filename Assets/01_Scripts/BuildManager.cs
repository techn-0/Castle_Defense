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
    SpriteRenderer preview;

    void Awake()
    {
        grid = FindFirstObjectByType<TileGrid>();
        if (cam == null) cam = Camera.main;

        var go = new GameObject("BuildPreview");
        preview = go.AddComponent<SpriteRenderer>();
        preview.sortingOrder = 10;
        preview.enabled = false;
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

        preview.transform.position = grid.CellToWorld(cell);
        preview.color = valid ? new Color(1, 1, 1, 0.5f) : new Color(1, 0.3f, 0.3f, 0.5f);

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
        var prefab = PrefabFor(mode);
        preview.sprite = prefab != null ? prefab.GetComponentInChildren<SpriteRenderer>().sprite : null;
        preview.enabled = mode != BuildMode.None;
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
