using UnityEngine;
using TMPro;
using DG.Tweening;
using System;

public class RewardCard : MonoBehaviour
{
    [Header("Sprite Renderers")]
    [SerializeField] private SpriteRenderer cardBackground;
    [SerializeField] private SpriteRenderer characterImage;

    [Header("TextMeshPro References")]
    [SerializeField] private TMP_Text nameTMP;
    [SerializeField] private TMP_Text attackTMP;
    [SerializeField] private TMP_Text healthTMP; // 필요시

    [Header("Card Assets")]
    [SerializeField] private Sprite cardFront; // 배경 이미지

    public Item item;
    public CardData cardData;

    // 보상 카드 UI에서는 앞면만 사용
    private bool isFront;

    public void Setup(Item item, Action<Item> onCardRewardSelected)
    {
        this.item = item;
        this.isFront = true; // 항상 앞면 표시

        // 배경/캐릭터 스프라이트 설정 (Sorting은 Order.cs에서 관리)
        if (cardBackground && cardFront)
        {
            cardBackground.sprite = cardFront;
        }
        if (characterImage && item.sprite)
        {
            characterImage.sprite = item.sprite;
        }

        // 텍스트 설정 (Sorting은 Order.cs에서 관리)
        if (nameTMP)
        {
            nameTMP.text = item.name;
        }
        if (attackTMP && cardData != null)
        {
            attackTMP.text = cardData.defaultValue.ToString();
        }
        if (healthTMP)
        {
            // 필요 시 healthTMP.text = ...;
        }
    }
    private void OnMouseDown()
    {
        if (isFront)
        {
            // 카드 클릭 시 보상 선택 콜백 호출 (선택된 카드를 RewardManager에서 처리)
            RewardManager.Instance.OnCardRewardSelected(item);

            // 카드 보상이 선택되면 RewardManager의 DisableRewardCards() 메서드를 호출하여
            // 보상 카드 UI 전체(모든 카드 오브젝트 및 패널)를 비활성화함
            RewardManager.Instance.DisableRewardCards();
        }
    }


    public void PlayShowAnimation(float duration)
    {
        // 스케일 0->1 (등장 애니메이션)
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, duration);
    }
}
