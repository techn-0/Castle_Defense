using UnityEngine;

[CreateAssetMenu(fileName = "Wave", menuName = "RCD/Wave")]
public class WaveData : ScriptableObject
{
    [System.Serializable]
    public class SpawnEntry
    {
        public GameObject enemyPrefab;
        public EnemyKind kind;
        public int count = 3;
        public float interval = 0.5f;

        // 0/1이면 프리팹 기본값을 그대로 쓴다. 보스·후반 웨이브 난이도 곡선을
        // 위해 웨이브별로 스탯을 덮어쓸 때만 채운다.
        public int hpOverride = 0;
        public float speedOverride = 0f;
        public int goldOverride = 0;
        public float sizeMultiplier = 1f;
    }

    public SpawnEntry[] entries;

    // 이 웨이브가 목표로 하는 총 스폰 지속 시간(초). 0(미설정)이면 WaveManager가
    // entry들의 count*interval 합으로 자동 폴백한다.
    public float phaseDuration = 45f;
}
