using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager I;
    public int spikeSlowCost = 30;
    public bool SpikeSlowUnlocked { get; private set; }

    void Awake() => I = this;

    public bool TryUnlockSpikeSlow()
    {
        if (SpikeSlowUnlocked) return false;
        if (!Economy.I.TrySpend(spikeSlowCost)) return false;
        SpikeSlowUnlocked = true;
        return true;
    }

    public int wallThornCost = 25;
    public bool WallThornUnlocked { get; private set; }

    public bool TryUnlockWallThorn()
    {
        if (WallThornUnlocked) return false;
        if (!Economy.I.TrySpend(wallThornCost)) return false;
        WallThornUnlocked = true;
        return true;
    }

    public int fireSpreadCost = 25;
    public bool FireSpreadUnlocked { get; private set; }

    public bool TryUnlockFireSpread()
    {
        if (FireSpreadUnlocked) return false;
        if (!Economy.I.TrySpend(fireSpreadCost)) return false;
        FireSpreadUnlocked = true;
        return true;
    }
}
