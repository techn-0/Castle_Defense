using TMPro;
using UnityEngine;

public class BuildPanelUI : MonoBehaviour
{
    public BuildManager buildManager;
    public TMP_Text wallButtonLabel;
    public TMP_Text spikeButtonLabel;

    void Start()
    {
        wallButtonLabel.text = $"벽 ({buildManager.wallCost}G)";
        spikeButtonLabel.text = $"함정 ({buildManager.spikeCost}G)";
    }
}
