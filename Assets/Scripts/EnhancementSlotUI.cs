// EnhancementSlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class EnhancementSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text infoText; // 예: "기본 공격 룬 | 값: 6"
    [SerializeField] private Button selectButton;

    /// <summary>
    /// 이 슬롯에 룬 정보를 설정하고, 클릭했을 때 실행될 행동(콜백)을 등록합니다.
    /// </summary>
    public void Setup(RuneInstance instance, Action<RuneInstance> onSlotSelectedCallback)
    {
        if (instance == null || instance.SO == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // UI 내용 설정
        iconImage.sprite = instance.SO.icon;
        infoText.text = $"{instance.SO.displayName}\n(값: {instance.value})";

        // 버튼 클릭 이벤트 설정
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => onSlotSelectedCallback?.Invoke(instance));
        gameObject.SetActive(true);
    }
}