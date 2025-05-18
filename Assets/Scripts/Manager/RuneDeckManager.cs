using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// 덱 및 묘지 관리를 담당하며, JSON 파일로 상태를 저장/로드합니다.
/// </summary>
public class RuneDeckManager : MonoBehaviour
{
    public static RuneDeckManager Instance { get; private set; }

    [Header("룬 정의 SO 리스트")]
    public List<RuneSO> runeDefinitions;

    // 덱 상태: 각 RuneSO별 보유 개수
    private Dictionary<RuneSO, int> deckCounts;
    // 사용된 룬(묘지)
    private List<RuneSO> discardPile;
    // 색상별 룬 그룹
    private Dictionary<RuneColor, List<RuneSO>> runeSOByColor;

    // 중앙 슬롯 (최대 5개)
    private List<RuneSO> selections = new List<RuneSO>(new RuneSO[5]);
    private int selectionCount = 0;
    private bool hasRerolledThisTurn;

    // JSON 저장 경로
    private string deckStatePath => Path.Combine(Application.persistentDataPath, "DeckState.json");

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 덱/묘지/그룹 초기화
        deckCounts = new Dictionary<RuneSO, int>();
        discardPile = new List<RuneSO>();
        runeSOByColor = new Dictionary<RuneColor, List<RuneSO>>();
        foreach (RuneColor c in Enum.GetValues(typeof(RuneColor)))
            runeSOByColor[c] = new List<RuneSO>();
        foreach (var so in runeDefinitions)
        {
            deckCounts[so] = so.initialCount;
            runeSOByColor[so.color].Add(so);
        }
        foreach (var kv in deckCounts)
            Debug.Log($"[Awake] {kv.Key.name}: {kv.Value}");
     ;
        // 이전 세이브 있으면 덱만 복원 (묘지는 새 전투마다 비워짐)
        LoadDeckState();
    }

    void Start()
    {
        // UI 이벤트 바인딩
        UIManager.Instance.BindRuneDeck(OnDeckClick, OnSlotClick, OnDrawClick);
        UIManager.Instance.BindReRoll(OnReRoll);
        RefreshUI();
    }

    /// <summary>
    /// 덱 버튼 클릭: 슬롯에 룬 추가 (덱이 비어 있을 경우 묘지에서 재생성)
    /// </summary>
    private void OnDeckClick(RuneColor color)
    {
        // 1) 이 색상의 덱이 전부 비어 있는지 확인
        var group = runeSOByColor[color];
        bool isColorEmpty = group.All(so => deckCounts[so] == 0);
        bool hasInDiscard = discardPile.Any(so => so.color == color);

        if (isColorEmpty && hasInDiscard)
        {
            // 2) 묘지에서 같은 색상 룬들만 골라 덱으로 복원
            var toRestore = discardPile.Where(so => so.color == color).ToList();
            foreach (var so in toRestore)
            {
                deckCounts[so] = deckCounts.ContainsKey(so) ? deckCounts[so] + 1 : 1;
                discardPile.Remove(so);
            }
        }

        // 3) 이제 남은 수량이 있는 룬만 뽑을 수 있게…
        if (selectionCount >= selections.Count) return;
        var available = group.Where(so => deckCounts[so] > 0).ToList();
        if (available.Count == 0) return;

        var chosen = available[UnityEngine.Random.Range(0, available.Count)];
        deckCounts[chosen]--;
        selections[selectionCount++] = chosen;

        RefreshUI();
    }


    /// <summary>슬롯 클릭: 효과 발동 및 묘지로 이동</summary>
    private void OnSlotClick(int index)
    {
        if (index < 0 || index >= selectionCount) return;
        var so = selections[index];
        // 효과 실행 (예: Execute(user, targets))
        var user = Player.Instance;
        var targets = EnemySpawner.Instance.SpawnedEnemies;
        so.effectSO.Execute(user, targets);

        // 묘지에 추가
        discardPile.Add(so);

        // 슬롯 제거
        for (int i = index; i < selectionCount - 1; i++)
            selections[i] = selections[i + 1];
        selections[--selectionCount] = null;

       
        RefreshUI();
    }

    /// <summary>확정 클릭: 남은 룬 반환 및 슬롯 초기화</summary>
    private void OnDrawClick()
    {
        for (int i = 0; i < selectionCount; i++)
            deckCounts[selections[i]]++;
        selections = new List<RuneSO>(new RuneSO[5]);
        selectionCount = 0;
        hasRerolledThisTurn = false;
        //SaveDeckState();
        RefreshUI();
    }

    /// <summary>리롤 클릭: 슬롯 전체 반환 후 재드로우 X</summary>
    private void OnReRoll()
    {
        if (hasRerolledThisTurn) return;
        for (int i = 0; i < selectionCount; i++)
            deckCounts[selections[i]]++;
        selections = new List<RuneSO>(new RuneSO[5]);
        selectionCount = 0;
        hasRerolledThisTurn = true;
       
        RefreshUI();
    }

    //보상룬 획득 시 기본룬 1개 교체
    public void ReplaceBasicWithReward(string rewardRuneID)
    {
        // JSON(또는 SO 리스트)에서 보상룬 SO 찾기
        var rewardSO = runeDefinitions
            .FirstOrDefault(r => r.name == rewardRuneID);
        if (rewardSO == null)
        {
            Debug.LogError($"룬 데이터베이스에 '{rewardRuneID}'가 없습니다.");
            return;
        }

        // 같은 색상의 기본룬 찾기
        var basicSO = deckCounts.Keys
            .FirstOrDefault(so =>
                so.color == rewardSO.color &&
                so.name.Contains("Basic") &&
                deckCounts[so] > 0);
        if (basicSO == null)
        {
            Debug.LogWarning($"교체할 기본 {rewardSO.color} 룬이 없습니다.");
            return;
        }

        // 덱 카운트 업데이트
        deckCounts[basicSO]--;
        if (deckCounts[basicSO] == 0)
            deckCounts.Remove(basicSO);
        if (!deckCounts.ContainsKey(rewardSO))
            deckCounts[rewardSO] = 0;
        deckCounts[rewardSO]++;

        // UI 갱신
        RefreshUI();
    }

    /// <summary>UI 갱신: 덱 상태, 슬롯, 버튼</summary>
    public void RefreshUI()
    {
        var countsByColor = new Dictionary<RuneColor, int>();
        foreach (RuneColor c in Enum.GetValues(typeof(RuneColor))) countsByColor[c] = 0;
        foreach (var kv in deckCounts) countsByColor[kv.Key.color] += kv.Value;
        UIManager.Instance.UpdateDeckCounts(countsByColor);
        UIManager.Instance.UpdateCentralSlotsWithSO(selections);
        UIManager.Instance.SetDrawButton(selectionCount > 0);
        UIManager.Instance.SetReRollButton(selectionCount > 0 && !hasRerolledThisTurn);
    }

    /// <summary>외부 호출용: JSON 파일에 현재 덱 상태를 저장</summary>
    public void SaveDeckState()
    {
        var state = new DeckState();
        foreach (var kv in deckCounts)
            state.entries.Add(new DeckEntry
            {
                runeID = kv.Key.name,
                count = kv.Value
            });
        string json = JsonUtility.ToJson(state, true);
        File.WriteAllText(deckStatePath, json);
        Debug.Log($"Saved deck state to {deckStatePath}");
    }
    /// <summary>Awake에서 호출: 저장된 JSON이 있으면 불러와 덱Counts 복원</summary>
    public void LoadDeckState()
    {
        if (!File.Exists(deckStatePath)) return;
        string json = File.ReadAllText(deckStatePath);
        var state = JsonUtility.FromJson<DeckState>(json);
        if (state?.entries == null) return;

        // 덱 초기화
        foreach (var so in runeDefinitions)
            deckCounts[so] = 0;
        // JSON대로 덮어쓰기
        foreach (var entry in state.entries)
        {
            var so = runeDefinitions
                .FirstOrDefault(r => r.name == entry.runeID);
            if (so != null)
                deckCounts[so] = entry.count;
        }
        foreach (var kv in deckCounts)
            Debug.Log($"[Load] {kv.Key.name}: {kv.Value}");
    }
    void OnDestroy()
    {
        UIManager.Instance.BindRuneDeck(null, null, null);
        UIManager.Instance.BindReRoll(null);
    }

    [ContextMenu("Reset DeckState.json")]
    private void ResetDeckState()
    {
        if (File.Exists(deckStatePath))
            File.Delete(deckStatePath);
        Debug.Log("Deleted DeckState.json");
    }

}
