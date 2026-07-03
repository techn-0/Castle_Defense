using UnityEngine;

// 웨이브가 적을 생성할 스폰 위치 표시용 마커. TileGrid가 Awake 시 씬의 모든
// SpawnPoint를 스캔해 스폰 셀 목록을 채운다. 여러 개 두면 다중 스폰 지점이 된다.
public class SpawnPoint : MonoBehaviour
{
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}
