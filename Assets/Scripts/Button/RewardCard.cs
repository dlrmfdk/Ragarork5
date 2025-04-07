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

    [Header("Sorting Setup")]
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int backgroundOrder = 0; // 배경
    [SerializeField] private int characterOrder = 1;  // 캐릭터
    [SerializeField] private int textOrder = 2;       // 텍스트들

    [Header("Card Assets")]
    [SerializeField] private Sprite cardFront; // 배경 이미지
    public Item item;
    public CardData cardData;

    private bool isFront;

    public void Setup(Item item, Action<Item> onCardRewardSelected)
    {
        this.item = item;
        this.isFront = true;

        // 배경/캐릭터 설정
        if (cardBackground && cardFront)
        {
            cardBackground.sprite = cardFront;
            cardBackground.sortingLayerName = sortingLayerName;
            cardBackground.sortingOrder = backgroundOrder;
        }
        if (characterImage && item.sprite)
        {
            characterImage.sprite = item.sprite;
            characterImage.sortingLayerName = sortingLayerName;
            characterImage.sortingOrder = characterOrder;
        }

        // 텍스트들 설정
        if (nameTMP)
        {
            nameTMP.text = item.name;
            var mr = nameTMP.GetComponent<MeshRenderer>();
            if (mr)
            {
                mr.sortingLayerName = sortingLayerName;
                mr.sortingOrder = textOrder;
            }
        }
        if (attackTMP && cardData != null)
        {
            attackTMP.text = cardData.defaultValue.ToString();
            var mr = attackTMP.GetComponent<MeshRenderer>();
            if (mr)
            {
                mr.sortingLayerName = sortingLayerName;
                mr.sortingOrder = textOrder;
            }
        }
        if (healthTMP)
        {
            // 필요시 healthTMP.text = ...;
            var mr = healthTMP.GetComponent<MeshRenderer>();
            if (mr)
            {
                mr.sortingLayerName = sortingLayerName;
                mr.sortingOrder = textOrder;
            }
        }

        // 클릭 시 콜백
        // (OnMouseDown()으로 처리할 거면 아래 생략)
        // Button button = GetComponent<Button>();
        // if (button != null)
        // {
        //     button.onClick.RemoveAllListeners();
        //     button.onClick.AddListener(() => onCardRewardSelected(item));
        // }
    }

    public void PlayShowAnimation(float duration)
    {
        // 스케일 0->1
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, duration);
    }
}
