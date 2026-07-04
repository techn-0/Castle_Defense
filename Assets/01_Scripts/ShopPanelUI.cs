using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanelUI : MonoBehaviour
{
    public GameObject panelRoot;
    public Button spikeSlowButton;
    public TMP_Text spikeSlowButtonLabel;
    public Button wallThornButton;
    public TMP_Text wallThornButtonLabel;
    public Button fireSpreadButton;
    public TMP_Text fireSpreadButtonLabel;

    void Start() => Refresh();

    public void TogglePanel() => panelRoot.SetActive(!panelRoot.activeSelf);

    public void OnClickSpikeSlow() { UpgradeManager.I.TryUnlockSpikeSlow(); Refresh(); }
    public void OnClickWallThorn() { UpgradeManager.I.TryUnlockWallThorn(); Refresh(); }
    public void OnClickFireSpread() { UpgradeManager.I.TryUnlockFireSpread(); Refresh(); }

    void Refresh()
    {
        RefreshOne(spikeSlowButton, spikeSlowButtonLabel, UpgradeManager.I.SpikeSlowUnlocked, UpgradeManager.I.spikeSlowCost, "가시 함정 강화: 슬로우");
        RefreshOne(wallThornButton, wallThornButtonLabel, UpgradeManager.I.WallThornUnlocked, UpgradeManager.I.wallThornCost, "벽 강화: 가시 반격");
        RefreshOne(fireSpreadButton, fireSpreadButtonLabel, UpgradeManager.I.FireSpreadUnlocked, UpgradeManager.I.fireSpreadCost, "화염 함정 강화: 화상 전이");
    }

    void RefreshOne(Button button, TMP_Text label, bool unlocked, int cost, string title)
    {
        label.text = unlocked ? $"{title} (완료)" : $"{title} ({cost}G)";
        button.interactable = !unlocked;
    }
}
