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
    }
}