using TMPro;
using UnityEngine;

public class GameHUD : MonoBehaviour
{
    public WaveManager waveManager;
    public TMP_Text goldText;
    public TMP_Text waveText;
    public TMP_Text castleHpText;

    void Update()
    {
        goldText.text = $"{Economy.I.gold}";
        waveText.text = $"{waveManager.WaveNumber}/{waveManager.TotalWaves}";
        castleHpText.text = $"{CastleHealth.I.Hp}/{CastleHealth.I.maxHp}";
    }
}
