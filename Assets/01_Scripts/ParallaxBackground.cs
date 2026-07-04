using UnityEngine;
using UnityEngine.InputSystem;

// 마우스 위치에 따라 배경이 살짝 움직이는 패럴랙스.
public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] private SpriteRenderer background;

    [Tooltip("최대 이동 거리(월드 단위)")]
    [SerializeField] private float maxOffset = 0.3f;

    [Tooltip("부드러움 (클수록 빠르게 반응)")]
    [SerializeField] private float smoothSpeed = 5f;

    private Vector3 _startPos;
    private Vector2 _currentOffset;

    void Start()
    {
        if (background != null)
            _startPos = background.transform.position;
    }

    void LateUpdate()
    {
        if (background == null) return;
        if (Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        Vector2 mouse = new Vector2(
            (mousePos.x / Screen.width) * 2f - 1f,
            (mousePos.y / Screen.height) * 2f - 1f
        );

        _currentOffset = Vector2.Lerp(_currentOffset, mouse, smoothSpeed * Time.deltaTime);

        background.transform.position = _startPos + new Vector3(
            -_currentOffset.x * maxOffset,
            -_currentOffset.y * maxOffset,
            0f
        );
    }
}