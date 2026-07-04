using UnityEngine;

// Player 전용 투사체. 물리 콜라이더 없이 타겟 참조 + 거리 판정만으로 동작한다
// (Enemy 이동부의 MoveTowards + sqrMagnitude 도착 판정과 동일한 스타일).
public class Projectile : MonoBehaviour
{
    public float maxLifetime = 3f;

    Enemy target;
    float speed;
    int damage;
    float lifeTimer;

    public void Init(Enemy target, float speed, int damage)
    {
        this.target = target;
        this.speed = speed;
        this.damage = damage;

        if (target != null) FaceTarget(target.transform.position);
    }

    void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer > maxLifetime) { Destroy(gameObject); return; }

        // 타겟이 비행 중 다른 수단으로 파괴됐다면(Unity의 == 오버라이드로 null 비교가 정상 동작) 조용히 자폭.
        if (target == null) { Destroy(gameObject); return; }

        Vector3 dest = target.transform.position;
        FaceTarget(dest);
        transform.position = Vector2.MoveTowards(transform.position, dest, speed * Time.deltaTime);

        if (((Vector2)transform.position - (Vector2)dest).sqrMagnitude < 0.01f)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    // 타겟이 매 프레임 움직이므로(호밍) 화살촉이 계속 이동 방향을 향하도록 매 프레임 갱신한다.
    // 화살 비주얼(자식)이 이미 "부모 기준 오른쪽을 바라볼 때"에 맞춘 로컬 회전 보정값을 갖고 있으므로,
    // 루트만 이동 방향으로 돌리면 된다.
    void FaceTarget(Vector3 dest)
    {
        Vector2 dir = (Vector2)dest - (Vector2)transform.position;
        if (dir.sqrMagnitude < 0.0001f) return;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
