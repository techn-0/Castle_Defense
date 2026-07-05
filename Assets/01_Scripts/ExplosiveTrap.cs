using System.Collections.Generic;
using UnityEngine;

public class ExplosiveTrap : MonoBehaviour
{
    public static readonly Dictionary<Vector3Int, ExplosiveTrap> ByCell = new();

    public int explosionDamage = 5;
    public float explosionRadius = 1.5f;
    public AudioClip explosionSfx;
    public GameObject explosionEffectPrefab;

    Vector3Int cell;
    LineRenderer explosionIndicator;

    void Awake()
    {
        var grid = FindFirstObjectByType<TileGrid>();
        cell = grid.WorldToCell(transform.position);
        ByCell[cell] = this;

        explosionIndicator = RangeIndicatorUtil.CreateCircle(transform, explosionRadius, BuildManager.ExplosionRangeColor, "ExplosionIndicator");
    }

    void OnDestroy()
    {
        if (ByCell.TryGetValue(cell, out var s) && s == this) ByCell.Remove(cell);
    }

    void Update()
    {
        explosionIndicator.gameObject.SetActive(BuildManager.I != null && BuildManager.I.IsBuildMode);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Enemy>() == null) return;
        Explode();
    }

    // Enemy.All을 직접 순회하며 TakeDamage를 호출하면, 사망한 적이 OnDestroy()에서
    // 같은 리스트를 변형시켜 반복 중 예외가 나므로 스냅샷을 떠서 순회한다.
    void Explode()
    {
        foreach (var e in new List<Enemy>(Enemy.All))
        {
            float sqr = ((Vector2)e.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (sqr < explosionRadius * explosionRadius) e.TakeDamage(explosionDamage);
        }
        if (explosionSfx != null) AudioSource.PlayClipAtPoint(explosionSfx, transform.position);
        EffectsUtil.SpawnBurst(transform.position, new Color(1f, 0.5f, 0.1f), 16, 3f);
        if (explosionEffectPrefab != null)
        {
            var effect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }
        Destroy(gameObject);
    }
}
