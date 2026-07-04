using TMPro;
using UnityEngine;

public class BuildPanelUI : MonoBehaviour
{
    public BuildManager buildManager;
    public TMP_Text wallButtonLabel;
    public TMP_Text spikeButtonLabel;
    public TMP_Text fireTrapButtonLabel;
    public TMP_Text explosiveTrapButtonLabel;
    public TMP_Text demolishButtonLabel;

    void Start()
    {
        if (demolishButtonLabel != null) demolishButtonLabel.text = "철거";
    }

    // 건설 비용 강화(costMultiplier)로 실제 비용이 바뀔 수 있으므로, GameHUD와 동일하게
    // 매 프레임 다시 계산해서 표시한다(정적으로 한 번만 찍으면 강화 후 표기가 갱신되지 않음).
    void Update()
    {
        wallButtonLabel.text = $"벽 ({buildManager.CostFor(BuildMode.Wall)}G)";
        spikeButtonLabel.text = $"가시 함정 ({buildManager.CostFor(BuildMode.Spike)}G)";
        fireTrapButtonLabel.text = $"화염 함정 ({buildManager.CostFor(BuildMode.FireTrap)}G)";
        explosiveTrapButtonLabel.text = $"폭탄 ({buildManager.CostFor(BuildMode.ExplosiveTrap)}G)";
    }
}
