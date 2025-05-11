using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class RuneRewardUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    private Button button;
    private RuneSO runeSo;
    private Action<RuneSO> onSelected;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Setup(RuneSO so, Action<RuneSO> callback)
    {
        runeSo = so;
        onSelected = callback;
        iconImage.sprite = so.icon;
        nameText.text = so.displayName;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onSelected?.Invoke(runeSo));
    }
}
