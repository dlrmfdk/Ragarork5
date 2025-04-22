using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuneDeckManager : MonoBehaviour
{
    public static RuneDeckManager Instance { get; private set; }

    [Header("룬 아이콘 매핑")]
    public Sprite redIcon, blueIcon, whiteIcon, yellowIcon;

    // 덱 개수 초기값
    private Dictionary<RuneColor, int> deckCounts = new Dictionary<RuneColor, int>()
    {
        { RuneColor.Red,    10 },
        { RuneColor.Blue,   10 },
        { RuneColor.White,   5 },
        { RuneColor.Yellow, 10 },
    };

    // 중앙 슬롯 (5개)
    private List<RuneColor?> selections = new List<RuneColor?>(new RuneColor?[5]);
    private int selectionCount = 0;

    private Dictionary<RuneColor, Sprite> iconMap;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 아이콘 맵 구성
        iconMap = new Dictionary<RuneColor, Sprite>()
        {
            { RuneColor.Red,    redIcon },
            { RuneColor.Blue,   blueIcon },
            { RuneColor.White,  whiteIcon },
            { RuneColor.Yellow, yellowIcon },
        };
    }

    void Start()
    {
        // UIManager에 콜백 바인딩
        UIManager.Instance.BindRuneDeck(
            OnDeckRuneClicked,
            OnSlotClicked,
            OnDrawClicked);

        // 턴 콜백
        TurnManager.OnTurnStarted += OnTurnStarted;
    }

    void OnDestroy()
    {
        TurnManager.OnTurnStarted -= OnTurnStarted;
    }

    private void OnTurnStarted(bool myTurn)
    {
        if (!myTurn)
        {
            UIManager.Instance.HideRuneUI();
            return;
        }

        // 초기화
        selectionCount = 0;
        for (int i = 0; i < selections.Count; i++)
            selections[i] = null;

        // UI 갱신 & 표시
        UIManager.Instance.UpdateDeckCounts(deckCounts);
        UIManager.Instance.UpdateCentralSlots(selections, iconMap);
        UIManager.Instance.SetDrawButton(false);
        UIManager.Instance.ShowRuneUI();
    }

    private void OnDeckRuneClicked(RuneColor color)
    {
        if (selectionCount >= 5) return;
        if (deckCounts[color] <= 0) return;

        deckCounts[color]--;
        selections[selectionCount] = color;
        selectionCount++;

        // UI 갱신
        UIManager.Instance.UpdateDeckCounts(deckCounts);
        UIManager.Instance.UpdateCentralSlots(selections, iconMap);
        UIManager.Instance.SetDrawButton(selectionCount == 5);
    }

    private void OnSlotClicked(int slotIndex)
    {
        var clr = selections[slotIndex];
        if (!clr.HasValue) return;

        // 선택 취소 → 덱으로 복귀
        deckCounts[clr.Value]++;
        // 슬롯 밀어내기
        for (int i = slotIndex; i < 4; i++)
            selections[i] = selections[i + 1];
        selections[4] = null;
        selectionCount--;

        // UI 갱신
        UIManager.Instance.UpdateDeckCounts(deckCounts);
        UIManager.Instance.UpdateCentralSlots(selections, iconMap);
        UIManager.Instance.SetDrawButton(selectionCount == 5);
    }

    private void OnDrawClicked()
    {
        // TODO: 룬 5개를 사용한 후 처리 로직 추가 (예: 효과 실행, 턴 진행)
        Debug.Log("룬 5개 선택 완료: " +
            string.Join(", ", selections));
        UIManager.Instance.HideRuneUI();
    }
}
