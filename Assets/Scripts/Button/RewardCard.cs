using UnityEngine;
using UnityEngine.UI;

public class RewardCard : MonoBehaviour
{
    [SerializeField] private Image cardImage;  // 카드 이미지 표시
    [SerializeField] private Text cardNameText;  // 카드 이름 표시

    private Item cardData;
    private System.Action<Item> onSelectedCallback;

    /// <summary>
    /// 보상 카드 버튼을 설정합니다.
    /// 카드의 정보를 전달받아 UI 요소를 갱신합니다.
    /// </summary>
    public void Setup(Item data, System.Action<Item> callback)
    {
        cardData = data;
        onSelectedCallback = callback;
        if (cardImage != null && data.sprite != null)
            cardImage.sprite = data.sprite;
        if (cardNameText != null)
            cardNameText.text = data.name;
    }

    // 버튼 클릭 시 호출되는 메서드 (Button의 OnClick 이벤트에 연결)
    public void OnButtonClicked()
    {
        onSelectedCallback?.Invoke(cardData);
    }
}
