using UnityEngine;

// 벽/함정 등 파괴 가능한 오브젝트 위에 붙이는 간단한 체력 게이지.
// 스프라이트 에셋 없이 1x1 텍스처를 런타임에 생성해서 쓰므로 별도 리소스가 필요 없다.
// 풀피 상태에서는 숨겨두고, 처음 피격됐을 때만 보이게 한다.
public class HealthBar : MonoBehaviour
{
    static Sprite squareSprite;

    public float width = 0.8f;
    public float height = 0.12f;
    public Vector3 offset = new Vector3(0, 0.6f, 0);
    public Color bgColor = new Color(0, 0, 0, 0.6f);
    public Color fillColor = new Color(0.2f, 0.9f, 0.2f, 1f);

    Transform bg;
    Transform fill;

    void Awake()
    {
        var sprite = GetSquareSprite();

        var bgGO = new GameObject("HPBar_BG");
        bgGO.transform.SetParent(transform, false);
        bgGO.transform.localPosition = offset;
        bgGO.transform.localScale = new Vector3(width, height, 1);
        var bgRenderer = bgGO.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = sprite;
        bgRenderer.color = bgColor;
        bgRenderer.sortingOrder = 20;
        bg = bgGO.transform;

        var fillGO = new GameObject("HPBar_Fill");
        fillGO.transform.SetParent(transform, false);
        var fillRenderer = fillGO.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = sprite;
        fillRenderer.color = fillColor;
        fillRenderer.sortingOrder = 21;
        fill = fillGO.transform;

        bg.gameObject.SetActive(false);
        fill.gameObject.SetActive(false);
    }

    public void SetFraction(float f)
    {
        f = Mathf.Clamp01(f);
        if (!bg.gameObject.activeSelf)
        {
            bg.gameObject.SetActive(true);
            fill.gameObject.SetActive(true);
        }
        fill.localScale = new Vector3(width * f, height, 1);
        fill.localPosition = offset + new Vector3(-width * (1f - f) / 2f, 0, 0);
    }

    static Sprite GetSquareSprite()
    {
        if (squareSprite != null) return squareSprite;
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        squareSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return squareSprite;
    }
}
