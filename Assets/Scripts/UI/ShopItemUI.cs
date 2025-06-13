// ShopItemUI.cs 전체 코드 (피드백 기능 추가 버전)
using System.Collections; // 코루틴을 위해 추가
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
    private string originalPriceText; // 원래 가격 텍스트를 저장할 변수

    // ... (이전 코드는 그대로) ...

    public void Setup(RuneSO runeToSell)
    {
        currentRune = runeToSell;
        if (currentRune == null) { /* ... */ return; }

        currentPrice = ShopManager.Instance.GetPriceForRarity(currentRune.rarity);

        runeIconImage.sprite = currentRune.icon;
        runeNameText.text = currentRune.displayName;

        originalPriceText = $"{currentPrice} G"; // 원래 가격 텍스트 저장
        priceText.text = originalPriceText;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyButtonClick);
        buyButton.interactable = true;
    }

    private void OnBuyButtonClick()
    {
        bool success = ShopManager.Instance.AttemptPurchase(currentRune, currentPrice);

        if (success)
        {
            // 구매 성공 시
            buyButton.interactable = false;
            priceText.text = "판매 완료";
        }
        else
        {
            // ▼▼▼ 구매 실패 (골드 부족) 시 피드백 처리 ▼▼▼
            StartCoroutine(ShowTemporaryFeedbackMessage("골드 부족!"));
        }
    }

    // ▼▼▼ 피드백 메시지를 잠시 보여주는 코루틴 ▼▼▼
    private IEnumerator ShowTemporaryFeedbackMessage(string message)
    {
        // 메시지 보여주기
        priceText.text = message;

        // 1.5초간 기다리기
        yield return new WaitForSeconds(1.5f);

        // 원래 가격 텍스트로 되돌리기
        // (만약 그 사이에 아이템이 팔렸으면 "판매 완료" 상태일 수 있으므로 체크)
        if (buyButton.interactable)
        {
            priceText.text = originalPriceText;
        }
    }
}