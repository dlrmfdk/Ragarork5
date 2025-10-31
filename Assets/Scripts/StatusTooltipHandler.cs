using UnityEngine;
using UnityEngine.EventSystems; // 마우스 이벤트를 감지하기 위해 필요

public class StatusTooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("툴팁 내용")]
    [Tooltip("툴팁에 표시될 제목 (예: 화상)")]
    [SerializeField] private string tooltipTitle;

    [Tooltip("툴팁에 표시될 설명 내용")]
    [TextArea(3, 5)] // 인스펙터에서 여러 줄로 편하게 입력
    [SerializeField] private string tooltipDescription;

    /// <summary>
    /// 마우스가 이 UI 요소(아이콘) 위로 올라왔을 때 호출됩니다.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // UIManager가 있다면, 이 스크립트에 설정된 제목과 설명으로 툴팁을 띄워달라고 요청
        if (UIManager.Instance != null && !string.IsNullOrEmpty(tooltipDescription))
        {
            UIManager.Instance.ShowSimpleTooltip(tooltipTitle, tooltipDescription);
        }
    }

    /// <summary>
    /// 마우스가 이 UI 요소(아이콘) 밖으로 나갔을 때 호출됩니다.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        // UIManager가 있다면, 툴팁을 숨겨달라고 요청
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideRuneTooltip(); // 기존 툴팁 숨기기 함수 재사용
        }
    }
}