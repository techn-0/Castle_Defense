using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public static readonly List<Wall> All = new();
    public static readonly Dictionary<Vector3Int, Wall> ByCell = new();

    public int hp = 5;
    public int thornDamage = 1;
    public AudioClip hitSfx;
    public AudioClip destroySfx;
    public Vector3Int Cell => cell;

    TileGrid grid;
    Vector3Int cell;
    int maxHp;
    HealthBar healthBar;

    // 점유 등록은 Awake에서만 한다(Recompute는 부르지 않음) — 씬에 미리 놓인 벽은
    // 전부 Awake가 끝난 뒤 Pathfinder.Start()의 최초 BFS에서 자동 반영된다.
    // 여기서 Recompute를 부르면 Enemy.Start()가 먼저 도는 경우 잘못된 경로를 잡을 수 있다.
    void Awake()
    {
        grid = FindFirstObjectByType<TileGrid>();
        cell = grid.WorldToCell(transform.position);
        grid.SetOccupied(cell, true);
        All.Add(this);
        ByCell[cell] = this;

        maxHp = hp;
        healthBar = GetComponent<HealthBar>();
        if (healthBar == null) healthBar = gameObject.AddComponent<HealthBar>();
    }

    void OnDestroy()
    {
        All.Remove(this);
        if (ByCell.TryGetValue(cell, out var w) && w == this) ByCell.Remove(cell);
        grid.SetOccupied(cell, false);
        if (Pathfinder.I != null) Pathfinder.I.Recompute();
    }

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        if (hp <= 0)
        {
            if (destroySfx != null) AudioSource.PlayClipAtPoint(destroySfx, transform.position);
            EffectsUtil.SpawnBurst(transform.position, Color.gray);
            Destroy(gameObject);
            return;
        }
        if (hitSfx != null) AudioSource.PlayClipAtPoint(hitSfx, transform.position);
        healthBar.SetFraction((float)hp / maxHp);
    }
}
