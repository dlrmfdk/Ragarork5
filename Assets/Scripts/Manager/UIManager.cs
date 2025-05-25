using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    // UIManager.cs
    void Awake()
    {
        Debug.Log($"[UIManager.Awake] UIManager 게임 오브젝트 '{this.gameObject.name}'의 Awake 호출됨. Instance 설정 시도. 현재 UIManager.Instance는 {(Instance == null ? "null" : Instance.gameObject.name)}");
        Instance = this;
        Debug.Log($"[UIManager.Awake] UIManager.Instance가 '{Instance.gameObject.name}'으로 설정됨.");
    }

    void OnDestroy()
    {
        Debug.Log($"[UIManager.OnDestroy] UIManager 게임 오브젝트 '{this.gameObject.name}'의 OnDestroy 호출됨.");
        if (Instance == this)
        {
            Instance = null;
            Debug.Log("[UIManager.OnDestroy] 현재 Instance가 파괴되는 인스턴스와 동일하여 UIManager.Instance를 null로 설정함.");
        }
    }

    [Header("룬 덱 버튼들")]
    public Button redButton;
    public TextMeshProUGUI redCountText;
    public Button blueButton;
    public TextMeshProUGUI blueCountText;
    public Button whiteButton;
    public TextMeshProUGUI whiteCountText;
    public Button yellowButton;
    public TextMeshProUGUI yellowCountText;

    [Header("중앙 슬롯")]
    public List<Button> slotButtons;      // 5개 슬롯 버튼
    public List<Image> slotIconImages;   // 슬롯 아이콘 표시용

    [Header("뽑기/리롤 버튼")]
    public Button drawButton;
    public Button rerollButton;

    [Header("패널들")]
    public GameObject runeDeckPanel;
    public GameObject centralSlotPanel;

    void Start()
    {
        // 초기에는 UI 숨기고 버튼 비활성화
        runeDeckPanel.SetActive(false);
        centralSlotPanel.SetActive(false);
        drawButton.interactable = false;
        rerollButton.interactable = false;
    }

    /// <summary>
    /// 덱 버튼, 슬롯 버튼, 뽑기 버튼 콜백 바인딩
    /// </summary>
    public void BindRuneDeck(
        Action<RuneColor> onDeckClick,
       
        Action onDrawClick
    )
    {
        redButton.onClick.RemoveAllListeners();
        redButton.onClick.AddListener(() => onDeckClick(RuneColor.Red));

        blueButton.onClick.RemoveAllListeners();
        blueButton.onClick.AddListener(() => onDeckClick(RuneColor.Blue));

        whiteButton.onClick.RemoveAllListeners();
        whiteButton.onClick.AddListener(() => onDeckClick(RuneColor.White));

        yellowButton.onClick.RemoveAllListeners();
        yellowButton.onClick.AddListener(() => onDeckClick(RuneColor.Yellow));

        // Draw버튼에 해당 콜백 연결
        drawButton.onClick.RemoveAllListeners();
        drawButton.onClick.AddListener(() => onDrawClick());
    }

    /// <summary>
    /// 리롤 버튼 콜백 바인딩
    /// </summary>
    public void BindReRoll(Action onReRoll)
    {
        rerollButton.onClick.RemoveAllListeners();
        rerollButton.onClick.AddListener(() => onReRoll());
    }

    /// <summary>
    /// 리롤 버튼 활성화/비활성화
    /// </summary>
    public void SetReRollButton(bool enabled)
    {
        rerollButton.interactable = enabled;
    }

    /// <summary>
    /// 확정 버튼 활성화/비활성화
    /// </summary>
    public void SetDrawButton(bool enabled)
    {
        drawButton.interactable = enabled;
    }

    /// <summary>
    /// 덱/슬롯 패널 보이기
    /// </summary>
    public void ShowRuneUI()
    {
        runeDeckPanel.SetActive(true);
        centralSlotPanel.SetActive(true);
    }

    /// <summary>
    /// 덱/슬롯 패널 숨기기
    /// </summary>
    public void HideRuneUI()
    {
        runeDeckPanel.SetActive(false);
        centralSlotPanel.SetActive(false);
    }

    /// <summary>
    /// 덱에 남은 각 룬 개수 업데이트
    /// </summary>
    public void UpdateDeckCounts(Dictionary<RuneColor, int> counts)
    {
        redCountText.text = counts[RuneColor.Red].ToString();
        blueCountText.text = counts[RuneColor.Blue].ToString();
        whiteCountText.text = counts[RuneColor.White].ToString();
        yellowCountText.text = counts[RuneColor.Yellow].ToString();

    }

    public void UpdateCentralSlotsWithSO(List<RuneSO> selections)
    {
        Debug.Log($"[UpdateCentralSlotsWithSO] 슬롯 아이콘 업데이트 시작. selections 개수: {selections.Count}");
        // 1) slotIconImages는 중앙 슬롯에 배치된 Image 컴포넌트들의 리스트
        for (int i = 0; i < slotIconImages.Count; i++)
        {
            // 2) selections 리스트에서 i번째 RuneSO를 꺼냄
            var so = selections[i];
            if (so != null)
            {
                Debug.Log($"Slot {i}: 룬 {so.name}, 아이콘 {(so.icon == null ? "NULL" : so.icon.name)}");
                // 3) 해당 슬롯에 룬이 있으면
                //   3-1) Image 컴포넌트의 sprite를 RuneSO.icon으로 설정
                slotIconImages[i].sprite = so.icon;
                //   3-2) 이미지 표시(enabled) 켜기
                slotIconImages[i].enabled = true;
            }
            else
            {
                // 4) so가 null이면(빈 슬롯) 
                //   이미지 표시(enabled) 끄기 → 빈 칸으로 보이게 함
                slotIconImages[i].enabled = false;
            }
        }
    }

}
