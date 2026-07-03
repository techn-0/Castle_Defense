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

    void Awake() { grid = FindFirstObjectByType<TileGrid>(); }
    void Start() { BeginPrep(); }

    void Update()
    {
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
            if (waveIndex + 1 < waves.Length) BeginPrep();
        }
    }

    void BeginPrep() { prepTimer = prepTime; }

    void StartWave()
    {
        waveIndex++;
        waveActive = true;
        StartCoroutine(SpawnWave(waves[waveIndex]));
    }

    IEnumerator SpawnWave(WaveData wave)
    {
        spawning = true;
        foreach (var entry in wave.entries)
        {
            for (int i = 0; i < entry.count; i++)
            {
                SpawnOne(entry);
                yield return new WaitForSeconds(entry.interval);
            }
        }
        spawning = false;
    }

    void SpawnOne(WaveData.SpawnEntry entry)
    {
        var spawnCells = new List<Vector3Int>(grid.GetSpawnCells());
        var cell = spawnCells[Random.Range(0, spawnCells.Count)];
        var go = Instantiate(entry.enemyPrefab, grid.CellToWorld(cell), Quaternion.identity);
        go.GetComponent<Enemy>().kind = entry.kind;
    }
}
