using System.Collections;
using UnityEngine;

// Day 1 테스트용 최소 스폰. Day 3에 WaveManager로 교체될 자리.
public class Spawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Node spawnNode;
    public float interval = 2f;
    public int count = 5;

    void Start() { StartCoroutine(Run()); }

    IEnumerator Run()
    {
        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(enemyPrefab);
            go.GetComponent<Enemy>().currentNode = spawnNode;
            yield return new WaitForSeconds(interval);
        }
    }
}
