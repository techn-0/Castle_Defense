using UnityEngine;

// 파괴 이펙트 공용 헬퍼. 외부 파티클/스프라이트 에셋 없이 기본 스프라이트 셰이더만으로
// 짧게 터졌다 사라지는 파티클을 만든다.
public static class EffectsUtil
{
    static Material burstMaterial;

    public static void SpawnBurst(Vector3 pos, Color color, int count = 10, float speed = 2f, float life = 0.35f)
    {
        var go = new GameObject("DestroyBurst");
        go.transform.position = pos;

        var ps = go.AddComponent<ParticleSystem>();
        // AddComponent는 playOnAwake 기본값(true) 때문에 즉시 재생을 시작해버려서,
        // 바로 이어지는 main.duration 등의 설정이 "재생 중 변경" 에러를 낸다 — 먼저 멈춘 뒤 설정한다.
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.duration = life;
        main.loop = false;
        main.startLifetime = life;
        main.startSpeed = speed;
        main.startSize = 0.12f;
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.05f;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = GetBurstMaterial();

        Object.Destroy(go, life + 0.2f);
        ps.Play();
    }

    static Material GetBurstMaterial()
    {
        if (burstMaterial == null) burstMaterial = new Material(Shader.Find("Sprites/Default"));
        return burstMaterial;
    }
}
