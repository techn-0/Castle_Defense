using UnityEngine;

// 강화 해금 상태만 들고 있는 싱글톤. 예전엔 골드로 직접 사는 상시 상점(ShopPanelUI)이었지만,
// 이제 UpgradeChoiceManager의 웨이브 클리어 3택 강화 풀에서 무료로 선택된다 — 그래서
// 비용 체크 없이 해금 플래그만 세우는 UnlockX()만 남는다. Spike/Wall/FireTrap이 참조하는
// XUnlocked getter는 변경 없음.
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager I;

    public bool SpikeSlowUnlocked { get; private set; }
    public bool WallThornUnlocked { get; private set; }
    public bool FireSpreadUnlocked { get; private set; }

    void Awake() => I = this;

    public void UnlockSpikeSlow()
    {
        SpikeSlowUnlocked = true;
        Spike.RefreshAllSlowTint();
    }
    public void UnlockWallThorn() => WallThornUnlocked = true;
    public void UnlockFireSpread() => FireSpreadUnlocked = true;
}
