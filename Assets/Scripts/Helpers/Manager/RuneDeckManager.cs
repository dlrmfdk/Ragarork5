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

    [Header("룬 정의 SO 리스트")]
    public List<RuneSO> runeDefinitions;

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
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        playerDeck = new List<RuneInstance>();
        discardPile = new List<RuneInstance>();
        selections = new List<RuneInstance>(new RuneInstance[5]);
        LoadDeckState();
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
            bool hasRedRunes = selections.Any(instance => instance != null && instance.SO.color == RuneColor.Red);
            if (hasRedRunes)
            {
                PlayerInputManager.Instance.StartEnemyTargeting(selections);
            }
            else
            {
                ExecuteAllSelectedRunesImmediately();
            }
        }
    }


    private void OnReRoll()
    {
        if (!isUIManagerReady || hasRerolledThisTurn) return;

        Debug.Log("리롤을 실행합니다.");

        // ▼▼▼ 리롤 로직 수정 ▼▼▼
        // 패에 있는 룬들을 하나씩 확인합니다.
        for (int i = 0; i < selectionCount; i++)
        {
            var rerolledInstance = selections[i];
            if (rerolledInstance == null) continue;

            // 룬의 색상이 회색이 아닐 경우에만 묘지로 보냅니다.
            if (rerolledInstance.SO.color != RuneColor.Gray)
            {
                discardPile.Add(rerolledInstance);
            }
            else
            {
                // 회색 룬일 경우, 아무것도 하지 않아 묘지로 가지 않고 소멸됩니다.
                Debug.Log($"회색 룬 '{rerolledInstance.SO.displayName}'이(가) 리롤되어 소멸되었습니다.");
            }
        }
        // ▲▲▲ 수정 완료 ▲▲▲

        // 패를 완전히 비우고 다음 행동을 준비합니다.
        ClearSelectionsAndPrepareForNextAction();
    }

    // RuneDeckManager.cs
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
        // ▲▲▲ 수정 완료 ▲▲▲

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
    }
  

    #region Effect Execution
    // RuneDeckManager.cs

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
            if (instance.SO.color == RuneColor.Red)
            {
                instance.SO.effectSO.Execute(user, singleTargetList, instance.value);
            }
            else
            {
                instance.SO.effectSO.Execute(user, allEnemies, instance.value);
            }
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
        bool full = (selectionCount == selections.Count);
        UIManager.Instance.SetDrawButton(full);
        UIManager.Instance.SetReRollButton(full && !hasRerolledThisTurn);
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
        playerDeck = new List<RuneInstance>();
        discardPile = new List<RuneInstance>();

        Debug.Log("[RDM] 새로운 덱 생성을 시작합니다. 'Is Basic Rune'이 체크된 모든 룬을 추가합니다.");

        // 1. runeDefinitions 리스트에 있는 모든 룬 정의를 순회합니다.
        foreach (var runeSO in runeDefinitions)
        {
            // 2. 'Is Basic Rune'이 체크되어 있고, 'Initial Deck Count'가 1 이상인지 확인합니다.
            if (runeSO != null && runeSO.isBasicRune && runeSO.initialDeckCount > 0)
            {
                Debug.Log($"[RDM] 기본 룬 '{runeSO.displayName}'을(를) {runeSO.initialDeckCount}개 추가합니다.");

                // 3. 해당 룬을 initialDeckCount 만큼 덱에 추가합니다.
                for (int i = 0; i < runeSO.initialDeckCount; i++)
                {
                    // 4. 각 룬 인스턴스에 고유한 무작위 값을 부여합니다.
                    //    (유틸리티 룬의 값은 나중에 다른 용도로 사용할 수 있습니다.)
                    playerDeck.Add(new RuneInstance(runeSO.name, Random.Range(1, 11)));
                }
            }
        }

        SaveDeckState();
        Debug.Log($"새로운 덱 생성 완료. 총 {playerDeck.Count}개의 룬이 추가되었습니다.");
    }
    #endregion
}