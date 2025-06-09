// RuneTooltipHandler.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class RuneTooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // 이 핸들러가 표시해야 할 룬의 데이터를 직접 저장합니다.
    public RuneSO runeSO;

    // 마우스가 이 UI 요소 위로 들어왔을 때 호출됩니다.
    public void OnPointerEnter(PointerEventData eventData)
    {
        // ▼▼▼ UIManager에게 자신의 Transform 정보를 전달하도록 수정 ▼▼▼
        if (runeSO != null && UIManager.Instance != null)
        {
            // 이 컴포넌트가 붙어있는 게임 오브젝트(즉, 버튼 자신)의 RectTransform을 가져옵니다.
            RectTransform anchorTransform = GetComponent<RectTransform>();
            UIManager.Instance.ShowRuneTooltip(runeSO, anchorTransform);
        }
        // ▲▲▲ 수정 완료 ▲▲▲
    }

    // 마우스가 이 UI 요소 위에서 벗어났을 때 호출됩니다.
    public void OnPointerExit(PointerEventData eventData)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideRuneTooltip();
        }
    }
}