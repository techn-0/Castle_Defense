using UnityEngine;

// UpgradeChoicePanelUI와 동일한 스타일의 단순 표시/숨김 패널.
public class PausePanelUI : MonoBehaviour
{
    public GameObject panelRoot;

    public void Show() => panelRoot.SetActive(true);
    public void Hide() => panelRoot.SetActive(false);

    public void OnClickResume() => PauseManager.I.Resume();
}
