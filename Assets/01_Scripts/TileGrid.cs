using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Tilemap을 감싸는 그리드 레이어. "걷기 가능"은 칠해진 Walkable 타일로 정적으로 결정되고,
// Castle/Spawn은 타일이 아니라 씬의 CastleMarker/SpawnPoint 오브젝트 위치로 결정한다
// (HP 등 게임 로직을 가질 성/스폰 지점을 타일맵과 이중으로 관리하지 않기 위함).
// occupied(벽 점유 여부)는 런타임에 별도로 관리한다.
public class TileGrid : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase walkableTile;

    readonly Dictionary<Vector3Int, bool> occupied = new();
    Vector3Int? castleCell;
    readonly List<Vector3Int> spawnCells = new();
    BoundsInt bounds;

    void Awake()
    {
        tilemap.CompressBounds();
        bounds = tilemap.cellBounds;

        var castle = FindFirstObjectByType<CastleMarker>();
        if (castle != null) castleCell = WorldToCell(castle.transform.position);

        foreach (var sp in FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None))
            spawnCells.Add(WorldToCell(sp.transform.position));
    }

    public bool InBounds(Vector3Int cell) => bounds.Contains(cell);

    bool HasBaseTile(Vector3Int cell) => tilemap.GetTile(cell) == walkableTile;

    public bool IsCastle(Vector3Int cell) => castleCell.HasValue && castleCell.Value == cell;
    public bool IsSpawn(Vector3Int cell) => spawnCells.Contains(cell);
    public bool IsOccupied(Vector3Int cell) => occupied.TryGetValue(cell, out var o) && o;
    public bool IsWalkable(Vector3Int cell) => HasBaseTile(cell) && !IsOccupied(cell);

    public void SetOccupied(Vector3Int cell, bool value) => occupied[cell] = value;

    public Vector3Int WorldToCell(Vector3 world) => tilemap.WorldToCell(world);
    public Vector3 CellToWorld(Vector3Int cell) => tilemap.GetCellCenterWorld(cell);

    public IEnumerable<Vector3Int> AllCells()
    {
        foreach (var p in bounds.allPositionsWithin)
            if (HasBaseTile(p)) yield return p;
    }

    public IEnumerable<Vector3Int> GetCastleCells()
    {
        if (castleCell.HasValue) yield return castleCell.Value;
    }

    public IEnumerable<Vector3Int> GetSpawnCells() => spawnCells;

    static readonly Vector3Int[] Dirs4 = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

    public IEnumerable<Vector3Int> GetNeighbors4(Vector3Int cell)
    {
        foreach (var d in Dirs4) yield return cell + d;
    }
}
