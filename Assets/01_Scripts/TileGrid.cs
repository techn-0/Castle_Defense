using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Tilemap을 감싸는 그리드 레이어. 타일 종류(Walkable/Castle/Spawn)는 칠해진 Tile 애셋으로
// 정적으로 결정되고, occupied(벽 점유 여부)는 런타임에 별도로 관리한다.
public class TileGrid : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase walkableTile, castleTile, spawnTile;

    readonly Dictionary<Vector3Int, bool> occupied = new();
    BoundsInt bounds;

    void Awake()
    {
        tilemap.CompressBounds();
        bounds = tilemap.cellBounds;
    }

    public bool InBounds(Vector3Int cell) => bounds.Contains(cell);

    bool HasBaseTile(Vector3Int cell)
    {
        var t = tilemap.GetTile(cell);
        return t == walkableTile || t == castleTile || t == spawnTile;
    }

    public bool IsCastle(Vector3Int cell) => tilemap.GetTile(cell) == castleTile;
    public bool IsSpawn(Vector3Int cell) => tilemap.GetTile(cell) == spawnTile;
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
        foreach (var c in AllCells())
            if (IsCastle(c)) yield return c;
    }

    public IEnumerable<Vector3Int> GetSpawnCells()
    {
        foreach (var c in AllCells())
            if (IsSpawn(c)) yield return c;
    }

    static readonly Vector3Int[] Dirs4 = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

    public IEnumerable<Vector3Int> GetNeighbors4(Vector3Int cell)
    {
        foreach (var d in Dirs4) yield return cell + d;
    }
}
