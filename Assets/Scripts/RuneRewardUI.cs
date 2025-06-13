// RuneRewardUI.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class RuneRewardUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText; // 설명을 표시할 텍스트. 없다면 새로 추가해주세요.
    private Button button;

    // CHANGED: RuneSO 대신 RuneInstance를 받도록 변경
    private RuneInstance runeInstance;
    private Action<RuneInstance> onSelected;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    // CHANGED: Setup 메서드의 파라미터를 RuneInstance로 변경
    public void Setup(RuneInstance instance, Action<RuneInstance> callback)
    {
        this.runeInstance = instance;
        this.onSelected = callback;

        if (instance == null || instance.SO == null) return;

        // 아이콘과 이름은 SO에서 가져옵니다.
        iconImage.sprite = instance.SO.icon;
        nameText.text = instance.SO.displayName;

        // 설명은 SO의 템플릿과 instance의 고유 수치를 조합하여 만듭니다.
        if (descriptionText != null)
        {
            string formattedDesc = instance.SO.description.Replace("{value}", instance.value.ToString());
            descriptionText.text = formattedDesc;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onSelected?.Invoke(runeInstance));
    }
}