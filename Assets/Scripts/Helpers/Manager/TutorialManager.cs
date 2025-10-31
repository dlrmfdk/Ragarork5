using UnityEngine;
using UnityEngine.UI;        // Button 사용
using UnityEngine.EventSystems; // EventTrigger 사용

public class TutorialManager : MonoBehaviour
{
    // --- 싱글톤 설정 ---
    public static TutorialManager Instance { get; private set; }

    [Header("UI 프리팹")]
    [Tooltip("튜토리얼 이미지를 담고 있는 Panel 프리팹")]
    [SerializeField] private GameObject tutorialPanelPrefab;

    private GameObject tutorialPanelInstance; // 생성된 패널 인스턴스

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 게임 전체에서 사용 가능하도록 설정
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    // --- 싱글톤 설정 끝 ---


    /// <summary>
    /// 튜토리얼 이미지를 보여줍니다. (튜토리얼 버튼의 OnClick에 연결)
    /// </summary>
    public void ShowTutorial()
    {
        // ▼▼▼ 로그 추가 ▼▼▼
        Debug.Log("ShowTutorial() 함수 호출됨.");
        Debug.Log($"tutorialPanelPrefab은 null인가? : {(tutorialPanelPrefab == null)}");
        // ▲▲▲

        // 이미 인스턴스가 없으면 프리팹에서 생성
        if (tutorialPanelInstance == null && tutorialPanelPrefab != null)
        {
            tutorialPanelInstance = Instantiate(tutorialPanelPrefab);

            // ▼▼▼ 로그 추가 ▼▼▼
            Debug.Log($"Instantiate 결과 tutorialPanelInstance는 null인가? : {(tutorialPanelInstance == null)}");
            // ▲▲▲


            // 생성된 패널이 최상위 Canvas를 찾도록 설정 (자동으로 찾아짐)
            // 필요하다면 특정 Canvas의 자식으로 지정할 수도 있습니다.
            // Canvas mainCanvas = FindObjectOfType<Canvas>();
            // if (mainCanvas != null)
            // {
            //     tutorialPanelInstance.transform.SetParent(mainCanvas.transform, false);
            // }
            if(tutorialPanelInstance != null) // Instantiate 성공 시
            {
                // ▼▼▼ Canvas 찾아서 자식으로 넣는 로직 ▼▼▼
                // 2. 현재 씬에서 활성화된 Canvas 찾기
                Canvas mainCanvas = FindObjectOfType<Canvas>();

                // 3. Canvas를 찾았다면 자식으로 설정
                if (mainCanvas != null)
                {
                    // SetParent의 두 번째 인자 false는 UI 요소 크기/비율 유지를 위해 중요
                    tutorialPanelInstance.transform.SetParent(mainCanvas.transform, false);
                    Debug.Log($"튜토리얼 패널 인스턴스를 Canvas '{mainCanvas.name}'의 자식으로 설정했습니다.");
                }
                else
                {
                    Debug.LogError("씬에서 Canvas를 찾을 수 없습니다! 튜토리얼 패널을 표시할 수 없습니다.");
                    Destroy(tutorialPanelInstance); // 생성된 인스턴스 제거
                    return; // 함수 종료
                }
                // ▲▲▲ Canvas 자식 설정 로직 끝 ▲▲▲
             }
                // --- 패널 클릭 시 닫히도록 EventTrigger 추가 (코드 방식) ---
                EventTrigger trigger = tutorialPanelInstance.GetComponent<EventTrigger>();
            if (trigger == null) // EventTrigger가 없다면 추가
            {
                trigger = tutorialPanelInstance.AddComponent<EventTrigger>();
            }

            // PointerClick 이벤트 항목 생성 또는 가져오기
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;

            // 클릭 시 HideTutorial 함수를 호출하도록 리스너 추가
            entry.callback.AddListener((eventData) => { HideTutorial(); });

            // EventTrigger에 PointerClick 이벤트 추가
            trigger.triggers.Add(entry);
            // --- EventTrigger 설정 끝 ---

            Debug.Log("튜토리얼 패널 인스턴스 생성 및 설정 완료.");
        }

        // 인스턴스가 존재하면 활성화
        if (tutorialPanelInstance != null)
        {
            tutorialPanelInstance.SetActive(true);
            Debug.Log("튜토리얼 패널 보이기.");
            // (선택) 튜토리얼 보이는 동안 게임 일시정지
            // Time.timeScale = 0f;
        }
        else
        {
            Debug.LogError("튜토리얼 패널 프리팹이 연결되지 않았거나 인스턴스 생성 실패!");
        }
    }

    /// <summary>
    /// 튜토리얼 이미지를 숨깁니다. (패널 자체의 클릭 이벤트에 의해 호출됨)
    /// </summary>
    public void HideTutorial()
    {
        if (tutorialPanelInstance != null)
        {
            tutorialPanelInstance.SetActive(false);
            Debug.Log("튜토리얼 패널 숨기기.");
            // (선택) 게임 재개
            // Time.timeScale = 1f;
        }
    }
}