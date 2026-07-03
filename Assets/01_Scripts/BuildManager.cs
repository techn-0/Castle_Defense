using UnityEngine;

public enum BuildMode { None, Wall, Spike }

public class BuildManager : MonoBehaviour
{
    public GameObject wallPrefab;
    public GameObject spikePrefab;
    public Transform placedParent;
    public Camera cam;

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
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetMode(BuildMode.Wall);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SetMode(BuildMode.Spike);
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

    void SetMode(BuildMode m)
    {
        mode = m;
        var prefab = mode == BuildMode.Wall ? wallPrefab : mode == BuildMode.Spike ? spikePrefab : null;
        preview.sprite = prefab != null ? prefab.GetComponentInChildren<SpriteRenderer>().sprite : null;
        preview.enabled = mode != BuildMode.None;
    }

    // 벽/함정 공용 유효성 검사. 골드 비용 체크는 Phase 4(Economy) 몫이라 여기 없음.
    bool CanPlace(Vector3Int cell)
    {
        if (!grid.InBounds(cell)) return false;
        if (!grid.IsWalkable(cell)) return false;
        if (grid.IsCastle(cell) || grid.IsSpawn(cell)) return false;
        if (Spike.ByCell.ContainsKey(cell)) return false;
        return true;
    }

    void Place(Vector3Int cell)
    {
        var prefab = mode == BuildMode.Wall ? wallPrefab : spikePrefab;
        Instantiate(prefab, grid.CellToWorld(cell), Quaternion.identity, placedParent);

        // Wall.Awake()는 점유 등록만 하고 재계산은 안 하므로(씬 시작 시점 벽과 구분하기 위해),
        // 런타임 배치는 이 콜사이트에서 명시적으로 재계산을 트리거해야 한다.
        if (mode == BuildMode.Wall) Pathfinder.I.Recompute();
    }
}
