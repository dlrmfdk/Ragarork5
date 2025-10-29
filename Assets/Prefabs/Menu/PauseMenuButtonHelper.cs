using UnityEngine;

public class PauseMenuButtonHelper : MonoBehaviour
{
    public void CallExitGame()
    {
        if (PauseMenuManager.Instance != null)
        {
            PauseMenuManager.Instance.OnClickExitGame();
        }
        else
        {
            Debug.LogError("PauseMenuManager 인스턴스를 찾을 수 없습니다!");
        }
    }

    // '계속하기' 버튼용 함수도 필요하다면 여기에 추가
    public void CallResumeGame()
    {
        if (PauseMenuManager.Instance != null)
        {
            PauseMenuManager.Instance.ResumeGame();
        }
    }
}