// RuneDeckManager.cs 최종 완성본
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class RuneDeckManager : MonoBehaviour
{
    public static RuneDeckManager Instance { get; private set; }

    private static bool hasDeletedSaveOnLaunch = false;

    [Header("룬 정의 SO 리스트")]
    public List<RuneSO> runeDefinitions;

    // ▼▼▼ 1. 모든 룬 정보를 이름으로 빠르게 찾을 수 있는 사전을 만듭니다. ▼▼▼
    private Dictionary<string, RuneSO> runeDefinitionMap;

    [Header("특별 룬 SO")]
    [SerializeField] private RuneSO aoeRuneSO;
    [SerializeField] private RuneSO lifestealRuneSO;

    private List<RuneInstance> playerDeck;
    private List<RuneInstance> discardPile;
    private List<RuneInstance> selections;

    private int selectionCount = 0;
    private bool hasRerolledThisTurn;
    public bool isUIManagerReady = false;
    private HashSet<RuneColor> colorsToRefillNextTurn = new HashSet<RuneColor>();
    private string deckStatePath => Path.Combine(Application.persistentDataPath, "DeckState.json");

    void Awake()
    {
        if (Instance == null)
        { Instance = this;
           DontDestroyOnLoad(gameObject);

            // ▼▼▼ Awake에서 사전을 미리 채워넣는 함수를 호출합니다. ▼▼▼
            InitializeRuneMap();

            // ▼▼▼ 게임 실행 후 딱 한 번만 실행되는 로직 ▼▼▼
            // 아직 한 번도 실행되지 않았을 때만 (자물쇠가 열려있을 때만)
            if (!hasDeletedSaveOnLaunch)
            {
                // 1. 파일 삭제 시도
                if (File.Exists(deckStatePath))
                {
                    File.Delete(deckStatePath);
                    Debug.Log($"[RuneDeckManager.Awake] 새 게임 세션 시작. 기존 덱 세이브 파일(DeckState.json)을 삭제했습니다.");
                }

                // 2. 이제 실행했으니, 자물쇠를 잠급니다. (다시는 실행되지 않도록)
                hasDeletedSaveOnLaunch = true;
            }
        }
        else { Destroy(gameObject); return; }

        
        playerDeck = new List<RuneInstance>();
        discardPile = new List<RuneInstance>();
        selections = new List<RuneInstance>(new RuneInstance[5]);
        LoadDeckState();
    }

    // ▼▼▼ 3. 사전을 초기화하는 함수를 새로 추가합니다. ▼▼▼
    private void InitializeRuneMap()
    {
        runeDefinitionMap = new Dictionary<string, RuneSO>();
        if (runeDefinitions == null) return; // 리스트가 비어있으면 실행하지 않음

        foreach (var runeSO in runeDefinitions)
        {
            if (runeSO != null && !runeDefinitionMap.ContainsKey(runeSO.name))
            {
                // 룬의 파일 이름(고유 ID)을 Key로, 룬 데이터(SO)를 Value로 저장합니다.
                runeDefinitionMap.Add(runeSO.name, runeSO);
            }
        }
        Debug.Log($"[RuneDeckManager] {runeDefinitionMap.Count}개의 룬 정보를 사전에 등록했습니다.");
    }

    // ▼▼▼ 4. RuneInstance가 호출할 '룬 ID로 설계도 찾아주기' 함수를 추가합니다. ▼▼▼
    public RuneSO FindRuneSOById(string id)
    {
        // 사전에 해당 ID가 있는지 확인하고, 있으면 바로 반환합니다.
        if (runeDefinitionMap != null && runeDefinitionMap.TryGetValue(id, out RuneSO so))
        {
            return so;
        }

        // 사전에 없다면, 어떤 ID가 문제인지 정확히 알려줍니다.
        Debug.LogError($"[RuneDeckManager] ID가 '{id}'인 RuneSO를 찾을 수 없습니다! runeDefinitions 리스트에 등록되어 있는지 확인해주세요.");
        return null;
    }

    void OnEnable() { UIManager.OnUIManagerReady += HandleUIManagerReady; }
    void OnDisable() { UIManager.OnUIManagerReady -= HandleUIManagerReady; }

    private void HandleUIManagerReady()
    {
        if (UIManager.Instance == null) return;
        UIManager.Instance.BindRuneDeck(OnDeckClick, OnDrawClick);
        UIManager.Instance.BindReRoll(OnReRoll);
        isUIManagerReady = true;
        RefreshUI();
    }

    private void OnDeckClick(RuneColor color)
    {
        
        // 1. 만약 클릭한 색상이 '흰색'이고,
        // 2. 현재 패(selections)에 이미 흰색 룬이 존재한다면,
        if (color == RuneColor.White && selections.Any(instance => instance != null && instance.SO.color == RuneColor.White))
        {
            Debug.Log("흰색 룬은 한 번에 하나만 선택할 수 있습니다.");
            return; // 함수를 즉시 종료하여 룬이 추가되는 것을 막습니다.
        }
       

        if (!isUIManagerReady) return;
        if (selectionCount >= selections.Count) return;

        var availableRunes = playerDeck.Where(inst => inst.SO?.color == color).ToList();

        if (availableRunes.Count == 0)
        {
            Debug.Log($"[OnDeckClick] {color} 색상에 뽑을 수 있는 룬이 없습니다.");
            return;
        }

        var chosenInstance = availableRunes[Random.Range(0, availableRunes.Count)];

        playerDeck.Remove(chosenInstance);
        selections[selectionCount++] = chosenInstance;

        RefreshUI();

        //10.23 김서현 추가 (룬 데미지, 방어도, 골드 등 합계 UI)
        CalculateAndDisplayHandTotals(); // [추가!] 룬을 뽑을 때마다 합계 다시 계산
    }

    private void OnDrawClick()
    {
        if (!isUIManagerReady || Player.Instance == null) return;

        BattleContext.Reset();

        // 1. 패에 '광역 공격' 룬(aoeRuneSO)이 있는지 먼저 확인합니다.
        bool isAoeAttack = selections.Any(instance => instance != null && instance.SO == aoeRuneSO);

        if (isAoeAttack)
        {
            // 2. 광역 룬이 있다면, 타겟팅 없이 즉시 '광역 공격 전용 로직'을 실행합니다.
            Debug.Log("[RDM] '광역공격' 룬 감지! 광역 공격 로직을 실행합니다.");
            ExecuteAoeRuneLogic();
        }
        else
        {
            // 3. 광역 룬이 없다면, 기존의 일반 공격(타겟팅) 또는 방어 로직을 실행합니다.
            // ▼▼▼ [이 부분을 수정합니다] ▼▼▼
            bool hasRedRunes = selections.Any(instance => instance != null && instance.SO.color == RuneColor.Red);
            // '타겟팅이 필요하다(requiresEnemyTarget)'고 설정된 룬이 있는지 확인
            bool needsTargeting = selections.Any(inst => inst != null && inst.SO.requiresEnemyTarget);

            if (hasRedRunes || needsTargeting) // 붉은 룬이 있거나 || 타겟팅이 필요한 룬이 있다면
            {
                // 단일 타겟팅 시작
                PlayerInputManager.Instance.StartEnemyTargeting(selections);
            }
            // ▲▲▲ 수정 완료 ▲▲▲
            else
            {
                ExecuteAllSelectedRunesImmediately();
            }
        }
    }

    private void OnReRoll()
    {
        // 1. UI가 준비되지 않았거나, 이번 턴에 이미 리롤을 했다면 아무것도 하지 않음
        if (!isUIManagerReady || hasRerolledThisTurn)
        {
            if (hasRerolledThisTurn) Debug.Log("이번 턴에는 이미 리롤을 사용했습니다.");
            return;
        }

        Debug.Log("리롤을 실행합니다.");

        // 2. 패에 있는 룬들을 묘지로 보내거나 소멸시킵니다.
        for (int i = 0; i < selectionCount; i++)
        {
            var rerolledInstance = selections[i];
            if (rerolledInstance == null) continue;

            // 회색 룬이 아니면 묘지로 보냅니다.
            if (rerolledInstance.SO.color != RuneColor.Gray)
            {
                discardPile.Add(rerolledInstance);
            }
        }

        // 3. 이번 턴에 리롤을 사용했다고 기록합니다.
        hasRerolledThisTurn = true;

        // 4. 패(selections)를 비우고 카운트를 0으로 만듭니다.
        selections = new List<RuneInstance>(new RuneInstance[5]);
        selectionCount = 0;

        // 5. UI를 새로고침하여 빈 패를 보여주고, 비활성화된 리롤 버튼 상태를 반영합니다.
        RefreshUI();

        //10.23 "
        selections = new List<RuneInstance>(new RuneInstance[5]);
        selectionCount = 0;

        RefreshUI();
        CalculateAndDisplayHandTotals(); // [추가!] 리롤(초기화) 시 합계 다시 계산 (0으로)
    }

    private void FinalizeTurnAfterAction()
    {
        Debug.Log("모든 룬 효과 처리 완료. 후처리 효과 및 턴 정리를 시작합니다.");

        // ▼▼▼ 피해 흡혈 효과 처리 로직 추가 ▼▼▼
        if (selections.Any(inst => inst != null && inst.SO == lifestealRuneSO))
        {
            int totalDamage = BattleContext.TotalDamageDealtThisAction;
            if (totalDamage > 0)
            {
                Debug.Log($"[착취의 룬] 총 피해량 {totalDamage}만큼 체력을 회복합니다.");
                Player.Instance.Heal(totalDamage);
            }
        }

        // 패에 있는 룬들을 하나씩 확인합니다.
        for (int i = 0; i < selectionCount; i++)
        {
            var usedInstance = selections[i];
            if (usedInstance == null) continue;

            // 룬의 색상이 회색이 아닐 경우에만 묘지로 보냅니다.
            if (usedInstance.SO.color != RuneColor.Gray)
            {
                discardPile.Add(usedInstance);
            }
            else
            {
                // 회색 룬일 경우, 아무것도 하지 않아 묘지로 가지 않고 소멸됩니다.
                Debug.Log($"회색 룬 '{usedInstance.SO.displayName}'이(가) 사용 후 소멸되었습니다.");
            }
        }
 

        ClearSelectionsAndPrepareForNextAction();
        SaveDeckState();

        if (TurnManager.Inst != null)
        {
            TurnManager.Inst.EndTurn();
        }
    }
    public void ClearSelectionsAndPrepareForNextAction()
    {
        selections = new List<RuneInstance>(new RuneInstance[5]);
        selectionCount = 0;
        hasRerolledThisTurn = false;
        RefreshUI();
        CalculateAndDisplayHandTotals(); // [이 줄을 추가하세요]
    }
  

    #region Effect Execution


    private void ExecuteAoeRuneLogic()
    {
        var user = Player.Instance;
        var allEnemies = EnemySpawner.Instance?.SpawnedEnemies;

        if (user == null || allEnemies == null) return;

        // 1. 패에 있는 모든 '빨간색 룬' 인스턴스를 찾습니다.
        var redRuneInstances = selections.Where(inst => inst != null && inst.SO.color == RuneColor.Red);

        // 2. 찾은 빨간 룬들의 효과를 '모든 적'에게 실행합니다.
        foreach (var instance in redRuneInstances)
        {
            if (instance.SO?.effectSO != null)
            {
                Debug.Log($"[RDM] 광역 공격: '{instance.SO.displayName}'(값: {instance.value}) 효과를 모든 적에게 적용.");
                // Execute 함수에 고유 수치(instance.value)와 모든 적(allEnemies) 목록을 전달합니다.
                instance.SO.effectSO.Execute(user, allEnemies, instance.value);
            }
        }

        // 3. (선택사항) 빨간색 룬 외 다른 룬(예: 방어도 룬)의 효과도 실행합니다.
        var otherRuneInstances = selections.Where(inst => inst != null && inst.SO.color != RuneColor.Red && inst.SO != aoeRuneSO);
        foreach (var instance in otherRuneInstances)
        {
            if (instance.SO?.effectSO != null)
            {
                instance.SO.effectSO.Execute(user, allEnemies, instance.value);
            }
        }

        // 4. 모든 효과 실행 후, 턴을 마무리합니다.
        FinalizeTurnAfterAction();
    }
    public void ProcessTargetedAttackComplete(RuneSO usedRepresentativeRune, Enemy targetEnemy)
    {
        var user = Player.Instance;
        var allEnemies = EnemySpawner.Instance?.SpawnedEnemies;
        if (user == null || targetEnemy == null) return;
        List<Enemy> singleTargetList = new List<Enemy> { targetEnemy };
        foreach (var instance in selections)
        {
            if (instance?.SO?.effectSO == null) continue;

            // ▼▼▼ [이 부분을 수정합니다] ▼▼▼
            // 룬의 색상이 Red이거나 || '적 타겟팅 필요' 플래그가 켜져있다면
            if (instance.SO.color == RuneColor.Red || instance.SO.requiresEnemyTarget)
            {
                // 선택한 '단일 타겟'(singleTargetList)에게 효과를 실행
                instance.SO.effectSO.Execute(user, singleTargetList, instance.value);
            }
            else
            {
                // 그 외 모든 룬 (기본 방어, 골드 획득 등)
                instance.SO.effectSO.Execute(user, allEnemies, instance.value);
            }
            // ▲▲▲ 수정 완료 ▲▲▲

            //if (instance.SO.color == RuneColor.Red)
            //{
            //    instance.SO.effectSO.Execute(user, singleTargetList, instance.value);
            //}
            //else
            //{
            //    instance.SO.effectSO.Execute(user, allEnemies, instance.value);
            //}
        }
        FinalizeTurnAfterAction();
    }

    private void ExecuteAllSelectedRunesImmediately()
    {
        var user = Player.Instance;
        var allEnemies = EnemySpawner.Instance?.SpawnedEnemies;
        if (user == null || allEnemies == null) return;
        foreach (var instance in selections)
        {
            instance?.SO?.effectSO?.Execute(user, allEnemies, instance.value);
        }
        FinalizeTurnAfterAction();
    }
    #endregion

    #region Public Deck Modifiers (Missing Methods)
    // --- 아래는 다른 스크립트들이 호출하던, 없어진 함수들을 복구 및 수정한 것입니다 ---

    public void PrepareDeckForNewBattle()
    {
        if (Player.Instance != null) Player.Instance.PrepareForNewBattle();
        playerDeck.AddRange(discardPile);
        discardPile.Clear();
        ClearSelectionsAndPrepareForNextAction();
        colorsToRefillNextTurn.Clear();
    }

    public void ConsolidateDeckPostBattle()
    {
        playerDeck.AddRange(discardPile);
        discardPile.Clear();
    }

    public void CheckAndFlagEmptyColorsForRefill()
    {
        colorsToRefillNextTurn.Clear();
        foreach (RuneColor color in Enum.GetValues(typeof(RuneColor)))
        {
            bool deckIsEmpty = !playerDeck.Any(inst => inst.SO.color == color);
            bool discardHasRunes = discardPile.Any(inst => inst.SO.color == color);
            if (deckIsEmpty && discardHasRunes)
            {
                colorsToRefillNextTurn.Add(color);
            }
        }
    }

    public void RefillFlaggedColorsFromDiscard()
    {
        if (colorsToRefillNextTurn.Count == 0) return;
        foreach (RuneColor color in colorsToRefillNextTurn)
        {
            var runesToRestore = discardPile.Where(inst => inst.SO.color == color).ToList();
            playerDeck.AddRange(runesToRestore);
            discardPile.RemoveAll(inst => inst.SO.color == color);
        }
        colorsToRefillNextTurn.Clear();
        RefreshUI();
    }

    public bool AddRuneToHand(RuneSO runeToAdd)

    {
        if (selectionCount >= selections.Count || runeToAdd == null) return false;
        var newInstance = new RuneInstance(runeToAdd.name, 0); // 패널티 룬은 value가 0이라고 가정
        selections[selectionCount++] = newInstance;
        RefreshUI();
        return true;
    }

    

    public void ReplaceBasicWithReward(RuneColor color, RuneSO rewardRuneSO)
    {
        if (rewardRuneSO == null) return;
        var basicRuneToReplace = playerDeck.FirstOrDefault(inst => inst.SO.isBasicRune && inst.SO.color == color);
        if (basicRuneToReplace == null) return;
        int preservedValue = basicRuneToReplace.value;
        playerDeck.Remove(basicRuneToReplace);
        var newRewardInstance = new RuneInstance(rewardRuneSO.name, preservedValue);
        playerDeck.Add(newRewardInstance);
        SaveDeckState();
        RefreshUI();
    }

    public void AddRuneToDeck(string runeIdentifier)
    {
        var runeToAdd = runeDefinitions.FirstOrDefault(r => r.displayName == runeIdentifier);
        if (runeToAdd == null) return;
        var newInstance = new RuneInstance(runeToAdd.name, Random.Range(1, 11)); // 상점 구매 룬도 랜덤 값 부여
        playerDeck.Add(newInstance);
        SaveDeckState();
    }

    public void ResetDeckToDefault()
    {
        CreateNewDeck();
        RefreshUI();
    }

    [ContextMenu("Delete DeckState.json File")]
    public void DeleteSavedDeckStateFile()
    {
        string deckStatePath = Path.Combine(Application.persistentDataPath, "DeckState.json");

        // 1. 먼저 파일이 존재하는지 확인합니다.
        if (File.Exists(deckStatePath))
        {
            // 2. 파일이 있을 경우에만 파일을 삭제하고 '삭제 완료' 메시지를 출력합니다.
            File.Delete(deckStatePath);
            Debug.Log($"[RuneDeckManager] DeckState.json 파일 삭제 완료: {deckStatePath}");
        }
        else
        {
            // 3. 파일이 없을 경우에는 '파일이 없어 삭제할 수 없다'는 메시지를 출력합니다.
            Debug.Log("[RuneDeckManager] DeckState.json 파일이 없어 삭제할 수 없습니다.");
        }
    }
    #endregion

    /// <summary>
    /// (방패 밀치기 룬 등이 호출할)
    /// 현재 손패의 '예측 총 방어도' 값만 계산하여 반환합니다.
    /// </summary>
    public int GetPredictedTotalDefense()
    {
        int totalDefense = 0;

        // 현재 손패에 뽑힌 룬들(selectionCount 개수만큼)을 순회합니다.
        for (int i = 0; i < selectionCount; i++)
        {
            var instance = selections[i];
            if (instance == null || instance.SO == null) continue;

            // 룬의 색상이 파란색이면, 방어도 합계에 더합니다.
            if (instance.SO.color == RuneColor.Blue)
            {
                totalDefense += instance.value;
            }
        }
        return totalDefense;
    }
    /// <summary>
    /// (미래의 룬 효과가 호출할)
    /// 현재 손패의 '예측 총 데미지' 값만 계산하여 반환합니다.
    /// </summary>
    public int GetPredictedTotalDamage()
    {
        int totalDamage = 0;
        for (int i = 0; i < selectionCount; i++)
        {
            var instance = selections[i];
            if (instance == null || instance.SO == null) continue;

            if (instance.SO.color == RuneColor.Red)
            {
                totalDamage += instance.value;
            }
        }
        return totalDamage;
    }

    // ▼▼▼ [2. 이 함수를 새로 추가] ▼▼▼
    /// <summary>
    /// (미래의 룬 효과가 호출할)
    /// 현재 손패의 '예측 총 골드' 값만 계산하여 반환합니다.
    /// </summary>
    public int GetPredictedTotalGold()
    {
        int totalGold = 0;
        for (int i = 0; i < selectionCount; i++)
        {
            var instance = selections[i];
            if (instance == null || instance.SO == null) continue;

            if (instance.SO.color == RuneColor.Yellow)
            {
                totalGold += instance.value;
            }
        }
        return totalGold;
    }
    // ▲▲▲ 추가 완료 ▲▲▲
    /// <summary>
    /// 현재 손패(selections)에 있는 룬들의 값을 단순 합산하여 UI에 표시합니다.
    /// </summary>
    private void CalculateAndDisplayHandTotals()
    {
        // UIManager가 준비되지 않았으면 실행하지 않습니다.
        if (UIManager.Instance == null || !isUIManagerReady) return;

        int totalDamage = 0;
        int totalDefense = 0;
        int totalGold = 0;

        // 현재 손패에 뽑힌 룬들(selectionCount 개수만큼)을 순회합니다.
        for (int i = 0; i < selectionCount; i++)
        {
            var instance = selections[i];
            if (instance == null || instance.SO == null) continue;

            // 1단계: 룬의 '색상'을 기준으로 단순 합산합니다.
            switch (instance.SO.color)
            {
                case RuneColor.Red:
                    totalDamage += instance.value;
                    break;
                case RuneColor.Blue:
                    totalDefense += instance.value;
                    break;
                case RuneColor.Yellow:
                    totalGold += instance.value;
                    break;
                    // White, Gray는 합산에서 제외합니다.
            }
        }

        // UIManager의 함수를 호출하여 UI를 업데이트합니다.
        UIManager.Instance.UpdatePreviewTotals(totalDamage, totalDefense, totalGold);
    }
    // ▲▲▲ 함수 추가 완료 ▲▲▲

    #region Save/Load & UI
    public void RefreshUI()
    {
        if (!isUIManagerReady || UIManager.Instance == null) return;
        var countsByColor = new Dictionary<RuneColor, int>();
        foreach (RuneColor c in Enum.GetValues(typeof(RuneColor))) { countsByColor[c] = 0; }
        foreach (var instance in playerDeck)
        {
            if (instance.SO != null) countsByColor[instance.SO.color]++;
        }
        UIManager.Instance.UpdateDeckCounts(countsByColor);
        UIManager.Instance.UpdateCentralSlotsWithInstances(selections);
        //bool full = (selectionCount == selections.Count);

        bool canAct = (selectionCount > 0);


        UIManager.Instance.SetDrawButton(canAct);
        UIManager.Instance.SetReRollButton(canAct && !hasRerolledThisTurn);
    }

    public void SaveDeckState()
    {
        var state = new DeckState { playerDeck = this.playerDeck, discardPile = this.discardPile };
        string json = JsonUtility.ToJson(state, true);
        File.WriteAllText(deckStatePath, json);
    }

    public void LoadDeckState()
    {
        if (!File.Exists(deckStatePath)) { CreateNewDeck(); return; }
        string json = File.ReadAllText(deckStatePath);
        var state = JsonUtility.FromJson<DeckState>(json);
        this.playerDeck = state.playerDeck ?? new List<RuneInstance>();
        this.discardPile = state.discardPile ?? new List<RuneInstance>();
    }

    // RuneDeckManager.cs

    public void CreateNewDeck()
    {
        Debug.Log("<color=green>--- CreateNewDeck 함수 실행 시작! ---</color>");

        playerDeck = new List<RuneInstance>();
        discardPile = new List<RuneInstance>();

        Debug.Log("[RDM] 'Rune Definitions' 리스트의 모든 룬을 검색합니다...");

        foreach (var runeSO in runeDefinitions)
        {
            if (runeSO == null) continue;

            if (runeSO.isBasicRune && runeSO.initialDeckCount > 0)
            {
                Debug.Log($"<color=cyan>[조건 만족!] '{runeSO.displayName}'을(를) {runeSO.initialDeckCount}개 덱에 추가합니다.</color>");

                for (int i = 0; i < runeSO.initialDeckCount; i++)
                {
                    // ▼▼▼ 여기에 || runeSO.color == RuneColor.White 조건을 추가했습니다. ▼▼▼
                    int value = (runeSO.color == RuneColor.Red || runeSO.color == RuneColor.Blue || runeSO.color == RuneColor.Yellow || runeSO.color == RuneColor.White) ? i + 1 : 1;
                    playerDeck.Add(new RuneInstance(runeSO.name, value));
                }
            }
        }

        SaveDeckState();
        Debug.Log($"새로운 덱 생성 완료. 총 {playerDeck.Count}개의 룬이 추가되었습니다.");
    }
    /// <summary>
    /// 현재 덱에서 특정 색상의 '기본 룬' 인스턴스 목록만 가져옵니다.
    /// </summary>
    public List<RuneInstance> GetBasicRunesByColor(RuneColor color)
    {
        return playerDeck
            .Where(inst => inst != null && inst.SO != null) // 1단계: 룬 인스턴스 자체와 룬 데이터(SO)가 null이 아닌지 확인
            .Where(inst => inst.SO.isBasicRune && inst.SO.color == color)
            .ToList();
    }

    // ▼▼▼ [새로 추가] 같은 색상의 '모든 룬'을 가져오는 함수 ▼▼▼
    /// <summary>
    /// 현재 덱에서 특정 색상의 '모든 룬' 인스턴스 목록을 가져옵니다. (기본 룬 + 강화 룬 모두)
    /// </summary>
    public List<RuneInstance> GetAllRunesByColor(RuneColor color)
    {
        return playerDeck
            .Where(inst => inst != null && inst.SO != null)
            .Where(inst => inst.SO.color == color) // 'isBasicRune' 조건이 제거되었습니다.
            .ToList();
    }
    // ▲▲▲ 추가 완료 ▲▲▲
    /// <summary>
    /// 선택된 특정 기본 룬을 새로운 보상 룬으로 교체(강화)합니다.
    /// </summary>
    public void EnhanceRune(RuneInstance basicRuneToEnhance, RuneSO rewardRuneSO)
    {
        if (basicRuneToEnhance == null || rewardRuneSO == null) return;
        if (!playerDeck.Contains(basicRuneToEnhance))
        {
            Debug.LogError("강화하려는 룬이 덱에 존재하지 않습니다!");
            return;
        }

        int preservedValue = basicRuneToEnhance.value;
        playerDeck.Remove(basicRuneToEnhance);
        var newRewardInstance = new RuneInstance(rewardRuneSO.name, preservedValue);
        playerDeck.Add(newRewardInstance);

        Debug.Log($"룬 강화 완료: '{basicRuneToEnhance.SO.displayName}'(값:{preservedValue}) -> '{rewardRuneSO.displayName}'(값:{preservedValue})");

        SaveDeckState();
        RefreshUI();
    }


    /// <summary>
    /// 턴 종료 시, 핸드에 있는 모든 패널티 룬의 효과를 발동시키고 제거합니다.
    /// </summary>
    public void ProcessAndRemovePenaltyRunes()
    {
        // 핸드에 룬이 없으면 실행하지 않습니다.
        if (selectionCount == 0) return;

        // 제거되지 않고 남을 일반 룬들을 담을 임시 리스트를 만듭니다.
        List<RuneInstance> runesToKeep = new List<RuneInstance>();
        bool penaltyRuneFound = false;

        // 현재 핸드에 있는 모든 룬을 확인합니다.
        for (int i = 0; i < selectionCount; i++)
        {
            var currentInstance = selections[i];
            if (currentInstance == null) continue;

            // 1. 룬 타입이 'Penalty'인지 확인합니다.
            if (currentInstance.SO.runeType == RuneType.Penalty)
            {
                Debug.Log($"<color=red>패널티 룬 '{currentInstance.SO.displayName}'의 효과를 발동합니다.</color>");

                // 2. 패널티 룬의 효과(EffectSO)를 실행합니다.
                currentInstance.SO.effectSO?.Execute(Player.Instance, null, 0);

                penaltyRuneFound = true;
                // 이 룬은 소멸되므로, 남겨둘 리스트(runesToKeep)에 추가하지 않습니다.
            }
            else
            {
                // 3. 일반 룬이라면, 남겨둘 리스트에 추가합니다.
                runesToKeep.Add(currentInstance);
            }
        }

        // 4. 만약 패널티 룬이 하나라도 있었다면, 핸드 목록을 새로고침합니다.
        if (penaltyRuneFound)
        {
            // 새로운 핸드 리스트를 만들고, 남겨둘 룬들로 채웁니다.
            selections = new List<RuneInstance>(new RuneInstance[5]);
            for (int i = 0; i < runesToKeep.Count; i++)
            {
                selections[i] = runesToKeep[i];
            }
            selectionCount = runesToKeep.Count;

            // UI를 새로고침하여 패널티 룬이 사라진 것을 반영합니다.
            RefreshUI();
        }
    }
    #endregion
}