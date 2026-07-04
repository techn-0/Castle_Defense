using System.Collections.Generic;
using UnityEngine;

public class FireTrap : MonoBehaviour
{
    public static readonly Dictionary<Vector3Int, FireTrap> ByCell = new();

    public float lureRadius = 3f;
    public int tickDamage = 1;
    public float tickInterval = 0.5f;
    public float dwellDuration = 3f;
    public int durability = 8;
    public float burnLingerDuration = 5f;
    public AudioClip hitSfx;
    public AudioClip destroySfx;

    Vector3Int cell;
    int maxDurability;
    HealthBar healthBar;
    readonly Dictionary<Enemy, float> tickTimers = new();

    void Awake()
    {
        var grid = FindFirstObjectByType<TileGrid>();
        cell = grid.WorldToCell(transform.position);
        ByCell[cell] = this;

        maxDurability = durability;
        healthBar = GetComponent<HealthBar>();
        if (healthBar == null) healthBar = gameObject.AddComponent<HealthBar>();
    }

    void OnDestroy()
    {
        if (ByCell.TryGetValue(cell, out var s) && s == this) ByCell.Remove(cell);
        // 소진되어 파괴될 때 이 함정에 유인되어 오던/체류 중이던 적을 정상적으로 풀어준다 —
        // 그냥 파괴에 맡기면 luredBy가 Unity의 fake-null이 되어 Update()의 셀 스냅 로직을
        // 건너뛰고 어중간한 위치에서 경로찾기가 재개된다.
        foreach (var e in new List<Enemy>(Enemy.All))
        {
            if (e.LuredBy == transform) e.ReleaseLure();
        }
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
        if (durability <= 0) return;
        var enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;

        if (!tickTimers.TryGetValue(enemy, out var tt)) tt = tickInterval;
        tt -= Time.deltaTime;
        if (tt <= 0f)
        {
            enemy.TakeDamage(tickDamage);
            // 강화 후에는 접촉이 끝나도(트랩을 벗어나도) 같은 틱뎀/주기로 일정 시간 계속 불타게 한다.
            if (UpgradeManager.I != null && UpgradeManager.I.FireSpreadUnlocked)
                enemy.ApplyBurn(tickDamage, tickInterval, burnLingerDuration);
            tt = tickInterval;
            ConsumeDurability();
        }
        tickTimers[enemy] = tt;
    }

    // 스파이크(밟은 횟수 소모)와 달리 화염 함정은 틱마다 소모된다 — 지속딜인 만큼
    // 무한히 눌러앉아 딜을 넣지 못하도록 내구도가 다하면 스스로 파괴된다.
    void ConsumeDurability()
    {
        durability--;
        if (durability <= 0)
        {
            if (destroySfx != null) AudioSource.PlayClipAtPoint(destroySfx, transform.position);
            EffectsUtil.SpawnBurst(transform.position, new Color(1f, 0.5f, 0.1f));
            Destroy(gameObject);
            return;
        }
        if (hitSfx != null) AudioSource.PlayClipAtPoint(hitSfx, transform.position);
        healthBar.SetFraction((float)durability / maxDurability);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;
        tickTimers.Remove(enemy);
    }
}
