using System.Collections;
using Febucci.TextAnimatorForUnity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Intro 씬 전용 컷신 시퀀서. 트럭 통과 -> 화이트/블랙 페이드 -> 스플릿 오픈 리빌 -> 자막 -> 다음 씬 순서로 재생.
// 스킵 버튼/ESC 키 모두 SkipToEnd()를 통해 동일한 GoToNextScene() 경로를 탄다.
public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager I;

    [Header("트럭 연출 (플레이스홀더 Transform 연결)")]
    public Transform truck;
    public Transform truckStartPoint; // 빈 오브젝트를 씬에 배치해 시작 위치로 연결
    public Transform truckEndPoint;   // 빈 오브젝트를 씬에 배치해 도착 위치로 연결
    public float truckSpeed = 8f;
    public AudioSource truckAudioSource; // 트럭 오브젝트에 붙은 AudioSource(3D Sound Settings, Spatial Blend=1) 연결
    public AudioClip truckSfx; // 비워두면 재생 안 함. Assets/Resources/Sounds/Truck_skid_1.wav 연결 가능

    [Range(0f, 1f)]
    [Tooltip("트럭이 시작~도착 지점을 이동하는 동안 몇 % 진행됐을 때 화이트 플래시를 시작할지 (0 = 출발 즉시, 1 = 도착 직후)")]
    public float whiteFlashTriggerProgress = 0.5f;

    [Header("트럭씬 오브젝트 (화이트 플래시가 화면을 완전히 덮는 순간 자동 비활성화)")]
    public GameObject[] truckSceneObjects; // 예: StudentPlaceholder, TruckPlaceholder, 배경 등

    [Header("화이트/블랙 페이드")]
    public Image fadeImage;
    public float whiteFlashDuration = 0.15f;
    public float blackHoldDuration = 0.6f;

    [Header("스플릿 오픈 리빌 (씬 시작 시 비활성 상태로 두고, 리빌 순간에 이 스크립트가 활성화한다)")]
    public RectTransform splitPanelTop;
    public RectTransform splitPanelBottom;
    public GameObject fortressBackground;
    public float splitOpenDuration = 0.8f;
    public float splitDistance = 600f;

    [Header("자막")]
    public TMP_Text subtitleText;
    public string[] subtitleLines;
    public float[] subtitleDurations;

    [Header("자막 타이핑 효과 (Text Animator for Unity by Febucci, 선택)")]
    [Tooltip("SubtitleText 오브젝트에 TypewriterComponent를 붙여서 연결하면 타이핑 효과로 출력된다. 비워두면 텍스트가 즉시 표시된다.")]
    public TypewriterComponent subtitleTypewriter;

    [Header("트럭 출발 전 자막 (입력 대기)")]
    [Tooltip("비워두면 이 단계를 건너뛴다. 자막이 다 표시된 뒤 마우스 클릭/아무 키 입력을 받아야 트럭이 출발한다.")]
    public string preTruckSubtitleLine;

    [Header("전환")]
    public string nextSceneName = "SampleScene";

    bool exiting;

    void Awake() { I = this; }

    void Start()
    {
        StartCoroutine(PlaySequence());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) SkipToEnd();
    }

    IEnumerator PlaySequence()
    {
        // TypewriterComponent 등 씬의 다른 컴포넌트들이 전부 Start()를 마칠 때까지 한 프레임 기다린다.
        // (Start() 호출 순서는 오브젝트 간에 보장되지 않아서, 같은 프레임에 바로 ShowText()를 호출하면
        //  타이핑 효과 쪽이 아직 초기화되지 않은 상태일 수 있다.)
        yield return null;

        yield return StartCoroutine(ShowPreTruckSubtitle());

        float truckDuration = ComputeTruckDuration();

        // 트럭 이동과 화이트 플래시를 동시에 진행시키되, whiteFlashTriggerProgress로 플래시 시작 시점만 지연시킨다.
        StartCoroutine(TruckPass(truckDuration));
        yield return new WaitForSeconds(truckDuration * whiteFlashTriggerProgress);

        yield return StartCoroutine(WhiteFlashThenBlack());
        yield return StartCoroutine(SplitOpenReveal());
        yield return StartCoroutine(PlaySubtitles());
        GoToNextScene();
    }

    // 트럭이 출발하기 전 자막 한 줄을 보여주고, 다 타이핑된 뒤 클릭/아무 키 입력이 들어와야 다음(트럭 출발)으로 넘어간다.
    IEnumerator ShowPreTruckSubtitle()
    {
        if (string.IsNullOrEmpty(preTruckSubtitleLine)) yield break;

        bool lineFinishedTyping = subtitleTypewriter == null; // 타이핑 효과가 없으면 즉시 완료로 취급
        UnityAction onShown = () => lineFinishedTyping = true;
        if (subtitleTypewriter != null) subtitleTypewriter.onTextShowed.AddListener(onShown);

        DisplayLine(preTruckSubtitleLine);

        // onTextShowed가 어떤 이유로든 오지 않는 경우를 대비한 안전장치(타임아웃) - 컷신이 영원히 멈추는 것을 방지한다.
        const float typingSafetyTimeout = 5f;
        float elapsed = 0f;
        while (!lineFinishedTyping && elapsed < typingSafetyTimeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (!lineFinishedTyping)
            Debug.LogWarning("[CutsceneManager] preTruckSubtitleLine의 타이핑 완료 이벤트(onTextShowed)를 받지 못해 타임아웃으로 넘어갑니다. SubtitleText의 TypewriterComponent 설정을 확인하세요.");

        if (subtitleTypewriter != null) subtitleTypewriter.onTextShowed.RemoveListener(onShown);

        yield return StartCoroutine(WaitForAdvanceInput());
        DisplayLine("");
    }

    IEnumerator WaitForAdvanceInput()
    {
        // MainMenu의 "게임 시작" 클릭이 씬 전환 프레임까지 눌린 상태로 남아있을 수 있으므로,
        // 일단 완전히 떼어질 때까지 기다린 다음부터 새로운 입력(클릭/키)을 받는다.
        while (Input.anyKey)
            yield return null;

        while (!Input.anyKeyDown)
            yield return null;
    }

    // subtitleTypewriter가 연결되어 있으면 타이핑 효과로, 아니면 텍스트를 즉시 표시한다.
    void DisplayLine(string line)
    {
        if (subtitleTypewriter != null) subtitleTypewriter.ShowText(line);
        else if (subtitleText != null) subtitleText.text = line;
    }

    float ComputeTruckDuration()
    {
        if (truckStartPoint == null || truckEndPoint == null) return 0f;
        return Vector3.Distance(truckStartPoint.position, truckEndPoint.position) / Mathf.Max(0.01f, truckSpeed);
    }

    IEnumerator TruckPass(float duration)
    {
        if (truck == null || truckStartPoint == null || truckEndPoint == null) yield break;

        Vector3 start = truckStartPoint.position;
        Vector3 end = truckEndPoint.position;
        truck.position = start;

        // 트럭에 붙은 AudioSource로 재생 - 트럭을 따라 위치가 갱신되므로 3D 사운드(좌우 패닝/거리 감쇠)가 자연스럽게 적용된다.
        if (truckAudioSource != null && truckSfx != null)
        {
            truckAudioSource.clip = truckSfx;
            truckAudioSource.Play();
        }

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            truck.position = Vector3.Lerp(start, end, t / duration);
            yield return null;
        }
        truck.position = end;
    }

    IEnumerator WhiteFlashThenBlack()
    {
        if (fadeImage == null) yield break;
        fadeImage.gameObject.SetActive(true);

        for (float t = 0f; t < whiteFlashDuration; t += Time.deltaTime)
        {
            fadeImage.color = Color.Lerp(new Color(1f, 1f, 1f, 0f), Color.white, t / whiteFlashDuration);
            yield return null;
        }
        fadeImage.color = Color.white;

        // 흰 화면이 완전히 덮은 시점 - 트럭씬 오브젝트(학생/트럭/배경)를 감춰도 티가 나지 않는다.
        HideTruckSceneObjects();

        for (float t = 0f; t < whiteFlashDuration; t += Time.deltaTime)
        {
            fadeImage.color = Color.Lerp(Color.white, Color.black, t / whiteFlashDuration);
            yield return null;
        }
        fadeImage.color = Color.black;

        yield return new WaitForSeconds(blackHoldDuration);
    }

    void HideTruckSceneObjects()
    {
        if (truckSceneObjects == null) return;
        foreach (var go in truckSceneObjects)
            if (go != null) go.SetActive(false);
    }

    IEnumerator SplitOpenReveal()
    {
        // 검은 fadeImage가 화면을 덮고 있는 동안 스플릿 패널을 "닫힌"(전체 화면 덮음) 상태로 먼저 켜고,
        // 같은 프레임 안에서 fadeImage를 끄기 때문에 화면상 끊김 없이 자연스럽게 이어진다.
        if (splitPanelTop != null) splitPanelTop.gameObject.SetActive(true);
        if (splitPanelBottom != null) splitPanelBottom.gameObject.SetActive(true);
        if (fortressBackground != null) fortressBackground.SetActive(true);
        if (fadeImage != null) fadeImage.gameObject.SetActive(false);

        if (splitPanelTop == null || splitPanelBottom == null) yield break;

        Vector2 topStart = splitPanelTop.anchoredPosition;
        Vector2 bottomStart = splitPanelBottom.anchoredPosition;
        Vector2 topEnd = topStart + new Vector2(0f, splitDistance);
        Vector2 bottomEnd = bottomStart - new Vector2(0f, splitDistance);

        for (float t = 0f; t < splitOpenDuration; t += Time.deltaTime)
        {
            float k = t / splitOpenDuration;
            splitPanelTop.anchoredPosition = Vector2.Lerp(topStart, topEnd, k);
            splitPanelBottom.anchoredPosition = Vector2.Lerp(bottomStart, bottomEnd, k);
            yield return null;
        }
        splitPanelTop.anchoredPosition = topEnd;
        splitPanelBottom.anchoredPosition = bottomEnd;
    }

    IEnumerator PlaySubtitles()
    {
        if (subtitleLines == null) yield break;

        for (int i = 0; i < subtitleLines.Length; i++)
        {
            DisplayLine(subtitleLines[i]);
            float duration = (subtitleDurations != null && i < subtitleDurations.Length) ? subtitleDurations[i] : 2f;
            yield return new WaitForSeconds(duration);
        }
        DisplayLine("");
    }

    public void SkipToEnd()
    {
        if (exiting) return;
        StopAllCoroutines();
        GoToNextScene();
    }

    void GoToNextScene()
    {
        if (exiting) return;
        exiting = true;
        SceneManager.LoadScene(nextSceneName);
    }
}
