using UnityEngine;

// Player의 건설 범위 인디케이터와 동일한 방식(LineRenderer 원)을 함정 유인 범위 표시에도
// 재사용하기 위한 공용 헬퍼.
public static class RangeIndicatorUtil
{
    const int Segments = 36;

    public static LineRenderer CreateCircle(Transform parent, float radius, Color color, string name = "RangeIndicator", bool startActive = false)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.loop = true;
        lr.useWorldSpace = false;
        lr.positionCount = Segments;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        SetRadius(lr, radius);
        go.SetActive(startActive);
        return lr;
    }

    public static void SetRadius(LineRenderer lr, float radius)
    {
        for (int i = 0; i < Segments; i++)
        {
            float angle = 2f * Mathf.PI * i / Segments;
            lr.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
        }
    }
}
