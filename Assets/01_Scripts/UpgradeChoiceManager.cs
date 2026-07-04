using System;
using System.Collections.Generic;
using UnityEngine;

// 웨이브 클리어 시 3택 무료 강화를 담당하는 싱글톤(Economy/UpgradeManager와 동일 패턴).
// 풀 전체를 코드로 구성해두고, 매번 CanOffer()인 옵션 중 무작위 3개를 뽑아 패널에 보여준다.
public class UpgradeChoiceManager : MonoBehaviour
{
    public static UpgradeChoiceManager I;
    public UpgradeChoicePanelUI panelUI;

    readonly List<UpgradeOption> pool = new();
    Action pendingResume;

    void Awake()
    {
        I = this;
        BuildPool();
    }

    void BuildPool()
    {
        pool.Add(new UpgradeOption
        {
            id = "atk_dmg",
            title = "공격력 증가",
            description = "플레이어 공격력이 1 증가합니다.",
            maxStacks = 5,
            apply = () => Player.I.attackDamage += 1,
        });

        pool.Add(new UpgradeOption
        {
            id = "atk_speed",
            title = "공격속도 증가",
            description = "공격속도가 15% 증가합니다.",
            maxStacks = 5,
            apply = () => Player.I.attackInterval = Mathf.Max(0.08f, Player.I.attackInterval * 0.85f),
        });

        pool.Add(new UpgradeOption
        {
            id = "atk_range",
            title = "사거리 증가",
            description = "공격 사거리가 1 증가합니다.",
            maxStacks = 4,
            apply = () => Player.I.attackRange += 1.0f,
        });

        pool.Add(new UpgradeOption
        {
            id = "move_speed",
            title = "이동속도 증가",
            description = "이동속도가 0.5증가합니다.",
            maxStacks = 4,
            apply = () => Player.I.moveSpeed += 0.5f,
        });

        pool.Add(new UpgradeOption
        {
            id = "proj_speed",
            title = "빠른 화살",
            description = "화살이 더 빠르게 날아갑니다.",
            maxStacks = 3,
            apply = () => Player.I.projectileSpeed += 2f,
        });

        pool.Add(new UpgradeOption
        {
            id = "multi_shot",
            title = "일석이조",
            description = "화살을 한 발 더 발사합니다.",
            maxStacks = 2, // 1 -> 2 -> 3발
            apply = () => Player.I.projectileCount += 1,
        });

        pool.Add(new UpgradeOption
        {
            id = "pierce",
            title = "관통 사격",
            description = "화살이 한번 관통합니다,",
            maxStacks = 1,
            apply = () => Player.I.projectilePierce += 1,
        });

        pool.Add(new UpgradeOption
        {
            id = "splash",
            title = "폭탄화살",
            description = "화살이 명중 지점 주변에도 추가 피해를 줍니다.",
            maxStacks = 1,
            apply = () => Player.I.projectileSplash = true,
        });

        pool.Add(new UpgradeOption
        {
            id = "gold_gain",
            title = "골드 획득 증가",
            description = "획득하는 골드가 20% 증가합니다.",
            maxStacks = 3,
            apply = () => Economy.I.goldMultiplier += 0.2f,
        });

        pool.Add(new UpgradeOption
        {
            id = "build_discount",
            title = "건설 비용 감소",
            description = "벽/함정 건설 비용이 10% 감소합니다.",
            maxStacks = 3,
            apply = () => BuildManager.I.costMultiplier = Mathf.Max(0.3f, BuildManager.I.costMultiplier - 0.1f),
        });

        pool.Add(new UpgradeOption
        {
            id = "castle_heal",
            title = "성벽 보수",
            description = "성 체력을 즉시 5 회복합니다.",
            maxStacks = int.MaxValue, // 성 체력이 깎일 때마다 다시 노출 가능 — canOfferOverride로 실질 제한
            canOfferOverride = () => CastleHealth.I.Hp < CastleHealth.I.maxHp,
            apply = () => CastleHealth.I.Heal(5),
        });

        pool.Add(new UpgradeOption
        {
            id = "spike_slow",
            title = "가시 함정: 슬로우",
            description = "가시 함정을 밟은 적이 3초간 느려집니다.",
            maxStacks = 1,
            canOfferOverride = () => !UpgradeManager.I.SpikeSlowUnlocked,
            apply = () => UpgradeManager.I.UnlockSpikeSlow(),
        });

        pool.Add(new UpgradeOption
        {
            id = "wall_thorn",
            title = "방벽: 가시 반격",
            description = "방벽을 공격한 적에게 반격 피해를 줍니다.",
            maxStacks = 1,
            canOfferOverride = () => !UpgradeManager.I.WallThornUnlocked,
            apply = () => UpgradeManager.I.UnlockWallThorn(),
        });

        pool.Add(new UpgradeOption
        {
            id = "fire_spread",
            title = "유인 함정: 지속 화상",
            description = "유인 함정에 닿으면 화상 피해가 지속됩니다.",
            maxStacks = 1,
            canOfferOverride = () => !UpgradeManager.I.FireSpreadUnlocked,
            apply = () => UpgradeManager.I.UnlockFireSpread(),
        });
    }

    public void ShowChoices(Action onResume)
    {
        pendingResume = onResume;

        var available = pool.FindAll(o => o.CanOffer());
        Shuffle(available);
        int take = Mathf.Min(3, available.Count);
        var picked = available.GetRange(0, take);

        if (picked.Count == 0)
        {
            // 모든 강화가 이미 최대치인 극단적 엣지케이스 — 패널 없이 즉시 재개.
            var resume = pendingResume;
            pendingResume = null;
            resume?.Invoke();
            return;
        }

        Time.timeScale = 0f;
        panelUI.Show(picked, OnPick);
    }

    void OnPick(UpgradeOption option)
    {
        option.Apply();
        panelUI.Hide();
        Time.timeScale = 1f;
        var resume = pendingResume;
        pendingResume = null;
        resume?.Invoke();
    }

    static void Shuffle(List<UpgradeOption> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
