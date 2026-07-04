using UnityEngine;

// 원거리 적 전용 공격 이펙트. Player의 Projectile과 달리 충돌 판정을 하지 않는다 —
// 데미지는 Enemy.cs가 공격 판정 시점에 이미 즉시 적용했고, 이건 그 공격을 눈에 보이게
// 만들기 위한 순수 시각효과라 목표 지점에 도달하면 그냥 스스로 파괴된다.
public class EnemyProjectile : MonoBehaviour
{
    Vector3 target;
    float speed;

    public void Init(Vector3 target, float speed)
    {
        this.target = target;
        this.speed = speed;

        Vector3 dir = target - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        if (((Vector2)transform.position - (Vector2)target).sqrMagnitude < 0.01f)
            Destroy(gameObject);
    }
}
