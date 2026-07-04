using System;

// 강화 하나를 나타내는 순수 데이터 클래스. ScriptableObject가 아니라 코드로 구성한다 —
// apply가 Player/Economy/UpgradeManager 등 다른 싱글톤을 직접 참조하는 델리게이트라
// 직렬화 자산으로 만들 실익이 적다.
public class UpgradeOption
{
    public string id;
    public string title;
    public string description;
    public int maxStacks = 1;
    public int currentStacks;

    public Action apply;
    public Func<bool> canOfferOverride; // null이면 currentStacks < maxStacks만 체크

    public bool CanOffer() => currentStacks < maxStacks && (canOfferOverride == null || canOfferOverride());

    public void Apply()
    {
        apply?.Invoke();
        currentStacks++;
    }
}
