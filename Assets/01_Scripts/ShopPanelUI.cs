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

    // 이 골드 상점은 UpgradeChoiceManager의 웨이브 클리어 3택 강화로 완전히 대체되었다 —
    // 씬에서 이 패널/버튼을 정리한 뒤 이 스크립트 자체를 삭제해도 된다(현재는 씬의 기존
    // 버튼 OnClick 참조가 깨지지 않도록 컴파일만 유지시켜 둔 상태).
    public void OnClickSpikeSlow() { UpgradeManager.I.UnlockSpikeSlow(); Refresh(); }
    public void OnClickWallThorn() { UpgradeManager.I.UnlockWallThorn(); Refresh(); }
    public void OnClickFireSpread() { UpgradeManager.I.UnlockFireSpread(); Refresh(); }

    void Refresh()
    {
        RefreshOne(spikeSlowButton, spikeSlowButtonLabel, UpgradeManager.I.SpikeSlowUnlocked, "함정 강화: 슬로우");
        RefreshOne(wallThornButton, wallThornButtonLabel, UpgradeManager.I.WallThornUnlocked, "벽 강화: 가시 반격");
        RefreshOne(fireSpreadButton, fireSpreadButtonLabel, UpgradeManager.I.FireSpreadUnlocked, "화염 함정 강화: 화상");
    }

    void RefreshOne(Button button, TMP_Text label, bool unlocked, string title)
    {
        label.text = unlocked ? $"{title} (완료)" : $"{title} (강화 선택 화면에서 획득)";
        button.interactable = !unlocked;
    }
}
