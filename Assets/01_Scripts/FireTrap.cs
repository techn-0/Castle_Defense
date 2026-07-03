using System.Collections.Generic;
using UnityEngine;

public class FireTrap : MonoBehaviour
{
    public static readonly Dictionary<Vector3Int, FireTrap> ByCell = new();

    public float lureRadius = 3f;
    public int tickDamage = 1;
    public float tickInterval = 0.5f;
    public float dwellDuration = 3f;

    Vector3Int cell;
    readonly Dictionary<Enemy, float> tickTimers = new();

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

    void Update()
    {
        foreach (var e in Enemy.All)
        {
            // 이미 이 함정을 한 번이라도 다녀간 적은 다시 유인하지 않는다(Enemy.visitedLures로 관리).
            if (e.LuredBy != null || e.HasVisitedLure(transform)) continue;
            float sqr = ((Vector2)e.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (sqr < lureRadius * lureRadius) e.GetLured(transform, dwellDuration);
        }
    }

    // 체류 해제(dwell release)는 이제 Enemy 쪽에서 자체 타이머로 보장한다(Enemy.cs 참고) —
    // 여기서는 접촉 데미지 틱만 담당한다.
    void OnTriggerStay2D(Collider2D other)
    {
        var enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;

        if (!tickTimers.TryGetValue(enemy, out var tt)) tt = tickInterval;
        tt -= Time.deltaTime;
        if (tt <= 0f) { enemy.TakeDamage(tickDamage); tt = tickInterval; }
        tickTimers[enemy] = tt;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;
        tickTimers.Remove(enemy);
    }
}
