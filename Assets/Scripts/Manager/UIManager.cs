using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    void Awake() => Instance = this;

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
    public List<Button> slotButtons;           // 5개 슬롯 버튼
    public List<Image> slotIconImages;        // 슬롯 아이콘 표시용

    [Header("뽑기 버튼")]
    public Button drawButton;

    [Header("패널들")]
    public GameObject runeDeckPanel;           // 하단 덱 전체
    public GameObject centralSlotPanel;        // 중앙 슬롯 전체

    void Start()
    {
        // 초기에는 숨김
        runeDeckPanel.SetActive(false);
        centralSlotPanel.SetActive(false);
        drawButton.interactable = false;
    }

    /// <summary>
    /// 클릭 이벤트와 텍스트 업데이트를 위한 연결
    /// </summary>
    public void BindRuneDeck(
        System.Action<RuneColor> onDeckClick, //델리게이트
        System.Action<int> onSlotClick,
        System.Action onDrawClick
    )
    {
        redButton.onClick.AddListener(() => onDeckClick(RuneColor.Red));
        blueButton.onClick.AddListener(() => onDeckClick(RuneColor.Blue));
        whiteButton.onClick.AddListener(() => onDeckClick(RuneColor.White));
        yellowButton.onClick.AddListener(() => onDeckClick(RuneColor.Yellow));

        for (int i = 0; i < slotButtons.Count; i++)
        {
            int idx = i;
            slotButtons[i].onClick.AddListener(() => onSlotClick(idx));
        }

        drawButton.onClick.AddListener(() => onDrawClick());
    }

    /// <summary>
    /// 덱 & 슬롯 패널 모두 보이기/숨기기
    /// </summary>
    public void ShowRuneUI()
    {
        runeDeckPanel.SetActive(true);
        centralSlotPanel.SetActive(true);
    }
    public void HideRuneUI()
    {
        runeDeckPanel.SetActive(false);
        centralSlotPanel.SetActive(false);
    }

    /// <summary>
    /// 남은 덱 개수 갱신
    /// </summary>
    public void UpdateDeckCounts(Dictionary<RuneColor, int> counts)
    {
        redCountText.text = counts[RuneColor.Red].ToString();
        blueCountText.text = counts[RuneColor.Blue].ToString();
        whiteCountText.text = counts[RuneColor.White].ToString();
        yellowCountText.text = counts[RuneColor.Yellow].ToString();
    }

    /// <summary>
    /// 중앙 슬롯 내용 갱신 (null이면 빈 아이콘)
    /// </summary>
    public void UpdateCentralSlots(List<RuneColor?> selections, Dictionary<RuneColor, Sprite> icons)
    {
        for (int i = 0; i < slotIconImages.Count; i++)
        {
            var clr = selections[i];
            slotIconImages[i].sprite = clr.HasValue
                ? icons[clr.Value]
                : null;
            slotIconImages[i].enabled = clr.HasValue;
        }
    }

    /// <summary>
    /// Draw 버튼 활성화/비활성화
    /// </summary>
    public void SetDrawButton(bool enabled)
    {
        drawButton.interactable = enabled;
    }
}
