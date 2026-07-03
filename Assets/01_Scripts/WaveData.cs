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
    }

    public SpawnEntry[] entries;
}
