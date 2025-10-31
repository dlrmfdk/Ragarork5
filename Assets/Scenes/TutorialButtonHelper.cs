using UnityEngine;

public class TutorialButtonHelper : MonoBehaviour
{
    /// <summary>
    /// 튜토리얼 버튼 클릭 시 호출될 함수입니다.
    /// </summary>
    public void OpenTutorial()
    {
        // ▼▼▼ 이 로그 추가 ▼▼▼
        Debug.Log("OpenTutorial() 함수 호출됨!");
        // ▲▲▲
        // 게임 실행 중에 TutorialManager 싱글톤 인스턴스를 찾아서 함수 호출
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.ShowTutorial();
        }
        else
        {
            // 게임 시작 씬에서 TutorialManager가 제대로 생성되지 않았을 경우
            Debug.LogError("TutorialManager 인스턴스를 찾을 수 없습니다! 게임 시작 씬을 확인하세요.");
        }
    }
}