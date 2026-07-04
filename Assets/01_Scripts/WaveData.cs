using UnityEngine;

[CreateAssetMenu(fileName = "Wave", menuName = "RCD/Wave")]
public class WaveData : ScriptableObject
{
    [System.Serializable]
    public class SpawnEntry
    {
        public GameObject enemyPrefab;
        public EnemyKind kind;

        // 이 웨이브에서 이 종류가 총 몇 마리 나올지. 스폰 순서를 정할 때 "남은 수량에 비례한
        // 확률 가중치"로도 함께 쓰인다(WaveManager 참고) — 값이 클수록 자주 뽑히고, 0이 되면
        // 더 이상 뽑히지 않는다.
        public int count = 3;

        // 0/1이면 프리팹 기본값을 그대로 쓴다. 보스·후반 웨이브 난이도 곡선을
        // 위해 웨이브별로 스탯을 덮어쓸 때만 채운다.
        public int hpOverride = 0;
        public float speedOverride = 0f;
        public int goldOverride = 0;
        public float sizeMultiplier = 1f;
    }

    public SpawnEntry[] entries;

    // 몹 한 마리를 스폰한 뒤 다음 한 마리를 스폰하기까지의 고정 대기 시간(초) — 종류와 무관하게
    // 항상 이 간격으로 끊임없이 스폰된다. 웨이브 전체 길이는 대략 (entries의 count 합) * spawnInterval.
    public float spawnInterval = 0.6f;
}
