using System.Collections.Generic;
using UnityEngine;

public class Spike : MonoBehaviour
{
    public static readonly Dictionary<Vector3Int, Spike> ByCell = new();

    public int damage = 1;
    public int durability = 3;

    Vector3Int cell;

    // 벽과 달리 grid.SetOccupied를 호출하지 않는다 — 함정은 이동을 막지 않고
    // 밟고 지나가며 데미지만 주는 게 목적이라 경로 계산에 영향을 주면 안 된다.
    void Awake()
    {
        var grid = FindFirstObjectByType<TileGrid>();
        cell = grid.WorldToCell(transform.position);
        ByCell[cell] = this;
    }

    void OnDestroy()
    {
        if (ByCell.TryGetValue(cell, out var s) && s == this) ByCell.Remove(cell);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;

        enemy.TakeDamage(damage);
        durability--;
        if (durability <= 0) Destroy(gameObject);
    }
}
