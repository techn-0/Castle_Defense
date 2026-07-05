using TMPro;
using UnityEngine;

public class InfoPanelUI : MonoBehaviour
{
    public TMP_Text bodyText;
    public TMP_Text pageLabel;
    [TextArea(3, 10)]
    public string[] pages;

    private int pageIndex;

    public void Show()
    {
        gameObject.SetActive(true);
        pageIndex = 0;
        RefreshPage();
    }

    public void Hide() => gameObject.SetActive(false);

    public void NextPage()
    {
        pageIndex = (pageIndex + 1) % pages.Length;
        RefreshPage();
    }

    public void PrevPage()
    {
        pageIndex = (pageIndex - 1 + pages.Length) % pages.Length;
        RefreshPage();
    }

    private void RefreshPage()
    {
        if (pages.Length == 0) return;
        bodyText.text = pages[pageIndex];
        if (pageLabel != null)
            pageLabel.text = $"{pageIndex + 1} / {pages.Length}";
    }
}
