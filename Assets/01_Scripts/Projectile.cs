using System.Collections.Generic;
using UnityEngine;

// Player 전용 투사체. 고정 방향으로 직선 이동하며, 물리 트리거로 충돌을 판정한다
// (호밍 방식이었을 때는 위치 비교로 "도착"을 판정했지만, 고정 방향에서는 그 개념이
// 없으므로 실제 Collider2D(Trigger) + Rigidbody2D(Kinematic)가 프리팹에 있어야 한다).
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    public float maxLifetime = 3f;
    public AudioClip explosionSfx;
    public GameObject explosionEffectPrefab;

    Vector2 direction;
    float speed;
    int damage;
    int pierceRemaining;
    bool splash;
    float splashRadius;
    int splashDamage;
    float lifeTimer;

    public void Init(Vector2 direction, float speed, int damage, int pierceCount = 0, bool splash = false, float splashRadius = 1.2f, int splashDamage = 1)
    {
        this.direction = direction.normalized;
        this.speed = speed;
        this.damage = damage;
        this.pierceRemaining = pierceCount;
        this.splash = splash;
        this.splashRadius = splashRadius;
        this.splashDamage = splashDamage;

        // 호밍이 아니므로 회전은 스폰 시 한 번만 정하면 된다.
        float angle = Mathf.Atan2(this.direction.y, this.direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // 사거리 강화가 누적될수록 화면 밖으로 더 오래 날아갈 수 있어 고정값 대신 여유를 둔다.
        if (Player.I != null && speed > 0f)
            maxLifetime = Mathf.Max(maxLifetime, Player.I.attackRange / speed * 1.5f);
    }

    void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer > maxLifetime) { Destroy(gameObject); return; }

        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;

        enemy.TakeDamage(damage);
        if (splash) ApplySplash(enemy);

        if (pierceRemaining > 0)
        {
            pierceRemaining--;
            return; // 파괴하지 않고 계속 직진해 다른 적을 추가로 맞힌다.
        }

        Destroy(gameObject);
    }

    void ApplySplash(Enemy hitEnemy)
    {
        if (explosionSfx != null) AudioSource.PlayClipAtPoint(explosionSfx, transform.position);
        EffectsUtil.SpawnBurst(transform.position, new Color(1f, 0.5f, 0.1f), 16, 3f);
        if (explosionEffectPrefab != null)
        {
            var effect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }

        foreach (var e in new List<Enemy>(Enemy.All))
        {
            if (e == hitEnemy) continue;
            float sqr = ((Vector2)e.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (sqr < splashRadius * splashRadius) e.TakeDamage(splashDamage);
        }
    }
}
