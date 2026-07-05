using System.Collections.Generic;
using UnityEngine;

public class Spike : MonoBehaviour
{
    public static readonly Dictionary<Vector3Int, Spike> ByCell = new();

    // 슬로우 강화가 해금된 함정임을 알리는 초록빛 틴트 — 슬로우 걸린 적(Enemy)에도
    // 동일 색을 써서 "슬로우 = 초록"이라는 시각 언어를 통일한다.
    public static readonly Color SlowTintColor = new Color(0.5f, 1f, 0.5f);

    public int damage = 1;
    public int durability = 3;
    public float slowMultiplier = 0.5f;
    public float slowDuration = 3f;
    public AudioClip hitSfx;
    public AudioClip destroySfx;

    Vector3Int cell;
    int maxDurability;
    HealthBar healthBar;
    SpriteRenderer sr;
    Color baseColor = Color.white;

    // 벽과 달리 grid.SetOccupied를 호출하지 않는다 — 함정은 이동을 막지 않고
    // 밟고 지나가며 데미지만 주는 게 목적이라 경로 계산에 영향을 주면 안 된다.
    void Awake()
    {
        var grid = FindFirstObjectByType<TileGrid>();
        cell = grid.WorldToCell(transform.position);
        ByCell[cell] = this;

        maxDurability = durability;
        healthBar = GetComponent<HealthBar>();
        if (healthBar == null) healthBar = gameObject.AddComponent<HealthBar>();

        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
        UpdateSlowTint();
    }

    void OnDestroy()
    {
        if (ByCell.TryGetValue(cell, out var s) && s == this) ByCell.Remove(cell);
    }

    // 슬로우 강화가 해금된 순간 이미 깔려 있던 함정들도 즉시 초록빛으로 갱신되도록
    // UpgradeManager.UnlockSpikeSlow()에서 호출한다.
    public static void RefreshAllSlowTint()
    {
        foreach (var spike in ByCell.Values) spike.UpdateSlowTint();
    }

    void UpdateSlowTint()
    {
        if (sr == null) return;
        bool slowActive = UpgradeManager.I != null && UpgradeManager.I.SpikeSlowUnlocked;
        Color mul = slowActive ? SlowTintColor : Color.white;
        sr.color = new Color(baseColor.r * mul.r, baseColor.g * mul.g, baseColor.b * mul.b, baseColor.a);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;

        enemy.TakeDamage(damage);
        if (UpgradeManager.I != null && UpgradeManager.I.SpikeSlowUnlocked)
            enemy.ApplySlow(slowMultiplier, slowDuration);
        durability--;
        if (durability <= 0)
        {
            if (destroySfx != null) AudioSource.PlayClipAtPoint(destroySfx, transform.position);
            EffectsUtil.SpawnBurst(transform.position, Color.red);
            Destroy(gameObject);
            return;
        }
        if (hitSfx != null) AudioSource.PlayClipAtPoint(hitSfx, transform.position);
        healthBar.SetFraction((float)durability / maxDurability);
    }
}
