using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 웨이브 클리어 시 뜨는 3택 강화 패널. ShopPanelUI/BuildPanelUI와 같은 라벨 갱신 스타일이지만,
// 옵션이 웨이브마다 무작위로 바뀌므로 버튼 리스너를 매번 새로 등록한다.
public class UpgradeChoicePanelUI : MonoBehaviour
{
    public GameObject panelRoot;
    public Button[] optionButtons;
    public TMP_Text[] titleTexts;
    public TMP_Text[] descTexts;

    public void Show(List<UpgradeOption> options, Action<UpgradeOption> onPick)
    {
        panelRoot.SetActive(true);
        for (int i = 0; i < optionButtons.Length; i++)
        {
            bool active = i < options.Count;
            optionButtons[i].gameObject.SetActive(active);
            if (!active) continue;

            var opt = options[i];
            titleTexts[i].text = opt.title;
            descTexts[i].text = opt.description;

            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => onPick(opt));
        }
    }

    public void Hide() => panelRoot.SetActive(false);
}
