using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
        UIManager.Instance.BindRuneDeck(OnDeckClick, OnDrawClick);
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
            //// 여기서 바로 덱 개수만 업데이트
            //var countsByColor = new Dictionary<RuneColor, int>();
            //foreach (var kv in deckCounts)
            //{
            //    var col = kv.Key.color;
            //    if (!countsByColor.ContainsKey(col))
            //        countsByColor[col] = 0;
            //    countsByColor[col] += kv.Value;
            //}
            //UIManager.Instance.UpdateDeckCounts(countsByColor);

        }

        // 3) 이제 남은 수량이 있는 룬만 뽑을 수 있게
        if (selectionCount >= selections.Count) return;
        var available = group.Where(so => deckCounts[so] > 0).ToList();
        if (available.Count == 0) return;

        var chosen = available[UnityEngine.Random.Range(0, available.Count)];
        deckCounts[chosen]--;
        selections[selectionCount++] = chosen;

        RefreshUI();
    }




    private void OnDrawClick()
    {
  
           // 1) 슬롯에 담긴 모든 룬 효과 발동 및 묘지로 이동
            var user = Player.Instance;
            var targets = EnemySpawner.Instance.SpawnedEnemies;
           for (int i = 0; i < selectionCount; i++)
               {
                var so = selections[i];
                  // 효과 실행
             so.effectSO.Execute(user, targets);
                   // 묘지로 이동
                discardPile.Add(so);
               }

    
        // 2) 슬롯 초기화
        selections = new List<RuneSO>(new RuneSO[5]);
        selectionCount = 0;
        hasRerolledThisTurn = false;

   
      
        // 3) 덱 상태 저장 (덱은 변화 없지만, 묘지 변화가 반영될 수 있음)
        SaveDeckState();

         // 4) 플레이어 턴 종료 신호
        Debug.Log("[OnDrawClick] EndTurn 호출 전 myTurn=" + TurnManager.Inst.myTurn);
        TurnManager.Inst.EndTurn();
        Debug.Log("[OnDrawClick] EndTurn 호출 후 myTurn=" + TurnManager.Inst.myTurn);


        RefreshUI();
    }



    /// <summary>리롤 클릭: 슬롯 전체 반환 후 재드로우 X</summary>
    private void OnReRoll()
    {
        if (hasRerolledThisTurn) return;
        // ✅ 올바른 처리: 뽑았던 룬들은 묘지로 보내고, 덱에는 돌려주지 않음
        for (int i = 0; i < selectionCount; i++)
            discardPile.Add(selections[i]);

        // 슬롯 비우기
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

        //슬롯이 *전부* 채워진 경우에만 Draw/Reroll 활성화
        bool full = (selectionCount == selections.Count);  // selections.Count == 최대 슬롯 개수(예:5)
        UIManager.Instance.SetDrawButton(full);
        UIManager.Instance.SetReRollButton(full && !hasRerolledThisTurn);
    }
    /// <summary>
    /// 플레이어 턴 시작 시 호출:
    /// 덱에 남은 수량이 0인 색상의 룬을 묘지에서 모두 덱으로 복원합니다.
    /// </summary>
    public void RefillEmptyColorsFromDiscard()
    {
        foreach (var kv in runeSOByColor)
        {
            var color = kv.Key;
            var group = kv.Value;

            // 1) 이 색상의 덱이 전부 0인지 확인
            bool isEmpty = group.All(so => deckCounts[so] == 0);

            // 2) 묘지에 이 색상 룬이 하나라도 남아 있는지 확인
            bool hasInDiscard = discardPile.Any(so => so.color == color);

            if (isEmpty && hasInDiscard)
            {
                // 3) 묘지에서 같은 색상 룬을 모두 골라 덱으로 복원
                var toRestore = discardPile
                    .Where(so => so.color == color)
                    .ToList();

                foreach (var so in toRestore)
                {
                    deckCounts[so] = deckCounts.ContainsKey(so)
                        ? deckCounts[so] + 1
                        : 1;
                    discardPile.Remove(so);
                }
            }
        }
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
    /// <summary>
    /// 덱을 기본 최대치(10/10/5/10)로 전부 리셋하고 묘지 비우기
    /// </summary>
    public void ResetDeckToDefault()
    {
        // 1) 묘지 초기화
        discardPile.Clear();

        // 2) 덱 카운트 초기화
        deckCounts.Clear();
        foreach (var so in runeDefinitions)
        {
            int count = so.color switch
            {
                RuneColor.Red => 10,
                RuneColor.Blue => 10,
                RuneColor.White => 5,
                RuneColor.Yellow => 10,
                _ => so.initialCount  // 혹시 다른 색이 있으면 기본값 사용
            };
            deckCounts[so] = count;
        }
    }


    void OnDestroy()
    {
        UIManager.Instance.BindRuneDeck(null, null);
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
