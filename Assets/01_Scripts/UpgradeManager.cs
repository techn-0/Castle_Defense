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
}
