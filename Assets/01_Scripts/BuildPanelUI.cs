using TMPro;
using UnityEngine;

public class BuildPanelUI : MonoBehaviour
{
    public BuildManager buildManager;
    public TMP_Text wallButtonLabel;
    public TMP_Text spikeButtonLabel;
    public TMP_Text fireTrapButtonLabel;
    public TMP_Text explosiveTrapButtonLabel;

    void Start()
    {
        wallButtonLabel.text = $"벽 ({buildManager.wallCost}G)";
        spikeButtonLabel.text = $"가시 함정 ({buildManager.spikeCost}G)";
        fireTrapButtonLabel.text = $"화염 함정 ({buildManager.fireTrapCost}G)";
        explosiveTrapButtonLabel.text = $"폭탄 ({buildManager.explosiveTrapCost}G)";
    }
}
