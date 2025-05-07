using System.Collections.Generic;
using UnityEngine;

public class RuneDeckManager : MonoBehaviour
{
    public static RuneDeckManager Instance { get; private set; }

    [Header("룬 정의 SO 리스트")]
    [Tooltip("에디터에서 모든 RuneSO 에셋을 할당하세요")]
    public List<RuneSO> runeDefinitions;

    // 런타임 덱 카운트 (RuneSO → 남은 개수)
    private Dictionary<RuneSO, int> deckCounts;
    // 색상별 SO 리스트 (랜덤 추출용)
    private Dictionary<RuneColor, List<RuneSO>> runeSOByColor;

    // 중앙 슬롯(최대 5개)에 선택된 RuneSO
    private List<RuneSO> selections = new List<RuneSO>(new RuneSO[5]);
    private int selectionCount = 0;

    // 묘지(사용된 룬 보관용)
    private List<RuneSO> graveyard = new List<RuneSO>();

    // 턴당 리롤 사용 여부
    private bool hasRerolledThisTurn;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // 1) 덱 카운트 초기화
        deckCounts = new Dictionary<RuneSO, int>();
        foreach (var so in runeDefinitions)
            deckCounts[so] = so.initialCount;

        // 2) 색상별 SO 리스트 구성
        runeSOByColor = new Dictionary<RuneColor, List<RuneSO>>();
        foreach (var so in runeDefinitions)
        {
            if (!runeSOByColor.ContainsKey(so.color))
                runeSOByColor[so.color] = new List<RuneSO>();
            runeSOByColor[so.color].Add(so);
        }
    }

    void Start()
    {
        //  콜백 바인딩: 슬롯 취소 기능은 빈 람다로 대체
        UIManager.Instance.BindRuneDeck(
            OnDeckRuneClicked,
            idx => { /* 슬롯 클릭 취소 기능 삭제 */ },
            OnDrawClicked
        );
        UIManager.Instance.BindReRoll(OnReRollClicked);

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

        // 1) 덱 리필
        foreach (var so in runeDefinitions)
            if (deckCounts[so] <= 0)
                deckCounts[so] = so.initialCount;

        // 2) 슬롯 초기화
        selectionCount = 0;
        for (int i = 0; i < selections.Count; i++)
            selections[i] = null;

        // 3) 리롤 플래그 리셋
        hasRerolledThisTurn = false;

        // 4) UI 갱신 및 표시
        RefreshUI();
        UIManager.Instance.ShowRuneUI();
    }

    // 색상 버튼 클릭 → 해당 색상의 SO 리스트에서 랜덤 추출
    private void OnDeckRuneClicked(RuneColor color)
    {
        if (selectionCount >= 5) return;

        var list = runeSOByColor[color];
        if (list == null || list.Count == 0) return;

        // 랜덤으로 하나 골라쓰기
        var chosenSO = list[Random.Range(0, list.Count)];
        if (deckCounts[chosenSO] <= 0) return;

        deckCounts[chosenSO]--;
        selections[selectionCount++] = chosenSO;

        RefreshUI();
    }

    // 리롤 버튼 클릭 → 중앙 슬롯에 있던 모든 룬을 묘지에 보내고 빈 슬롯으로

    private void OnReRollClicked()
    {
        // 이미 리롤했으면 동작 안 함
        if (hasRerolledThisTurn) return;

        // 기존 리롤 로직: 묘지로 보내고 슬롯 비우기
        for (int i = 0; i < selections.Count; i++)
        {
            var so = selections[i];
            if (so != null)
                graveyard.Add(so);
            selections[i] = null;
        }
        selectionCount = 0;

        // 리롤 사용 처리
        hasRerolledThisTurn = true;

        // UI 갱신 (버튼 비활성화 포함)
        RefreshUI();
    }


    // 뽑기 버튼 클릭 → 선택된 룬 효과 실행 후 UI 숨김
    private void OnDrawClicked()
    {
        var targets = EnemySpawner.Instance.SpawnedEnemies;
        foreach (var so in selections)
            so?.effectSO.Execute(Player.Instance, targets);

        UIManager.Instance.HideRuneUI();
    }

    // UI 갱신
    private void RefreshUI()
    {
        // 1) 색상별 남은 개수 합산
        var countsByColor = new Dictionary<RuneColor, int>();
        foreach (var kv in deckCounts)
        {
            var so = kv.Key;
            var cnt = kv.Value;
            if (!countsByColor.ContainsKey(so.color))
                countsByColor[so.color] = 0;
            countsByColor[so.color] += cnt;
        }
        UIManager.Instance.UpdateDeckCounts(countsByColor);

        // 2) 슬롯 아이콘 갱신
        UIManager.Instance.UpdateCentralSlotsWithSO(selections);

        // 3) 버튼 상태
        bool full = (selectionCount == selections.Count);
        UIManager.Instance.SetDrawButton(full);

        // 리롤은 “슬롯이 가득 차고, 아직 리롤 안 했을 때”만 활성화
        UIManager.Instance.SetReRollButton(full && !hasRerolledThisTurn);
    }
}
