using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    /// <summary>다음 턴에 리필할 색상 목록</summary>
    private HashSet<RuneColor> colorsToRefillNextTurn = new HashSet<RuneColor>(); // 다음 턴에 리필할 색상을 저장

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

        deckCounts = new Dictionary<RuneSO, int>();
        discardPile = new List<RuneSO>();
        runeSOByColor = new Dictionary<RuneColor, List<RuneSO>>();
        foreach (RuneColor c in Enum.GetValues(typeof(RuneColor)))
            runeSOByColor[c] = new List<RuneSO>();

        // runeDefinitions에 있는 모든 룬을 대상으로 작업
        foreach (var so in runeDefinitions)
        {
            if (so.isBasicRune)
            {
                deckCounts[so] = so.initialDeckCount; // 기본 룬은 지정된 초기 수량으로 설정
            }
            else
            {
                deckCounts[so] = 0; // 보상 룬 및 기타 룬은 시작 시 0개
            }
            runeSOByColor[so.color].Add(so); // 색상별 그룹핑은 그대로 유지
        }

        foreach (var kv in deckCounts)
            Debug.Log($"[Awake - Initial Setup] {kv.Key.name}: {kv.Value}");

        LoadDeckState(); // 저장된 상태가 있으면 여기서 덮어쓰므로, 초기 설정은 이전에 와야 함
    }

    void Start()
    {
        //// UI 이벤트 바인딩
        //UIManager.Instance.BindRuneDeck(OnDeckClick, OnDrawClick);
        //UIManager.Instance.BindReRoll(OnReRoll);
        //RefreshUI();
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        // 만약 UIManager.Instance가 여전히 유효하고, 이 RuneDeckManager가 파괴될 때
        // UIManager의 리스너를 정리하고 싶다면 여기서 처리할 수 있지만,
        // 보통은 UIManager가 자신의 OnDestroy에서 리스너를 정리하는 것이 더 일반적입니다.
    }



    // RuneDeckManager.cs의 OnSceneLoaded 수정 (코루틴 사용 제안)
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[RDM.OnSceneLoaded] >>>>> 씬 로드 시작: '{scene.name}', 로드 모드: {mode}, 시간: {Time.time} <<<<<");

        if (scene.name == "MapScene" || scene.name == "SomeOtherSceneWithoutRuneDeckUI" || scene.name == "sampleScene")
        {
            Debug.LogWarning($"[RDM.OnSceneLoaded] '{scene.name}' 씬은 UIManager 관련 로직을 건너뜁니다.");
            Debug.Log($"[RDM.OnSceneLoaded] <<<<< 씬 '{scene.name}' 처리 완료 (건너뜀) >>>>>");
            return;
        }

        // UIManager가 준비될 시간을 주기 위해 코루틴으로 실행
        StartCoroutine(ProcessSceneLoad(scene));
    }

    private IEnumerator ProcessSceneLoad(Scene scene)
    {
        // 한 프레임 대기 (또는 아주 짧은 시간 대기)
        yield return null; // 또는 yield return new WaitForSeconds(0.1f);

        Debug.Log($"[RDM.ProcessSceneLoad] 코루틴 실행됨. 씬: '{scene.name}'. UIManager.Instance 확인 중...");
        if (UIManager.Instance != null)
        {
            Debug.Log($"[RDM.ProcessSceneLoad] UIManager.Instance 유효함! (오브젝트: {UIManager.Instance.gameObject.name}). UI 바인딩 및 RefreshUI를 시도합니다.");
            try
            {
                UIManager.Instance.BindRuneDeck(OnDeckClick, OnDrawClick);
                UIManager.Instance.BindReRoll(OnReRoll);
                RefreshUI();
                Debug.Log("[RDM.ProcessSceneLoad] UI 바인딩 및 RefreshUI 성공적으로 완료됨.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[RDM.ProcessSceneLoad] UI 바인딩 또는 RefreshUI 중 예외 발생: {e}");
            }
        }
        else
        {
            Debug.LogError($"[RDM.ProcessSceneLoad] !!! UIManager.Instance가 NULL입니다. 씬 '{scene.name}'은 UIManager가 필요하지만 찾을 수 없습니다. UI 관련 작업 실패 !!!");
        }
        Debug.Log($"[RDM.ProcessSceneLoad] <<<<< 씬 '{scene.name}' 처리 완료 >>>>>");
    }
    /// <summary>
    /// 덱 버튼 클릭: 슬롯에 룬 추가 (덱이 비어 있을 경우 묘지에서 재생성)
    /// </summary>
    // RuneDeckManager.cs 의 OnDeckClick 수정 예시
    private void OnDeckClick(RuneColor color)
    {
        var group = runeSOByColor[color]; // 뽑을 룬 그룹 가져오기

        // 이제 남은 수량이 있는 룬만 뽑을 수 있게
        if (selectionCount >= selections.Count) return;

        // TryGetValue를 사용하여 더 안전하게 접근
        var available = group.Where(so => deckCounts.TryGetValue(so, out int count) && count > 0).ToList();
        Debug.Log($"[OnDeckClick] 색상 {color}의 뽑을 수 있는 룬 목록 ({available.Count}개):");
        foreach (var r_so in available)
        {
            Debug.Log($" - {r_so.name} (아이콘: {(r_so.icon == null ? "NULL" : r_so.icon.name)}, 현재 덱 개수: {deckCounts[r_so]})");
        }

        if (available.Count == 0)
        {
            Debug.Log($"[OnDeckClick] {color} 색상에 뽑을 수 있는 룬이 없습니다.");
            return;
        }

        var chosen = available[UnityEngine.Random.Range(0, available.Count)];
        Debug.Log($"[OnDeckClick] 선택된 룬: {chosen.name} (아이콘: {(chosen.icon == null ? "NULL" : chosen.icon.name)})");
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
        var rewardSO = runeDefinitions.FirstOrDefault(r => r.name == rewardRuneID);
        if (rewardSO == null)
        {
            Debug.LogError($"[RuneDeckManager] 룬 데이터베이스(runeDefinitions)에 '{rewardRuneID}' ID를 가진 RuneSO가 없습니다. 모든 RuneSO가 RuneDeckManager의 runeDefinitions 리스트에 등록되어 있는지 확인해주세요.");
            return;
        }
        Debug.Log($"[ReplaceBasicWithReward] 찾은 보상 룬: {rewardSO.name}, 아이콘: {(rewardSO.icon == null ? "NULL" : rewardSO.icon.name)}"); // 아이콘 정보 로그 추가

        // 교체 대상: 같은 색상의 '기본 룬' 중에서, 현재 플레이어가 1개 이상 소유하고 있는 룬
        var basicSOToReplace = deckCounts
            .Where(kvp => kvp.Key.isBasicRune &&              // '기본 룬'이어야 하고
                          kvp.Key.color == rewardSO.color &&  // 보상 룬과 색상이 같아야 하며
                          kvp.Value > 0)                      // 현재 1개 이상 소유하고 있어야 함
            .Select(kvp => kvp.Key)
            .FirstOrDefault();

        if (basicSOToReplace == null)
        {
            // 교체할 기본 룬이 없는 경우의 처리:
            // 1. 경고만 출력하고 아무것도 안 함 (기획상 교체이므로)
            // 2. 보상 룬을 그냥 추가 (덱 개수 제한 깨짐) - 현재 기획과는 다름
            // 3. 다른 색상의 기본 룬이라도 교체? - 기획과 다름
            Debug.LogWarning($"[RuneDeckManager] 교체할 기본 {rewardSO.color} 룬이 덱에 없습니다. (해당 색상의 기본 룬이 모두 보상 룬으로 교체되었거나, RuneSO의 isBasicRune 설정 오류일 수 있습니다)");
            // 기획서에 따르면 "기존 룬 중 1개에 효과 부여" [cite: 5] 이므로, 교체 대상이 없으면 더 이상 진행하지 않는 것이 적절해 보입니다.
            return;
        }

        // 덱 카운트 업데이트: 기본 룬 감소, 보상 룬 증가
        deckCounts[basicSOToReplace]--;
        // deckCounts에 rewardSO 키가 없다면(Awake에서 0으로 초기화되었으므로 보통은 존재함), 추가해줍니다.
        if (!deckCounts.ContainsKey(rewardSO))
        {
            deckCounts[rewardSO] = 0;
        }
        deckCounts[rewardSO]++;
        Debug.Log($"[ReplaceBasicWithReward] 덱 카운트 업데이트 후 - {basicSOToReplace.name}: {deckCounts[basicSOToReplace]}, {rewardSO.name}: {deckCounts[rewardSO]}"); // 카운트 확인
        Debug.Log($"룬 교체 완료: '{basicSOToReplace.displayName}' 1개 감소, '{rewardSO.displayName}' 1개 증가. 총 룬 개수 유지.");

        RefreshUI();
        SaveDeckState(); // 변경된 덱 상태 저장
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
    // RuneDeckManager.cs
    public void RefillFlaggedColorsFromDiscard() // 메서드 이름 변경 제안 (기존 것 수정해도 무방)
    {
        Debug.Log("[RuneDeckManager] RefillFlaggedColorsFromDiscard 호출됨 (플레이어 턴 시작 시점)");

        if (colorsToRefillNextTurn.Count == 0)
        {
            Debug.Log("[RuneDeckManager] 이번 턴에 리필하도록 플래그된 색상이 없습니다.");
            return;
        }

        // HashSet을 순회 중 변경할 수 없으므로, 복사본 사용 또는 플래그를 나중에 지움
        List<RuneColor> flaggedColors = colorsToRefillNextTurn.ToList();

        foreach (RuneColor colorToRefill in flaggedColors)
        {
            Debug.Log($"[RuneDeckManager] 플래그된 {colorToRefill} 색상 룬 리필을 시도합니다.");

            var runesToRestore = discardPile
                .Where(runeSO => runeSO.color == colorToRefill)
                .ToList(); // 묘지에서 해당 색상의 모든 룬을 가져옴

            if (runesToRestore.Any())
            {
                foreach (var runeToMove in runesToRestore)
                {
                    // deckCounts에 해당 룬의 카운트를 증가시킴
                    if (!deckCounts.ContainsKey(runeToMove))
                    {
                        // 이 경우는 Awake에서 모든 SO를 deckCounts에 0으로라도 초기화했다면 발생하지 않아야 함
                        deckCounts[runeToMove] = 0;
                        Debug.LogWarning($"[RuneDeckManager] {runeToMove.name} ({runeToMove.color}) 가 deckCounts에 없어 0으로 초기화 후 리필합니다.");
                    }
                    deckCounts[runeToMove]++;
                    discardPile.Remove(runeToMove); // 묘지에서 해당 '인스턴스' 제거
                }
                Debug.Log($"[RuneDeckManager] {colorToRefill} 색상 룬 {runesToRestore.Count}개를 묘지에서 덱으로 리필했습니다.");
            }
            else
            {
                Debug.LogWarning($"[RuneDeckManager] {colorToRefill} 색상은 리필 플래그되었으나, 묘지에서 가져올 룬이 없습니다.");
            }
        }

        colorsToRefillNextTurn.Clear(); // 모든 플래그된 색상에 대한 리필 시도 후 플래그 초기화
        RefreshUI(); // UI 갱신
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
        discardPile.Clear();
        deckCounts.Clear(); // 덱 카운트 초기화

        foreach (var so in runeDefinitions)
        {
            if (so.isBasicRune)
            {
                deckCounts[so] = so.initialDeckCount; // 기본 룬은 지정된 초기 수량으로 설정 [cite: 1]
            }
            else
            {
                deckCounts[so] = 0; // 보상 룬 및 기타 룬은 0개로 초기화
            }
        }
        Debug.Log("덱이 기본 상태로 리셋되었습니다.");
        RefreshUI();
        SaveDeckState(); // 리셋된 상태 저장 (선택 사항)
    }
    // RuneDeckManager.cs
    public void CheckAndFlagEmptyColorsForRefill()
    {
        Debug.Log("[RuneDeckManager] CheckAndFlagEmptyColorsForRefill 호출됨 (플레이어 턴 종료 시점)");
        colorsToRefillNextTurn.Clear(); // 이전 턴의 플래그는 초기화

        foreach (var colorEntry in runeSOByColor) // runeSOByColor는 Dictionary<RuneColor, List<RuneSO>>
        {
            RuneColor currentColor = colorEntry.Key;
            List<RuneSO> runesInColorGroup = colorEntry.Value; // 해당 색상으로 정의된 모든 RuneSO (기본+보상)

            // 해당 색상의 모든 종류의 룬(기본, 보상 포함)이 현재 deckCounts에서 0개인지 확인
            bool isColorDeckCompletelyEmpty = runesInColorGroup.All(runeSO =>
                deckCounts.TryGetValue(runeSO, out int count) && count == 0
            );

            if (isColorDeckCompletelyEmpty)
            {
                // 덱이 비었더라도, 묘지에 해당 색상 룬이 있어야 리필 의미가 있음
                bool hasMatchingRunesInDiscard = discardPile.Any(discardedRune => discardedRune.color == currentColor);
                if (hasMatchingRunesInDiscard)
                {
                    Debug.Log($"[RuneDeckManager] {currentColor} 색상 덱이 비었고, 묘지에 관련 룬이 있어 다음 턴 리필 대상으로 플래그합니다.");
                    colorsToRefillNextTurn.Add(currentColor);
                }
                else
                {
                    Debug.Log($"[RuneDeckManager] {currentColor} 색상 덱은 비었지만, 묘지에 관련 룬이 없어 리필 플래그를 설정하지 않습니다.");
                }
            }
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
