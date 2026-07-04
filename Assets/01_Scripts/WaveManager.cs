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

    // 근접/원거리/닌자 등 entry들이 블록으로 몰리지 않고 섞여 나오도록, entry별 개별 타이머를 두고
    // 매 프레임 라운드로빈으로 스폰한다. entry.interval은 "최소 간격 하한"으로 재해석되고,
    // 실제 간격은 phaseDuration에 걸쳐 고르게 분산되도록 계산한다.
    IEnumerator SpawnWave(WaveData wave)
    {
        spawning = true;

        int entryCount = wave.entries.Length;
        if (entryCount == 0) { spawning = false; yield break; }

        int totalCount = 0;
        foreach (var e in wave.entries) totalCount += e.count;

        float duration = wave.phaseDuration > 0f ? wave.phaseDuration : EstimateFallbackDuration(wave);

        var remaining = new int[entryCount];
        var gaps = new float[entryCount];
        var timers = new float[entryCount];
        for (int i = 0; i < entryCount; i++)
        {
            remaining[i] = wave.entries[i].count;
            float evenGap = totalCount > 0 ? duration / totalCount : wave.entries[i].interval;
            gaps[i] = Mathf.Max(wave.entries[i].interval, evenGap);
            timers[i] = 0f;
        }

        int aliveEntries = entryCount;
        while (aliveEntries > 0)
        {
            for (int i = 0; i < entryCount; i++)
            {
                if (remaining[i] <= 0) continue;
                timers[i] -= Time.deltaTime;
                if (timers[i] <= 0f)
                {
                    SpawnOne(wave.entries[i]);
                    remaining[i]--;
                    timers[i] = gaps[i];
                    if (remaining[i] <= 0) aliveEntries--;
                }
            }
            yield return null;
        }

        spawning = false;
    }

    float EstimateFallbackDuration(WaveData wave)
    {
        float sum = 0f;
        foreach (var e in wave.entries) sum += e.count * e.interval;
        return sum;
    }

    void SpawnOne(WaveData.SpawnEntry entry)
    {
        var spawnCells = new List<Vector3Int>(grid.GetSpawnCells());
        var cell = spawnCells[Random.Range(0, spawnCells.Count)];
        var go = Instantiate(entry.enemyPrefab, grid.CellToWorld(cell), Quaternion.identity);

        var enemy = go.GetComponent<Enemy>();
        enemy.kind = entry.kind;
        if (entry.hpOverride > 0) enemy.maxHp = entry.hpOverride;
        if (entry.speedOverride > 0) enemy.speed = entry.speedOverride;
        if (entry.goldOverride > 0) enemy.goldReward = entry.goldOverride;
        if (entry.sizeMultiplier != 1f) go.transform.localScale *= entry.sizeMultiplier;
    }
}
