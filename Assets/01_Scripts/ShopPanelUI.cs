using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanelUI : MonoBehaviour
{
    public GameObject panelRoot;
    public Button spikeSlowButton;
    public TMP_Text spikeSlowButtonLabel;

    void Start() => Refresh();

    public void TogglePanel() => panelRoot.SetActive(!panelRoot.activeSelf);

    public void OnClickSpikeSlow()
    {
        UpgradeManager.I.TryUnlockSpikeSlow();
        Refresh();
    }

    void Refresh()
    {
        bool unlocked = UpgradeManager.I.SpikeSlowUnlocked;
        spikeSlowButtonLabel.text = unlocked
            ? "가시 함정 강화: 슬로우 (완료)"
            : $"가시 함정 강화: 슬로우 ({UpgradeManager.I.spikeSlowCost}G)";
        spikeSlowButton.interactable = !unlocked;
    }
}
