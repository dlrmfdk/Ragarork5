// RuneTooltipHandler.cs (보상 화면 등 정적인 UI 전용)
using UnityEngine;
using UnityEngine.EventSystems;

public class RuneTooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // 이 핸들러는 정적인 RuneSO 데이터만 직접 할당받습니다.
    public RuneSO runeSO;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (runeSO != null && UIManager.Instance != null)
        {
            // UIManager에게 자신의 UI 위치 정보(RectTransform)를 전달합니다.
            RectTransform anchorTransform = GetComponent<RectTransform>();
            UIManager.Instance.ShowRuneTooltip(runeSO, anchorTransform);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideRuneTooltip();
        }
    }
}