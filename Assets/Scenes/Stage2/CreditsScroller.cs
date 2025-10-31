using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 씬 전환을 위해 필요

/// <summary>
/// UI 이미지를 아래에서 위로 스크롤하고,
/// 스크롤이 끝나거나 아무 키나 누르면 지정된 씬으로 이동합니다.
/// </summary>
public class CreditsScroller : MonoBehaviour
{
    [Header("스크롤 대상 이미지")]
    [Tooltip("아래에서 위로 스크롤할 크레딧 이미지의 RectTransform")]
    [SerializeField] private RectTransform creditsImageRect;

    [Header("스크롤 설정")]
    [Tooltip("스크롤 속도 (초당 픽셀 이동량)")]
    [SerializeField] private float scrollSpeed = 100f;

    [Tooltip("스크롤이 멈추고 씬이 전환될 Y 위치입니다.\n(이미지의 'Pivot'이 위쪽 끝까지 올라왔을 때의 Pos Y 값)")]
    [SerializeField] private float endYPosition = 4000f; // 씬 에디터에서 이미지의 최종 위치를 확인하고 입력하세요.

    [Header("씬 전환 설정")]
    [Tooltip("스크롤이 끝나거나 스킵할 때 돌아갈 씬 이름 (예: Lobby)")]
    [SerializeField] private string sceneToLoadAfter = "Lobby";

    private bool isScrolling = true; // 현재 스크롤 중인지 확인

    void Start()
    {
        if (creditsImageRect == null)
        {
            Debug.LogError("CreditsImageRect가 연결되지 않았습니다!");
            isScrolling = false;
        }

        // 씬 시작 시 타임스케일이 0이라면 (예: PauseMenu에서 넘어온 경우) 1로 복구
        Time.timeScale = 1f;
    }

    void Update()
    {
        // 1. 스킵 기능: 아무 키나 마우스 버튼을 누르면 즉시 씬 전환
        if (Input.anyKeyDown)
        {
            Debug.Log("크레딧 스킵... 씬 전환.");
            LoadNextScene();
        }

        // 2. 스크롤 로직
        if (isScrolling)
        {
            // 현재 위치가 목표 위치보다 낮은 경우에만 스크롤
            if (creditsImageRect.anchoredPosition.y < endYPosition)
            {
                // Y 위치를 위로(양수 방향) 이동시킴
                creditsImageRect.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);
            }
            else
            {
                // 목표 위치에 도달하면 스크롤 중지 및 씬 전환
                Debug.Log("크레딧 스크롤 완료... 씬 전환.");
                isScrolling = false;
                LoadNextScene();
            }
        }
    }

    /// <summary>
    /// 지정된 씬을 로드합니다.
    /// </summary>
    private void LoadNextScene()
    {
        // 씬 로드 전에 싱글톤 인스턴스 파괴 (필요한 경우)
        // 예: if (GameManager.Inst != null) Destroy(GameManager.Inst.gameObject);
        // 예: if (Player.Instance != null) Destroy(Player.Instance.gameObject);
        // 예: if (RuneDeckManager.Instance != null) Destroy(RuneDeckManager.Instance.gameObject);

        SceneManager.LoadScene(sceneToLoadAfter);
    }
}