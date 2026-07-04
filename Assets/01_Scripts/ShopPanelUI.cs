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

    // 건축 모드 우클릭 취소(BuildManager)와 동일하게, 상점 패널이 열려 있을 때도
    // 우클릭 한 번으로 즉시 닫히게 한다.
    void Update()
    {
        if (panelRoot.activeSelf && Input.GetMouseButtonDown(1))
            panelRoot.SetActive(false);
    }

    public void TogglePanel() => panelRoot.SetActive(!panelRoot.activeSelf);

    public void OnClickSpikeSlow() { UpgradeManager.I.TryUnlockSpikeSlow(); Refresh(); }
    public void OnClickWallThorn() { UpgradeManager.I.TryUnlockWallThorn(); Refresh(); }
    public void OnClickFireSpread() { UpgradeManager.I.TryUnlockFireSpread(); Refresh(); }

    void Refresh()
    {
        RefreshOne(spikeSlowButton, spikeSlowButtonLabel, UpgradeManager.I.SpikeSlowUnlocked, UpgradeManager.I.spikeSlowCost, "함정 강화: 슬로우");
        RefreshOne(wallThornButton, wallThornButtonLabel, UpgradeManager.I.WallThornUnlocked, UpgradeManager.I.wallThornCost, "벽 강화: 가시 반격");
        RefreshOne(fireSpreadButton, fireSpreadButtonLabel, UpgradeManager.I.FireSpreadUnlocked, UpgradeManager.I.fireSpreadCost, "화염 함정 강화: 화상");
    }

    void RefreshOne(Button button, TMP_Text label, bool unlocked, int cost, string title)
    {
        label.text = unlocked ? $"{title} (완료)" : $"{title} ({cost}G)";
        button.interactable = !unlocked;
    }
}
