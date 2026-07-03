using UnityEngine;

// 성의 위치만 표시하는 마커. TileGrid가 Awake 시 이 오브젝트의 위치로 성 셀을 구한다.
// HP/게임오버 같은 게임 로직은 이후 단계의 별도 Castle 스크립트가 담당한다.
public class CastleMarker : MonoBehaviour
{
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}
