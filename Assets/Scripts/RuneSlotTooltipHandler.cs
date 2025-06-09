using UnityEngine;
using UnityEngine.EventSystems; // IPointerEnterHandler, IPointerExitHandler 사용을 위해 추가

public class RuneSlotTooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("이 슬롯의 인덱스 (0부터 4까지)")]
    public int slotIndex;

    // 마우스가 이 UI 요소 위로 들어왔을 때 호출됩니다.
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 플레이어의 턴일 때만 툴팁을 보여줍니다.
        if (TurnManager.Inst != null && TurnManager.Inst.myTurn)
        {
            // RuneDeckManager에서 해당 슬롯의 룬 정보를 가져옵니다.
            RuneSO runeSO = RuneDeckManager.Instance.GetRuneInSelection(slotIndex);
            if (runeSO != null && UIManager.Instance != null)
            {
                UIManager.Instance.ShowRuneTooltip(runeSO);
            }
        }
    }

    // 마우스가 이 UI 요소 위에서 벗어났을 때 호출됩니다.
    public void OnPointerExit(PointerEventData eventData)
    {
        // 마우스가 벗어나면 항상 툴팁을 숨깁니다.
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideRuneTooltip();
        }
    }
}