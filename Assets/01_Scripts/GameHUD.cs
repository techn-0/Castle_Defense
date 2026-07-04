using TMPro;
using UnityEngine;

public class GameHUD : MonoBehaviour
{
    public WaveManager waveManager;
    public TMP_Text goldText;
    public TMP_Text waveText;
    public TMP_Text castleHpText;
    public TMP_Text nextWaveText;

    void Update()
    {
        goldText.text = $"{Economy.I.gold}";
        waveText.text = $"{waveManager.WaveNumber}/{waveManager.TotalWaves}";
        castleHpText.text = $"{CastleHealth.I.Hp}/{CastleHealth.I.maxHp}";

        if (nextWaveText != null)
        {
            nextWaveText.text = waveManager.WaveActive
                ? " "
                : $"다음 웨이브: {Mathf.CeilToInt(waveManager.PrepTimeRemaining)}초";
        }
    }
}
