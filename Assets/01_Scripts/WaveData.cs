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
}
