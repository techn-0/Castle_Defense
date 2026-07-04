using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.15f;
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    Vector3 velocity;

    void Start()
    {
        if (target == null && Player.I != null) target = Player.I.transform;
    }

    // Player.Update()에서 이동이 끝난 뒤 카메라가 쫓아가야 튐이 없다.
    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }
}
