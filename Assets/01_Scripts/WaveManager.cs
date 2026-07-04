using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public WaveData[] waves;
    public float prepTime = 5f;
    public int waveClearBonus = 10;

    TileGrid grid;
    int waveIndex = -1;
    float prepTimer;
    bool waveActive;
    bool spawning;
    bool awaitingChoice; // 3택 강화 패널이 떠 있는 동안 웨이브 진행/Enter 입력을 완전히 차단

    void Awake() { grid = FindFirstObjectByType<TileGrid>(); }
    void Start() { BeginPrep(); }

    public int WaveNumber => waveIndex + 1;
    public int TotalWaves => waves.Length;

    void Update()
    {
        if (!GameManager.I.IsPlaying) return;
        // 없으면 Time.timeScale=0이어도 Input.GetKeyDown(Return)은 계속 감지되어
        // 강화 패널이 떠 있는 동안 StartWave()가 조기 실행되는 버그가 생긴다.
        if (awaitingChoice) return;

        if (!waveActive)
        {
            prepTimer -= Time.deltaTime;
            if (prepTimer <= 0f || Input.GetKeyDown(KeyCode.Return)) StartWave();
            return;
        }

        if (!spawning && Enemy.All.Count == 0)
        {
            Economy.I.AddGold(waveClearBonus);
            waveActive = false;
            if (waveIndex + 1 < waves.Length)
            {
                awaitingChoice = true;
                if (UpgradeChoiceManager.I != null) UpgradeChoiceManager.I.ShowChoices(OnUpgradeChosen);
                else OnUpgradeChosen(); // 씬에 매니저 배치를 깜빡한 경우의 안전장치
            }
            else
            {
                GameManager.I.Victory(); // 마지막 웨이브는 강화 선택 없이 바로 승리
            }
        }
    }

    void OnUpgradeChosen()
    {
        awaitingChoice = false;
        BeginPrep();
    }

    void BeginPrep() { prepTimer = prepTime; }

    void StartWave()
    {
        waveIndex++;
        waveActive = true;
        StartCoroutine(SpawnWave(waves[waveIndex]));
    }

    // 몹 종류를 "남은 수량에 비례한 확률"로 뽑아 고정 간격(spawnInterval)마다 한 마리씩
    // 끊임없이 스폰한다. 매번 완전히 독립적인 확률로 뽑으면 count=1인 보스가 아예 안 나오거나
    // 여러 번 나올 수 있어서, 뽑힌 만큼 그 종류의 남은 수량을 줄여나가는 "복원 없는 가중 추첨"
    // 방식을 쓴다 — 순서는 랜덤하게 섞이면서도 웨이브가 끝날 때까지 각 종류가 정확히
    // authored된 count만큼만 나오는 걸 보장한다.
    IEnumerator SpawnWave(WaveData wave)
    {
        spawning = true;

        int entryCount = wave.entries.Length;
        var remaining = new int[entryCount];
        int totalRemaining = 0;
        for (int i = 0; i < entryCount; i++)
        {
            remaining[i] = wave.entries[i].count;
            totalRemaining += remaining[i];
        }

        while (totalRemaining > 0)
        {
            int pick = Random.Range(0, totalRemaining);
            int idx = 0;
            int acc = remaining[0];
            while (pick >= acc) acc += remaining[++idx];

            SpawnOne(wave.entries[idx]);
            remaining[idx]--;
            totalRemaining--;

            yield return new WaitForSeconds(wave.spawnInterval);
        }

        spawning = false;
    }

    void SpawnOne(WaveData.SpawnEntry entry)
    {
        var spawnCells = new List<Vector3Int>(grid.GetSpawnCells());
        var cell = spawnCells[Random.Range(0, spawnCells.Count)];
        var go = Instantiate(entry.enemyPrefab, grid.CellToWorld(cell), Quaternion.identity);

        var enemy = go.GetComponent<Enemy>();
        if (entry.hpOverride > 0) enemy.maxHp = entry.hpOverride;
        if (entry.speedOverride > 0) enemy.speed = entry.speedOverride;
        if (entry.goldOverride > 0) enemy.goldReward = entry.goldOverride;
        if (entry.sizeMultiplier != 1f) go.transform.localScale *= entry.sizeMultiplier;
    }
}
