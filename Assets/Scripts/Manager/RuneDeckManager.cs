using System;
using System.Collections.Generic;
using UnityEngine;

public class RuneDeckManager : MonoBehaviour
{
    public static RuneDeckManager Instance { get; private set; }

    [Header("룬 정의 SO 리스트")]
    [Tooltip("에디터에서 생성한 모든 RuneSO 에셋을 할당")]
    [SerializeField] private List<RuneSO> runeDefinitions;

    [Header("덱 상태 저장용 SO")]
    [Tooltip("에디터에서 생성한 RuneDeckDataSO.asset을 할당")]
    [SerializeField] private RuneDeckDataSO deckData;

    // 런타임 덱 카운트: SO별 보유 개수
    private Dictionary<RuneSO, int> deckCounts;

    // 색상별 SO 그룹핑 (랜덤 추출용)
    private Dictionary<RuneColor, List<RuneSO>> runeSOByColor;

    // 중앙 슬롯(5개)에 선택된 룬들
    private List<RuneSO> selections = new List<RuneSO>(new RuneSO[5]);
    private int selectionCount;

    // 턴당 리롤 1회 제한 플래그
    private bool hasRerolledThisTurn;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
         

            // deckData 인스턴스 복제(원본 보호)
            deckData = Instantiate(deckData);

            // 1) 초기 카운트를 SO.initialCount로 세팅
            deckCounts = new Dictionary<RuneSO, int>();
            foreach (var so in runeDefinitions)
                deckCounts[so] = so.initialCount;

            // 2) 저장된 entries 값으로 덮어쓰기
            foreach (var entry in deckData.entries)
            {
                // SO.name 과 entry.runeID 일치시키세요
                var so = runeDefinitions.Find(r => r.name == entry.runeID);
                if (so != null)
                    deckCounts[so] = entry.count;
            }

            // 3) 색상별 SO 리스트 구성
            runeSOByColor = new Dictionary<RuneColor, List<RuneSO>>();
            foreach (var so in runeDefinitions)
            {
                if (!runeSOByColor.ContainsKey(so.color))
                    runeSOByColor[so.color] = new List<RuneSO>();
                runeSOByColor[so.color].Add(so);
            }

            // 4) 턴 시작 이벤트 구독 (Awake에서)
            TurnManager.OnTurnStarted += OnTurnStarted;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // UIManager에 덱/슬롯/드로우/리롤 바인딩
        UIManager.Instance.BindRuneDeck(
            OnDeckRuneClicked,
            /* 슬롯 취소 기능은 제거됨 */ idx => { },
            OnDrawClicked
        );
        UIManager.Instance.BindReRoll(OnRerollClicked);
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

        // 덱 리필: 카운트 0인 룬은 initialCount만큼 재설정
        foreach (var so in runeDefinitions)
        {
            if (deckCounts[so] <= 0)
                deckCounts[so] = so.initialCount;
        }

        // 슬롯 초기화
        selectionCount = 0;
        for (int i = 0; i < selections.Count; i++)
            selections[i] = null;

        hasRerolledThisTurn = false;

        RefreshUI();
        UIManager.Instance.ShowRuneUI();
    }

    private void OnDeckRuneClicked(RuneColor color)
    {
        if (selectionCount >= selections.Count) return;
        if (!runeSOByColor.TryGetValue(color, out var list) || list.Count == 0) return;

        // 같은 색상 중 랜덤 선택
        var chosen = list[UnityEngine.Random.Range(0, list.Count)];
        if (deckCounts[chosen] <= 0) return;

        deckCounts[chosen]--;
        selections[selectionCount++] = chosen;

        RefreshUI();
    }

    private void OnRerollClicked()
    {
        if (hasRerolledThisTurn) return;

        // 슬롯 비우기
        for (int i = 0; i < selections.Count; i++)
            selections[i] = null;
        selectionCount = 0;

        hasRerolledThisTurn = true;
        RefreshUI();
    }

    private void OnDrawClicked()
    {
        // 선택된 룬 효과 실행
        var targets = EnemySpawner.Instance.SpawnedEnemies;
        foreach (var so in selections)
            so?.effectSO.Execute(Player.Instance, targets);

        UIManager.Instance.HideRuneUI();
        TurnManager.Inst.EndTurn();
    }

    /// <summary>
    /// UI 갱신: 덱 카운트, 중앙 슬롯, 버튼 상태
    /// </summary>
    private void RefreshUI()
    {
        // 1) 색상별 남은 개수 집계
        var countsByColor = new Dictionary<RuneColor, int>();
        foreach (RuneColor c in Enum.GetValues(typeof(RuneColor)))
            countsByColor[c] = 0;
        foreach (var kv in deckCounts)
            countsByColor[kv.Key.color] += kv.Value;
        UIManager.Instance.UpdateDeckCounts(countsByColor);

        // 2) 중앙 슬롯 갱신 (List 그대로 전달)
        UIManager.Instance.UpdateCentralSlotsWithSO(selections);

        // 3) 버튼 상태
        bool full = (selectionCount == selections.Count);
        UIManager.Instance.SetDrawButton(full);
        UIManager.Instance.SetReRollButton(full && !hasRerolledThisTurn);
    }

    /// <summary>
    /// 보상 룬으로 기본룬 교체
    /// </summary>
    public void ReplaceBasicRune(RuneSO rewardRune)
    {
        // 같은 색상 & 기본룬(isBasic) 먼저 제거
        foreach (var kv in new Dictionary<RuneSO, int>(deckCounts))
        {
            var so = kv.Key;
            if (so.color == rewardRune.color && so.isBasic && kv.Value > 0)
            {
                deckCounts[so]--;
                break;
            }
        }

        // 보상룬 추가
        if (!deckCounts.ContainsKey(rewardRune))
            deckCounts[rewardRune] = 0;
        deckCounts[rewardRune]++;

        // 저장 데이터 갱신
        deckData.entries.Clear();
        foreach (var kv in deckCounts)
        {
            deckData.entries.Add(new RuneDeckDataSO.Entry
            {
                runeID = kv.Key.name,
                count = kv.Value
            });
        }

        RefreshUI();
        Debug.Log($"[RuneDeck] Replaced with {rewardRune.displayName}");
    }
}
