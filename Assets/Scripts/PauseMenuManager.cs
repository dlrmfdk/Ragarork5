using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PauseMenuManager : MonoBehaviour
{
    // ▼▼▼ 싱글톤 인스턴스 ▼▼▼
    public static PauseMenuManager Instance { get; private set; }
    // ▲▲▲

    [Header("UI 프리팹")]
    [SerializeField] private GameObject pauseMenuPrefab; // UI 프리팹을 연결할 슬롯

    private GameObject pauseMenuInstance; // 생성된 프리팹 인스턴스를 저장할 변수
    private bool isPaused = false;

    void Awake()
    {
        // ▼▼▼ 싱글톤 패턴 설정 ▼▼▼
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 유지되도록 설정
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // 이미 인스턴스가 있으면 자신을 파괴
            return;
        }
        // ▲▲▲
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        // ▼▼▼ 프리팹 인스턴스 관리 (수정됨) ▼▼▼
        if (pauseMenuInstance == null) // 아직 메뉴가 생성되지 않았다면
        {
            // 1. 프리팹으로부터 인스턴스 생성 (아직 Canvas 자식이 아님)
            pauseMenuInstance = Instantiate(pauseMenuPrefab);

            // 2. 현재 씬에서 Canvas 찾기 (가장 일반적인 방법)
            Canvas mainCanvas = FindObjectOfType<Canvas>();

            // 3. Canvas를 찾았다면, 생성된 메뉴를 Canvas의 자식으로 설정
            if (mainCanvas != null)
            {
                // SetParent의 두 번째 인자 'false'는 UI 요소의 크기/비율 유지를 위해 중요
                pauseMenuInstance.transform.SetParent(mainCanvas.transform, false);
                Debug.Log($"PauseMenu 인스턴스를 Canvas '{mainCanvas.name}'의 자식으로 설정했습니다.");

                // (선택 사항) RectTransform 초기화 - 가끔 부모 설정 후 위치/크기가 이상할 때
                // RectTransform rect = pauseMenuInstance.GetComponent<RectTransform>();
                // if(rect != null)
                // {
                //     rect.anchoredPosition = Vector2.zero; // 중앙 정렬 예시
                //     rect.localScale = Vector3.one;
                // }
            }
            else
            {
                Debug.LogError("씬에서 Canvas를 찾을 수 없습니다! PauseMenu를 표시할 수 없습니다.");
                // Canvas가 없으면 메뉴를 다시 비활성화하거나 파괴하는 게 좋을 수 있음
                if (pauseMenuInstance != null) Destroy(pauseMenuInstance);
                isPaused = false; // 일시정지 상태도 해제
                Time.timeScale = 1f;
                return; // 함수 종료
            }
        }
        pauseMenuInstance.SetActive(true); // 메뉴 보이기
        // ▲▲▲ 수정 완료 ▲▲▲
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuInstance != null)
        {
            pauseMenuInstance.SetActive(false); // 메뉴 숨기기
        }
    }

    /// <summary>
    /// 게임 종료 버튼 클릭 시 호출 (UI 버튼에서 호출)
    /// </summary>
    public void OnClickExitGame() // 함수 이름은 public이어야 버튼에서 호출 가능
    {
        Debug.Log("게임 종료 버튼 클릭됨 (싱글톤 메뉴)");
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}