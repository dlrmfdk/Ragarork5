// ShopItemUI.cs (새로 만들기)
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private Image runeIconImage;
    [SerializeField] private TMP_Text runeNameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;

    private RuneSO currentRune;
    private int currentPrice;

    // ShopManager가 이 함수를 호출하여 아이템 UI를 설정합니다.
    public void Setup(RuneSO runeToSell)
    {
        currentRune = runeToSell;
        if (currentRune == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // ShopManager로부터 가격 정보 가져오기
        currentPrice = ShopManager.Instance.GetPriceForRarity(currentRune.rarity);

        // UI 업데이트
        runeIconImage.sprite = currentRune.icon;
        runeNameText.text = currentRune.displayName;
        priceText.text = $"{currentPrice} G"; // 예: "100 G"

        // 리스너 초기화 및 설정
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyButtonClick);
        buyButton.interactable = true; // 기본적으로 구매 가능 상태
    }

    private void OnBuyButtonClick()
    {
        // 구매 시도
        bool success = ShopManager.Instance.AttemptPurchase(currentRune, currentPrice);

        // 구매에 성공하면 버튼을 비활성화 (중복 구매 방지)
        if (success)
        {
            buyButton.interactable = false;
            priceText.text = "판매 완료";
        }
    }
}