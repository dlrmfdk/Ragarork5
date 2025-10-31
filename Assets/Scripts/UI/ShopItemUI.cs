// ShopItemUI.cs (수정된 최종 버전)
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private Image runeIconImage;
    [SerializeField] private TMP_Text runeNameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;

    // ▼▼▼ 1. 툴팁 핸들러 참조 변수 추가 ▼▼▼
    [Header("툴팁 핸들러")]
    [Tooltip("이 아이템의 툴팁을 담당할 RuneTooltipHandler 컴포넌트")]
    [SerializeField] private RuneTooltipHandler tooltipHandler;
    // ▲▲▲ 변수 추가 완료 ▲▲▲

    private RuneSO currentRune;
    private int currentPrice;

    public void Setup(RuneSO runeToSell)
    {
        currentRune = runeToSell;
        if (currentRune == null)
        {
            gameObject.SetActive(false);
            return;
        }

        currentPrice = ShopManager.Instance.GetPriceForRarity(currentRune.rarity);

        runeIconImage.sprite = currentRune.icon;
        runeNameText.text = currentRune.displayName;
        priceText.text = $"{currentPrice} G";

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyButtonClick);
        buyButton.interactable = true;

        // ▼▼▼ 2. 툴팁 핸들러에 룬 정보 전달 코드 추가 ▼▼▼
        if (tooltipHandler != null)
        {
            // 이 핸들러가 툴팁을 표시할 룬(SO)이 무엇인지 알려줍니다.
            tooltipHandler.runeSO = currentRune;
        }
        else
        {
            Debug.LogWarning($"ShopItemUI '{currentRune.displayName}'에 tooltipHandler가 연결되지 않았습니다.");
        }
        // ▲▲▲ 코드 추가 완료 ▲▲▲
    }

    private void OnBuyButtonClick()
    {
        // 구매 시도 시, 이 ShopItemUI 컴포넌트 자체를 넘겨줍니다.
        ShopManager.Instance.AttemptPurchase(currentRune, currentPrice, this);
    }

    // 구매가 최종 완료되었을 때 ShopManager가 호출해 줄 함수
    public void MarkAsSold()
    {
        buyButton.interactable = false;
        priceText.text = "판매 완료";

        // ▼▼▼ 3. 판매 완료 시 툴팁 핸들러도 비활성화 (선택적이지만 권장) ▼▼▼
        if (tooltipHandler != null)
        {
            tooltipHandler.enabled = false; // 툴팁 핸들러 스크립트 비활성화
        }
        // ▲▲▲ 코드 추가 완료 ▲▲▲

    }
}