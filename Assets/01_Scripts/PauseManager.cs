using UnityEngine;

// ESC로 일시정지 패널을 여닫는 싱글톤(GameManager와 동일 패턴).
// GameOver/Victory 화면이 떠 있거나(IsPlaying == false), 강화 선택 패널처럼 우리가 아닌
// 다른 곳에서 이미 Time.timeScale을 0으로 만든 상태라면 개입하지 않는다.
public class PauseManager : MonoBehaviour
{
    public static PauseManager I;
    public PausePanelUI panelUI;

    public bool IsPaused { get; private set; }

    void Awake() => I = this;

    // BuildManager의 Update()가 Escape로 건축 모드를 취소한 뒤에 판단해야 같은 키 입력으로
    // 건축 취소와 일시정지가 동시에 터지지 않는다 — LateUpdate는 이번 프레임의 모든 Update
    // 이후 실행되는 게 보장되므로 실행 순서에 의존하지 않고 이를 확인할 수 있다.
    void LateUpdate()
    {
        if (!GameManager.I.IsPlaying) return;
        if (Time.timeScale == 0f && !IsPaused) return;
        if (BuildManager.I != null && BuildManager.I.ConsumedEscape) return;

        if (Input.GetKeyDown(KeyCode.Escape)) Toggle();
    }

    public void Toggle()
    {
        if (IsPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        if (IsPaused) return;
        IsPaused = true;
        Time.timeScale = 0f;
        panelUI.Show();
    }

    public void Resume()
    {
        if (!IsPaused) return;
        IsPaused = false;
        Time.timeScale = 1f;
        panelUI.Hide();
    }
}
