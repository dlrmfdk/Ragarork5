using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 덱 및 묘지 관리를 담당하며, JSON 파일로 상태를 저장/로드합니다.
/// UIManager가 준비되면 이벤트를 통해 UI 관련 작업을 초기화합니다.
/// </summary>
public class RuneDeckManager : MonoBehaviour
{
    public static RuneDeckManager Instance { get; private set; }

    public bool isUIManagerReady = false; // UIManager 준비 및 바인딩 완료 여부 플래그

    [Header("룬 정의 SO 리스트")]
    public List<RuneSO> runeDefinitions;

    // 덱 상태: 각 RuneSO별 보유 개수
    private Dictionary<RuneSO, int> deckCounts;
    // 사용된 룬(묘지)
    private List<RuneSO> discardPile;
    // 색상별 룬 그룹
    private Dictionary<RuneColor, List<RuneSO>> runeSOByColor;

    /// <summary>다음 턴에 리필할 색상 목록</summary>
    private HashSet<RuneColor> colorsToRefillNextTurn = new HashSet<RuneColor>();

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

        foreach (var so in runeDefinitions)
        {
            if (so.isBasicRune)
            {
                deckCounts[so] = so.initialDeckCount;
            }
            else
            {
                deckCounts[so] = 0;
            }
            runeSOByColor[so.color].Add(so);
        }

        foreach (var kv in deckCounts)
            Debug.Log($"[RDM.Awake - Initial Setup] {kv.Key.name}: {kv.Value}");

        LoadDeckState();
    }

    void Start()
    {
        Debug.Log("[RDM.Start] 시작됨. UIManager.OnUIManagerReady 이벤트를 기다려 UI 설정 예정.");
        // Start에서 직접적인 UI 바인딩 및 RefreshUI 호출 제거
        // 모든 UI 관련 초기화는 HandleUIManagerReady에서 수행
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        UIManager.OnUIManagerReady += HandleUIManagerReady; // UIManager 준비 이벤트 구독
        Debug.Log("[RDM.OnEnable] UIManager.OnUIManagerReady 이벤트 구독 완료.");
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UIManager.OnUIManagerReady -= HandleUIManagerReady; // UIManager 준비 이벤트 구독 해제
        Debug.Log("[RDM.OnDisable] UIManager.OnUIManagerReady 이벤트 구독 해제 완료.");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[RDM.OnSceneLoaded] >>>>> 씬 로드 완료: '{scene.name}', 로드 모드: {mode}, 시간: {Time.time} <<<<<");
        isUIManagerReady = false; // 새 씬이 로드되면 UIManager 준비 상태 초기화
                                  // UIManager가 준비되면 OnUIManagerReady 이벤트가 발생하여 isUIManagerReady가 true로 설정될 것임.
                                  // 씬 로드 시 특별히 RuneDeckManager가 해야 할 작업 (UI 바인딩 외)이 있다면 여기에 추가
    }

    private void HandleUIManagerReady()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"[RDM.HandleUIManagerReady] UIManager.OnUIManagerReady 이벤트 수신! 현재 활성 씬: {currentSceneName}");

        if (IsUISceneToSkip(currentSceneName))
        {
            Debug.Log($"[RDM.HandleUIManagerReady] 현재 씬 '{currentSceneName}'은 UIManager 연동을 건너뜁니다.");
            isUIManagerReady = false;
            return;
        }

        if (UIManager.Instance != null)
        {
            Debug.Log($"[RDM.HandleUIManagerReady] UIManager.Instance 유효함! (오브젝트: {UIManager.Instance.gameObject.name}). UI 바인딩 및 RefreshUI를 시도합니다.");
            try
            {
                UIManager.Instance.BindRuneDeck(OnDeckClick, OnDrawClick);
                UIManager.Instance.BindReRoll(OnReRoll);
                isUIManagerReady = true; // UIManager 준비 및 바인딩 완료
                RefreshUI(); // UIManager가 준비된 후 첫 UI 새로고침
                Debug.Log("[RDM.HandleUIManagerReady] UI 바인딩 및 RefreshUI 성공적으로 완료됨.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[RDM.HandleUIManagerReady] UI 바인딩 또는 RefreshUI 중 예외 발생: {e}");
                isUIManagerReady = false;
            }
        }
        else
        {
            Debug.LogError($"[RDM.HandleUIManagerReady] !!! UIManager.OnUIManagerReady 이벤트는 발생했으나, UIManager.Instance가 NULL입니다. 심각한 오류 !!!");
            isUIManagerReady = false;
        }
    }

    // UI 연동을 건너뛸 씬인지 확인하는 헬퍼 메서드
    private bool IsUISceneToSkip(string sceneName)
    {
        return sceneName == "MapScene" || sceneName == "sampleScene" || sceneName == "SomeOtherSceneWithoutRuneDeckUI";
    }

    private void OnDeckClick(RuneColor color)
    {
        if (!isUIManagerReady)
        {
            Debug.LogWarning("[RDM.OnDeckClick] UIManager가 아직 준비되지 않아 작업을 건너뜁니다.");
            return;
        }

        var group = runeSOByColor[color];
        if (selectionCount >= selections.Count) return;

        var available = group.Where(so => deckCounts.TryGetValue(so, out int count) && count > 0).ToList();
        Debug.Log($"[OnDeckClick] 색상 {color}의 뽑을 수 있는 룬 목록 ({available.Count}개):");
        foreach (var r_so in available)
        {
            Debug.Log($" - {r_so.name} (아이콘: {(r_so.icon == null ? "NULL" : r_so.icon.name)}, 현재 덱 개수: {(deckCounts.ContainsKey(r_so) ? deckCounts[r_so] : 0)})");
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
        if (!isUIManagerReady)
        {
            Debug.LogWarning("[RDM.OnDrawClick] UIManager가 아직 준비되지 않아 작업을 건너뜁니다.");
            return;
        }

        if (Player.Instance == null) { Debug.LogError("[RDM.OnDrawClick] Player.Instance is null!"); return; }
        // EnemySpawner.Instance 또는 targets에 대한 null 체크도 필요에 따라 추가
        var user = Player.Instance;
        var targets = EnemySpawner.Instance != null ? EnemySpawner.Instance.SpawnedEnemies : null;
        if (targets == null) { Debug.LogWarning("[RDM.OnDrawClick] targets (SpawnedEnemies) is null!"); /* 전투 대상이 없으면 효과 발동이 무의미할 수 있음 */ }


        for (int i = 0; i < selectionCount; i++)
        {
            var so = selections[i];
            if (so != null) // selections에 null이 들어갈 수 있으므로 체크
            {
                if (so.effectSO == null)
                {
                    Debug.LogError($"[RDM.OnDrawClick] {so.name}의 effectSO가 null입니다! 효과를 실행할 수 없습니다.");
                    continue;
                }
                if (targets != null) // 대상이 있을 때만 효과 실행 (혹은 effectSO 내부에서 처리)
                {
                    so.effectSO.Execute(user, targets);
                }
                discardPile.Add(so);
            }
        }

        selections = new List<RuneSO>(new RuneSO[5]); // 새 리스트로 초기화
        selectionCount = 0;
        hasRerolledThisTurn = false;

        SaveDeckState();

        Debug.Log("[OnDrawClick] EndTurn 호출 전 myTurn=" + (TurnManager.Inst != null ? TurnManager.Inst.myTurn.ToString() : "TurnManager.Inst is NULL"));
        if (TurnManager.Inst != null)
        {
            TurnManager.Inst.EndTurn();
        }
        else
        {
            Debug.LogError("[RDM.OnDrawClick] TurnManager.Inst is null! EndTurn을 호출할 수 없습니다.");
        }
        Debug.Log("[OnDrawClick] EndTurn 호출 후 myTurn=" + (TurnManager.Inst != null ? TurnManager.Inst.myTurn.ToString() : "TurnManager.Inst is NULL"));

        RefreshUI();
    }

    private void OnReRoll()
    {
        if (!isUIManagerReady)
        {
            Debug.LogWarning("[RDM.OnReRoll] UIManager가 아직 준비되지 않아 작업을 건너뜁니다.");
            return;
        }

        if (hasRerolledThisTurn) return;

        for (int i = 0; i < selectionCount; i++)
        {
            if (selections[i] != null) // null 체크
                discardPile.Add(selections[i]);
        }

        selections = new List<RuneSO>(new RuneSO[5]); // 새 리스트로 초기화
        selectionCount = 0;
        hasRerolledThisTurn = true;

        RefreshUI();
    }

    public void ReplaceBasicWithReward(string rewardRuneID)
    {
        var rewardSO = runeDefinitions.FirstOrDefault(r => r.name == rewardRuneID);
        if (rewardSO == null)
        {
            Debug.LogError($"[RDM.ReplaceBasicWithReward] 룬 데이터베이스(runeDefinitions)에 '{rewardRuneID}' ID를 가진 RuneSO가 없습니다.");
            return;
        }
        Debug.Log($"[ReplaceBasicWithReward] 찾은 보상 룬: {rewardSO.name}, 아이콘: {(rewardSO.icon == null ? "NULL" : rewardSO.icon.name)}");

        var basicSOToReplace = deckCounts
            .Where(kvp => kvp.Key.isBasicRune && kvp.Key.color == rewardSO.color && kvp.Value > 0)
            .Select(kvp => kvp.Key)
            .FirstOrDefault();

       

        deckCounts[basicSOToReplace]--;
        if (!deckCounts.ContainsKey(rewardSO)) { deckCounts[rewardSO] = 0; }
        deckCounts[rewardSO]++;
        Debug.Log($"[ReplaceBasicWithReward] 덱 카운트 업데이트 후 - {basicSOToReplace.name}: {deckCounts[basicSOToReplace]}, {rewardSO.name}: {deckCounts[rewardSO]}");
        Debug.Log($"룬 교체 완료: '{basicSOToReplace.displayName}' 1개 감소, '{rewardSO.displayName}' 1개 증가. 총 룬 개수 유지.");

        RefreshUI();
        SaveDeckState();
    }

    public void RefreshUI()
    {
        if (!isUIManagerReady || UIManager.Instance == null)
        {
            Debug.LogWarning($"[RDM.RefreshUI] UIManager가 준비되지 않았거나 Instance가 null입니다. UI 새로고침을 건너뜁니다. isUIManagerReady: {isUIManagerReady}, UIManager.Instance is null: {(UIManager.Instance == null)}");
            return;
        }

        var countsByColor = new Dictionary<RuneColor, int>();
        foreach (RuneColor c in Enum.GetValues(typeof(RuneColor))) countsByColor[c] = 0;

        foreach (var kv in deckCounts)
        {
            if (kv.Key != null) // 키가 null이 아닌지 확인
            {
                countsByColor[kv.Key.color] += kv.Value;
            }
        }

        UIManager.Instance.UpdateDeckCounts(countsByColor);
        UIManager.Instance.UpdateCentralSlotsWithSO(selections);

        bool full = (selectionCount == selections.Count);
        UIManager.Instance.SetDrawButton(full);
        UIManager.Instance.SetReRollButton(full && !hasRerolledThisTurn);
    }

    public void RefillFlaggedColorsFromDiscard()
    {
        Debug.Log("[RDM.RefillFlaggedColorsFromDiscard] 호출됨 (플레이어 턴 시작 시점)");
        if (colorsToRefillNextTurn.Count == 0)
        {
            Debug.Log("[RDM.RefillFlaggedColorsFromDiscard] 이번 턴에 리필하도록 플래그된 색상이 없습니다.");
            return;
        }

        List<RuneColor> flaggedColors = colorsToRefillNextTurn.ToList();
        foreach (RuneColor colorToRefill in flaggedColors)
        {
            Debug.Log($"[RDM.RefillFlaggedColorsFromDiscard] 플래그된 {colorToRefill} 색상 룬 리필을 시도합니다.");
            var runesToRestore = discardPile.Where(runeSO => runeSO != null && runeSO.color == colorToRefill).ToList(); // null 체크 추가

            if (runesToRestore.Any())
            {
                foreach (var runeToMove in runesToRestore)
                {
                    if (!deckCounts.ContainsKey(runeToMove)) { deckCounts[runeToMove] = 0; }
                    deckCounts[runeToMove]++;
                    // discardPile.Remove(runeToMove)는 List<T>.Remove의 특성상 첫 번째로 일치하는 요소만 제거합니다.
                    // 만약 동일한 SO 인스턴스가 여러 번 묘지에 갔다면, 한 번만 제거됩니다.
                    // 묘지에 있는 모든 인스턴스를 제거하려면 RemoveAll 또는 역순 루프 후 RemoveAt을 사용해야 하지만,
                    // 현재는 카운트 기반이므로 이 방식도 문제는 없습니다 (단, 묘지에는 중복 SO가 여러 개 있을 수 있음).
                    // 여기서는 일단 기존 로직 유지.
                    discardPile.Remove(runeToMove);
                }
                Debug.Log($"[RDM.RefillFlaggedColorsFromDiscard] {colorToRefill} 색상 룬 {runesToRestore.Count}개를 묘지에서 덱으로 리필했습니다.");
            }
            else
            {
                Debug.LogWarning($"[RDM.RefillFlaggedColorsFromDiscard] {colorToRefill} 색상은 리필 플래그되었으나, 묘지에서 가져올 룬이 없습니다.");
            }
        }
        colorsToRefillNextTurn.Clear();
        RefreshUI();
    }

    public void ResetDeckToDefault()
    {
        discardPile.Clear();
        deckCounts.Clear();
        foreach (var so in runeDefinitions)
        {
            if (so.isBasicRune) { deckCounts[so] = so.initialDeckCount; } // [cite: 1] // 기본 룬 초기화
            else { deckCounts[so] = 0; } // 보상 룬은 0으로
        }
        Debug.Log("덱이 기본 상태로 리셋되었습니다.");
        isUIManagerReady = false; // UI 매니저도 새 씬에서 다시 준비해야 할 수 있으므로 리셋
        if (UIManager.Instance != null)
        { // 즉시 RefreshUI를 시도하기보다, UIManagerReady 이벤트를 통해 처리되도록 유도
            RefreshUI(); // 이 호출은 UIManager.Instance가 null이 아닐 때만 의미 있음
        }
        SaveDeckState(); // 리셋된 상태 저장
    }

    public void CheckAndFlagEmptyColorsForRefill()
    {
        Debug.Log("[RDM.CheckAndFlagEmptyColorsForRefill] 호출됨 (플레이어 턴 종료 시점)");
        colorsToRefillNextTurn.Clear();
        foreach (var colorEntry in runeSOByColor)
        {
            RuneColor currentColor = colorEntry.Key;
            List<RuneSO> runesInColorGroup = colorEntry.Value;

            if (runesInColorGroup == null || !runesInColorGroup.Any()) continue; // 해당 색상 그룹에 룬 정의가 없으면 건너뜀

            bool isColorDeckCompletelyEmpty = runesInColorGroup.All(runeSO =>
                runeSO != null && deckCounts.TryGetValue(runeSO, out int count) && count == 0 // runeSO null 체크 추가
            );

            if (isColorDeckCompletelyEmpty)
            {
                bool hasMatchingRunesInDiscard = discardPile.Any(discardedRune => discardedRune != null && discardedRune.color == currentColor); // null 체크 추가
                if (hasMatchingRunesInDiscard)
                {
                    Debug.Log($"[RDM.CheckAndFlagEmptyColorsForRefill] {currentColor} 색상 덱이 비었고, 묘지에 관련 룬이 있어 다음 턴 리필 대상으로 플래그합니다.");
                    colorsToRefillNextTurn.Add(currentColor);
                }
                else
                {
                    Debug.Log($"[RDM.CheckAndFlagEmptyColorsForRefill] {currentColor} 색상 덱은 비었지만, 묘지에 관련 룬이 없어 리필 플래그를 설정하지 않습니다.");
                }
            }
        }
    }
    // RuneDeckManager.cs 에 추가
    public void PrepareDeckForNewBattle()
    {
        Debug.Log("[RDM.PrepareDeckForNewBattle] 새 전투를 위해 덱을 준비합니다.");

        // 1. 묘지에 있는 룬들을 다시 deckCounts로 옮기기
        if (discardPile != null && discardPile.Any())
        {
            foreach (var runeSO_instance in discardPile)
            {
                if (runeSO_instance != null)
                {
                    if (deckCounts.ContainsKey(runeSO_instance))
                    {
                        deckCounts[runeSO_instance]++;
                    }
                    else
                    {
                        deckCounts[runeSO_instance] = 1;
                        Debug.LogWarning($"[RDM.PrepareDeckForNewBattle] 묘지의 룬 '{runeSO_instance.name}'이 deckCounts에 없어 1로 추가합니다.");
                    }
                }
            }
            Debug.Log($"[RDM.PrepareDeckForNewBattle] 묘지에서 {discardPile.Count}개의 룬을 덱으로 복원했습니다.");
            discardPile.Clear(); // 묘지 비우기
        }
        else
        {
            Debug.Log("[RDM.PrepareDeckForNewBattle] 묘지가 비어있어 복원할 룬이 없습니다.");
        }

        // 2. 선택된 룬(패) 초기화 및 리롤 상태 리셋
        selections = new List<RuneSO>(new RuneSO[5]); // 최대 패 크기가 5라고 가정
        selectionCount = 0;
        hasRerolledThisTurn = false;

        // 3. 다음 턴 리필 플래그 초기화 (이전 전투의 상태가 다음 전투로 이어지지 않도록)
        colorsToRefillNextTurn.Clear();

        Debug.Log("[RDM.PrepareDeckForNewBattle] 새 전투 준비 완료. 현재 덱 상태:");
        foreach (var kvp in deckCounts)
        {
            if (kvp.Key != null)
            {
                Debug.Log($" - {kvp.Key.displayName} ({kvp.Key.name}): {kvp.Value}개");
            }
        }
        // RefreshUI()는 이후 PlayerTurn 시작 시 또는 UIManager가 준비될 때 호출될 것이므로 여기서 직접 호출하지 않아도 될 수 있습니다.
        // 만약 이 시점에 즉시 UI 갱신이 필요하다면, isUIManagerReady 플래그를 확인하고 호출하세요.
        // if (isUIManagerReady && UIManager.Instance != null) RefreshUI();
    }




    public void SaveDeckState()
    {
        var state = new DeckState();
        foreach (var kv in deckCounts)
        {
            if (kv.Key != null) // 키가 null이 아닌지 확인
            {
                state.entries.Add(new DeckEntry
                {
                    runeID = kv.Key.name, // RuneSO의 name (에셋 파일 이름)을 ID로 사용
                    count = kv.Value
                });
            }
        }
        try
        {
            string json = JsonUtility.ToJson(state, true);
            File.WriteAllText(deckStatePath, json);
            Debug.Log($"Saved deck state to {deckStatePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save deck state: {e.Message}");
        }
    }

    public void LoadDeckState()
    {
        if (!File.Exists(deckStatePath)) return;
        try
        {
            string json = File.ReadAllText(deckStatePath);
            var state = JsonUtility.FromJson<DeckState>(json);
            if (state?.entries == null) return;

            // deckCounts를 직접 수정하기 전에, 모든 정의된 룬에 대해 기본 카운트(보통 0)를 설정할 수 있음
            // Awake에서 이미 isBasicRune에 따라 초기화 했으므로, 여기서는 로드된 값으로 덮어쓰기만 해도 됨.
            // 다만, 안전을 위해 deckCounts에 키가 없는 경우를 대비할 수 있으나, Awake에서 모든 키를 생성하므로 문제는 없을 것.

            foreach (var entry in state.entries)
            {
                var so = runeDefinitions.FirstOrDefault(r => r != null && r.name == entry.runeID); // null 체크 추가
                if (so != null)
                {
                    deckCounts[so] = entry.count; // Awake에서 이미 키가 존재함
                }
                else
                {
                    Debug.LogWarning($"[RDM.LoadDeckState] 저장된 룬 ID '{entry.runeID}'에 해당하는 RuneSO를 runeDefinitions에서 찾을 수 없습니다.");
                }
            }
            Debug.Log("Deck state loaded from JSON.");
            foreach (var kv in deckCounts) // 로드 후 상태 확인 로그
                if (kv.Key != null) Debug.Log($"[Load - 확인] {kv.Key.name}: {kv.Value}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load deck state: {e.Message}");
        }
    }

    void OnDestroy()
    {
        // UIManager.Instance가 null일 수 있으므로 null 체크 추가
        if (UIManager.Instance != null && isUIManagerReady) // UIManager가 준비되었을 때만 해제 시도
        {
            Debug.Log("[RDM.OnDestroy] UIManager의 리스너 해제 시도.");
            // BindRuneDeck(null, null)은 UIManager의 해당 메서드에서 RemoveAllListeners를 호출함
            UIManager.Instance.BindRuneDeck(null, null);
            UIManager.Instance.BindReRoll(null);
        }
    }

    [ContextMenu("Delete DeckState.json File")]
    public void DeleteSavedDeckStateFile()
    {
        if (File.Exists(deckStatePath))
        {
            File.Delete(deckStatePath);
            Debug.Log($"[RuneDeckManager] DeckState.json 파일 삭제 완료: {deckStatePath}");
        }
        else
        {
            Debug.Log("[RuneDeckManager] DeckState.json 파일이 없어 삭제할 수 없습니다.");
        }
    }
}

