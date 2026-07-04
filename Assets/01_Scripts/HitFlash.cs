using System.Collections;
using UnityEngine;

/// <summary>
/// SpriteRenderer 히트 점멸 + 스케일 펀치(순간 축소 후 복귀) 효과.
/// 하위 SpriteRenderer가 여러 개여도 일괄 점멸.
///
/// [사용법]
///   1. hitFlashMaterial을 Inspector에서 연결. 비워두면 Resources/HitFlash.mat을 자동으로 찾는다
///      (Enemy처럼 런타임에 AddComponent로 붙는 경우 Inspector 연결이 불가능하므로 이 폴백이 필요).
///   2. targets 비워두면 자신+하위 SpriteRenderer 전부 자동 수집
///   3. 피격 시 Flash() 호출 — 점멸과 스케일 펀치가 함께 재생
///
/// [스케일 펀치]
///   - transform 크기(절대값)만 조절, 현재 좌우 반전 부호는 유지 → 좌우 반전 로직과 충돌 없음
///   - 기준 스케일은 첫 Flash() 때 캡처 → elite 스폰 배율까지 자동 반영
/// </summary>
public class HitFlash : MonoBehaviour
{
    [Header("점멸")]
    [Tooltip("Custom/HitFlash 셰이더가 적용된 머티리얼. 비워두면 Resources/HitFlash를 로드")]
    [SerializeField] private Material hitFlashMaterial;

    [Tooltip("점멸 색상 (셰이더에 _FlashColor 프로퍼티가 있어야 적용)")]
    [SerializeField] private Color flashColor = Color.white;

    [Tooltip("점멸 지속 시간 (초)")]
    [SerializeField] private float flashDuration = 0.08f;

    [Tooltip("대상 SpriteRenderer 목록. 비워두면 자신+하위 전부 자동 수집")]
    [SerializeField] private SpriteRenderer[] targets;

    [Header("스케일 펀치")]
    [Tooltip("피격 시 순간 축소 효과 사용")]
    [SerializeField] private bool enableScalePunch = true;

    [Tooltip("순간적으로 줄어드는 최소 배율 (1 = 변화 없음)")]
    [SerializeField, Range(0.1f, 1f)] private float punchScale = 0.8f;

    [Tooltip("줄었다 돌아오는 총 시간 (초)")]
    [SerializeField] private float punchDuration = 0.12f;

    private Material[] matInstances;
    private Coroutine flashRoutine;
    private Coroutine punchRoutine;

    private Vector3 baseScaleAbs;   // 기준 스케일(절대값)
    private bool baseCaptured;

    static readonly int FlashAmountID = Shader.PropertyToID("_FlashAmount");
    static readonly int FlashColorID  = Shader.PropertyToID("_FlashColor");

    void Awake()
    {
        if (hitFlashMaterial == null)
            hitFlashMaterial = Resources.Load<Material>("HitFlash");
        if (hitFlashMaterial == null) return;

        if (targets == null || targets.Length == 0)
            targets = GetComponentsInChildren<SpriteRenderer>(true);

        matInstances = new Material[targets.Length];
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == null) continue;
            var mat = new Material(hitFlashMaterial);
            if (mat.HasProperty(FlashColorID))
                mat.SetColor(FlashColorID, flashColor);
            targets[i].material = mat;
            matInstances[i] = mat;
        }
    }

    /// <summary>점멸 + 스케일 펀치 1회 재생.</summary>
    public void Flash()
    {
        if (matInstances != null)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(DoFlash());
        }

        if (enableScalePunch)
        {
            if (punchRoutine != null) StopCoroutine(punchRoutine);
            punchRoutine = StartCoroutine(DoPunch());
        }
    }

    /// <summary>효과 즉시 중단 + 원래 크기 복원 (사망 등).</summary>
    public void StopFlash()
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = null;
        SetAllFlash(0f);

        if (punchRoutine != null) StopCoroutine(punchRoutine);
        punchRoutine = null;
        if (baseCaptured) ApplyScale(1f);
    }

    IEnumerator DoFlash()
    {
        SetAllFlash(1f);
        yield return new WaitForSeconds(flashDuration);
        SetAllFlash(0f);
        flashRoutine = null;
    }

    IEnumerator DoPunch()
    {
        if (!baseCaptured)
        {
            Vector3 s = transform.localScale;
            baseScaleAbs = new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
            baseCaptured = true;
        }

        float half = Mathf.Max(0.0001f, punchDuration * 0.5f);

        // 1 → punchScale (축소)
        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            ApplyScale(Mathf.Lerp(1f, punchScale, t / half));
            yield return null;
        }
        // punchScale → 1 (복귀)
        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            ApplyScale(Mathf.Lerp(punchScale, 1f, t / half));
            yield return null;
        }

        ApplyScale(1f);
        punchRoutine = null;
    }

    // 크기(절대값)만 적용, 현재 좌우 반전 부호는 유지
    void ApplyScale(float k)
    {
        Vector3 cur = transform.localScale;
        transform.localScale = new Vector3(
            baseScaleAbs.x * k * Mathf.Sign(cur.x == 0f ? 1f : cur.x),
            baseScaleAbs.y * k,
            baseScaleAbs.z * k);
    }

    void SetAllFlash(float amount)
    {
        if (matInstances == null) return;
        foreach (var mat in matInstances)
            if (mat != null) mat.SetFloat(FlashAmountID, amount);
    }

    void OnDestroy()
    {
        if (matInstances == null) return;
        foreach (var mat in matInstances)
            if (mat != null) Destroy(mat);
    }
}
