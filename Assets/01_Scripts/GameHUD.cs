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
        goldText.text = $"Gold: {Economy.I.gold}";
        waveText.text = $"Wave: {waveManager.WaveNumber}/{waveManager.TotalWaves}";
        castleHpText.text = $"Castle HP: {CastleHealth.I.Hp}/{CastleHealth.I.maxHp}";
    }
}
