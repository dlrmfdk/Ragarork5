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

    [Header("특별 룬 SO")]
    [Tooltip("광역 공격 규칙을 발동시키는 '광역의 룬' SO를 여기에 할당하세요.")]
    [SerializeField] private RuneSO aoeRuneSO;
    [Tooltip("피해 흡혈 규칙을 발동시키는 '피해 흡혈의 룬' SO를 여기에 할당하세요.")]
    [SerializeField] private RuneSO lifestealRuneSO;
    [Tooltip("흡혈 비율 (예: 1.0은 100%, 0.5는 50%)")]
    private float lifestealRatio = 1.0f;

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
        if (!isUIManagerReady || Player.Instance == null) return;

        BattleContext.Reset(); // 행동 시작 시 피해량 카운터 초기화

        bool isAoeAttack = (aoeRuneSO != null && selections.Contains(aoeRuneSO));

        if (isAoeAttack)
        {
            Debug.Log("[RDM.OnDrawClick] '광역의 룬' 발견! 광역 공격을 실행합니다.");
            ExecuteAoeRuneLogic();
        }
        else
        {
            List<RuneSO> redRunes = selections.Where(so => so != null && so.color == RuneColor.Red).ToList();
            if (redRunes.Any())
            {
                Debug.Log($"[RDM.OnDrawClick] {redRunes.Count}개의 빨간색 룬 발견. 타겟팅을 시작합니다.");
                if (PlayerInputManager.Instance != null)
                {
                    PlayerInputManager.Instance.StartEnemyTargeting(redRunes);
                }
            }
            else
            {
                Debug.Log("[RDM.OnDrawClick] 타겟팅할 빨간색 룬이 없습니다. 선택된 모든 룬을 즉시 실행합니다.");
                ExecuteAllSelectedRunesImmediately();
            }
        
    
    }
}
    /// <summary>
    /// "광역의 룬" 효과를 처리하는 새로운 함수
    /// </summary>
    private void ExecuteAoeRuneLogic()
    {
        var user = Player.Instance;
        var allEnemies = EnemySpawner.Instance?.SpawnedEnemies;

        if (user == null || allEnemies == null) return;

        // 1. 함께 선택된 빨간색 룬들의 효과를 모든 적에게 적용
        List<RuneSO> redRunes = selections.Where(so => so != null && so.color == RuneColor.Red).ToList();
        if (redRunes.Any())
        {
            Debug.Log($"{redRunes.Count}개의 빨간 룬 효과를 모든 적에게 광역으로 적용합니다.");
            foreach (var redRune in redRunes)
            {
                if (redRune.effectSO != null)
                {
                    redRune.effectSO.Execute(user, allEnemies);
                }
            }
        }
        else
        {
            Debug.Log("광역의 룬과 함께 사용된 빨간 룬이 없어 광역 공격 효과가 발동되지 않았습니다.");
        }

        // 2. 함께 선택된 다른 색(비-빨강, 비-광역룬) 룬들의 효과도 실행
        List<RuneSO> otherRunes = selections.Where(so => so != null && so.color != RuneColor.Red && so != aoeRuneSO).ToList();
        if (otherRunes.Any())
        {
            Debug.Log($"{otherRunes.Count}개의 다른 색 룬 효과를 실행합니다.");
            foreach (var otherRune in otherRunes)
            {
                if (otherRune.effectSO != null)
                {
                    otherRune.effectSO.Execute(user, allEnemies);
                }
            }
        }

        // 3. 모든 룬 사용 후 턴 종료 처리
        FinalizeTurnAfterAction();
    }
    /// <summary>
    /// 턴 종료 처리를 위한 공용 헬퍼 함수 (중복 제거)
    /// </summary>
    private void FinalizeTurnAfterAction()
    {
        Debug.Log("모든 룬 효과 처리 완료. 후처리 효과 및 턴 정리를 시작합니다.");

        // ▼▼▼ 피해 흡혈 효과 처리 로직 추가 ▼▼▼
        if (lifestealRuneSO != null && selections.Contains(lifestealRuneSO))
        {
            int totalDamage = BattleContext.TotalDamageDealtThisAction;
            if (totalDamage > 0)
            {
                int healAmount = Mathf.FloorToInt(totalDamage * lifestealRatio);
                if (healAmount > 0 && Player.Instance != null)
                {
                    Debug.Log($"[피해 흡혈 룬] 총 피해량 {totalDamage}의 {lifestealRatio * 100}%인 {healAmount}만큼 체력을 회복합니다.");
                    Player.Instance.Heal(healAmount);
                }
            }
        }
        // ▲▲▲ 로직 추가 완료 ▲▲▲

        // 사용된 룬 묘지로 보내기
        for (int i = 0; i < selectionCount; i++)
        {
            if (selections[i] != null)
            {
                discardPile.Add(selections[i]);
            }
        }

        ClearSelectionsAndPrepareForNextAction();
        SaveDeckState();

        if (TurnManager.Inst != null)
        {
            TurnManager.Inst.EndTurn();
        }
    }

    // RuneDeckManager.cs

    // 기존의 비어있던 함수를 아래 내용으로 교체합니다.
    public void ProcessTargetedAttackComplete(RuneSO usedRepresentativeRune, Enemy targetEnemy)
    {
        Debug.Log($"[RDM.ProcessTargetedAttackComplete] 타겟팅 공격 완료. 룬 효과를 '{targetEnemy.EnemyData.EnemyName}'에게 적용합니다.");

        var user = Player.Instance;

        // 1. 현재 패(selections)에서 모든 빨간 룬을 찾습니다.
        List<RuneSO> redRunes = selections.Where(so => so != null && so.color == RuneColor.Red).ToList();

        // 2. 찾은 모든 빨간 룬의 효과를 선택된 타겟(targetEnemy)에게 실행합니다.
        if (redRunes.Any())
        {
            // effectSO.Execute의 두 번째 인자는 List<Enemy> 타입이므로, 단일 타겟을 리스트로 만들어 전달합니다.
            List<Enemy> singleTargetList = new List<Enemy> { targetEnemy };

            foreach (var redRune in redRunes)
            {
                if (redRune.effectSO != null)
                {
                    redRune.effectSO.Execute(user, singleTargetList);
                }
            }
        }

        // 3. 공격 후, 빨간색이 아닌 다른 룬들의 효과를 실행합니다.
        List<RuneSO> otherRunes = selections.Where(so => so != null && so.color != RuneColor.Red).ToList();
        if (otherRunes.Any())
        {
            var allEnemies = EnemySpawner.Instance?.SpawnedEnemies;
            foreach (var otherRune in otherRunes)
            {
                if (otherRune.effectSO != null)
                {
                    // 다른 룬들은 모든 적을 대상으로 할 수 있으므로 allEnemies를 전달합니다.
                    otherRune.effectSO.Execute(user, allEnemies);
                }
            }
        }

        // 4. 모든 룬 효과 처리 후 턴을 종료하는 공용 함수를 호출합니다.
        FinalizeTurnAfterAction();

    }
    // 빨간색 룬이 아닌, 즉시 실행될 룬들을 처리하고 턴을 종료하는 함수

    private void ExecuteAllSelectedRunesImmediately()
    {
        var user = Player.Instance;
        if (user == null)
        {
            Debug.LogError("[RDM.ExecuteAllSelectedRunesImmediately] Player.Instance is null!");
            return;
        }

        var targets = EnemySpawner.Instance != null ? EnemySpawner.Instance.SpawnedEnemies : new List<Enemy>(); // null 대신 빈 리스트 전달 고려
        bool effectWasExecuted = false;

        for (int i = 0; i < selectionCount; i++)
        {
            var so = selections[i]; // selections는 RuneSO[] 또는 List<RuneSO>일 수 있음
            if (so != null)
            {
                if (so.effectSO == null)
                {
                    Debug.LogError($"[RDM.ExecuteAllSelectedRunesImmediately] {so.name}의 effectSO가 null입니다!");
                    continue;
                }
                Debug.Log($"[RDM.ExecuteAllSelectedRunesImmediately] 룬 '{so.displayName}' 효과 실행.");
                so.effectSO.Execute(user, targets); // targets가 null이어도 effectSO 내부에서 처리할 수 있도록 설계
                discardPile.Add(so);
                effectWasExecuted = true;
            }
        }

        // 효과 실행 후 선택 슬롯 정리 및 상태 저장, 턴 종료
        ClearSelectionsAndPrepareForNextAction(); // 이 함수는 RefreshUI를 호출함
        SaveDeckState(); // 덱 상태 저장

        if (TurnManager.Inst != null)
        {
            // 효과가 실제로 실행되었거나, "Draw" 액션 자체가 턴을 소모하는 경우 턴 종료
            // 여기서는 effectWasExecuted 여부와 관계없이 Draw 액션 후 턴을 종료하는 것으로 가정
            Debug.Log("[RDM.ExecuteAllSelectedRunesImmediately] 모든 비-빨강 룬 실행/처리 완료. 턴을 종료합니다.");
            TurnManager.Inst.EndTurn();
        }
        else
        {
            Debug.LogError("[RDM.ExecuteAllSelectedRunesImmediately] TurnManager.Inst is null! 턴을 종료할 수 없습니다.");
        }
        // RefreshUI()는 ClearSelectionsAndPrepareForNextAction 내부에서 이미 호출되었으므로,
        // TurnManager.EndTurn() 이후에 특별히 상태 변화를 다시 반영해야 한다면 여기서 또 호출할 수 있습니다.
        // (일반적으로 EndTurn 후에는 다음 턴 준비가 되므로, 현재 RefreshUI 위치도 괜찮아 보입니다.)
    }

    // 선택 슬롯을 비우고 다음 행동을 준비하는 (리롤 상태 초기화 등) 함수
    public void ClearSelectionsAndPrepareForNextAction()
    {
        selections = new List<RuneSO>(new RuneSO[5]); // 새 리스트로 초기화 (또는 모든 요소 null로)
        selectionCount = 0;
        hasRerolledThisTurn = false;
        // SaveDeckState(); // 턴이 종료될 때 저장하는 것이 더 적합할 수 있음 (OnDrawClick의 원래 위치 또는 EndTurn에서)
        RefreshUI();
    }


    // RuneDeckManager.cs

    private void OnReRoll()
    {
        if (!isUIManagerReady)
        {
            Debug.LogWarning("[RDM.OnReRoll] UIManager가 아직 준비되지 않아 작업을 건너뜁니다.");
            return;
        }

        if (hasRerolledThisTurn) return;

   

        // 1. 패에 남겨둘 패널티 룬을 저장할 임시 리스트를 만듭니다.
        List<RuneSO> penaltyRunesToKeep = new List<RuneSO>();

        // 2. 현재 패(selections)에 있는 룬들을 하나씩 확인합니다.
        for (int i = 0; i < selectionCount; i++)
        {
            RuneSO currentRune = selections[i];
            if (currentRune == null) continue;

            // 3. 룬의 타입을 확인합니다.
            if (currentRune.runeType == RuneType.Penalty)
            {
                // 패널티 룬이면, 나중에 패에 다시 넣기 위해 임시 리스트에 추가합니다.
                penaltyRunesToKeep.Add(currentRune);
                Debug.Log($"[RDM.OnReRoll] 패널티 룬 '{currentRune.displayName}'은 패에 남깁니다.");
            }
            else
            {
                // 일반 룬이면, 묘지(discardPile)로 보냅니다.
                discardPile.Add(currentRune);
            }
        }

        // 4. 패를 완전히 새로 구성합니다.
        // 먼저, 최대 크기(5)만큼 빈 슬롯으로 채워진 새 리스트를 만듭니다.
        selections = new List<RuneSO>(new RuneSO[5]);

        // 5. 남겨두었던 패널티 룬들을 새 리스트의 앞쪽부터 다시 채워 넣습니다.
        for (int i = 0; i < penaltyRunesToKeep.Count; i++)
        {
            selections[i] = penaltyRunesToKeep[i];
        }

        // 6. 현재 패에 있는 룬의 수를 업데이트합니다.
        selectionCount = penaltyRunesToKeep.Count;

        // 7. 이번 턴에 리롤을 사용했음을 기록합니다.
        hasRerolledThisTurn = true;

        // 8. 변경된 패의 상태를 UI에 즉시 반영합니다.
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
            if (kv.Key != null)
            {
                countsByColor[kv.Key.color] += kv.Value;
            }
        }

        UIManager.Instance.UpdateDeckCounts(countsByColor);
        UIManager.Instance.UpdateCentralSlotsWithSO(selections); 

        // 버튼 활성화 로직 
        bool canInteractWithButtons = true; // 기본적으로는 상호작용 가능
        if (PlayerInputManager.Instance != null && PlayerInputManager.Instance.IsTargetingMode)
        {
            canInteractWithButtons = false; // 타겟팅 중에는 버튼 상호작용 불가
        }

        bool full = (selectionCount == selections.Count); // selections.Count는 중앙 슬롯의 최대 크기 (예: 5)

        // UIManager의 SetDrawButton과 SetReRollButton은 null 체크를 이미 내부에서 하므로 여기서 Instance null 체크는 생략 가능
        UIManager.Instance.SetDrawButton(canInteractWithButtons && full); //
        UIManager.Instance.SetReRollButton(canInteractWithButtons && full && !hasRerolledThisTurn);
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

        //플레이어 상태 초기화 호출 추가
        if (Player.Instance != null)
        {
            Player.Instance.PrepareForNewBattle();
        }
        else
        {
            Debug.LogError("[RDM.PrepareDeckForNewBattle] Player.Instance가 null이라 플레이어 상태를 초기화할 수 없습니다.");
        }
   

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


    // (기존 PrepareDeckForNewBattle() 메서드는 그대로 유지합니다.
    //  TurnManager.GameLoop() 시작 시 호출되어 새 전투의 패 초기화 등을 담당합니다.)

    /// <summary>
    /// 전투 종료 후 호출되어 묘지의 룬들을 덱 카운트로 복원합니다.
    /// </summary>
    public void ConsolidateDeckPostBattle()
    {
        Debug.Log("[RDM.ConsolidateDeckPostBattle] 전투 후 덱 정리 시작 (묘지 -> 덱 카운트 복원)");
        if (discardPile != null && discardPile.Any())
        {
            int restoredCount = 0;
            // 묘지에 있는 각 룬에 대해 반복 (ToList()로 복사본을 만들어 순회 중 컬렉션 변경 문제 방지)
            List<RuneSO> runesToRestore = new List<RuneSO>(discardPile);
            discardPile.Clear(); // 묘지를 먼저 비웁니다.

            foreach (var runeSO_instance in runesToRestore)
            {
                if (runeSO_instance != null)
                {
                    if (deckCounts.ContainsKey(runeSO_instance))
                    {
                        deckCounts[runeSO_instance]++;
                        restoredCount++;
                    }
                    else
                    {
                        // 이 경우는 이론적으로 발생하면 안 됩니다 (덱에서 나온 룬이므로).
                        deckCounts[runeSO_instance] = 1;
                        restoredCount++;
                        Debug.LogWarning($"[RDM.ConsolidateDeckPostBattle] 묘지의 룬 '{runeSO_instance.name}'이 deckCounts에 없어 1로 추가합니다.");
                    }
                }
            }
            Debug.Log($"[RDM.ConsolidateDeckPostBattle] 묘지에서 {restoredCount}개의 룬을 덱 카운트로 복원했습니다. 현재 묘지 크기: {discardPile.Count}");
        }
        else
        {
            Debug.Log("[RDM.ConsolidateDeckPostBattle] 묘지가 비어있어 복원할 룬이 없습니다.");
        }

        // 중요: 이 시점에서는 패(selections)나 리롤 상태는 건드리지 않습니다.
        // 오직 deckCounts만 갱신하여 보상 선택 시 정확한 덱 상태를 반영하도록 합니다.
        // SaveDeckState()도 여기서 호출하지 않습니다. 보상 선택 후 ReplaceBasicWithReward에서 저장합니다.

        Debug.Log("[RDM.ConsolidateDeckPostBattle] 전투 후 덱 정리 완료. 현재 덱 카운트 상태:");
        foreach (var kvp in deckCounts)
        {
            if (kvp.Key != null)
            {
                Debug.Log($" - {kvp.Key.displayName} ({kvp.Key.name}): {kvp.Value}개");
            }
        }

        // 만약 이 시점에 덱 카운트 UI를 즉시 갱신하고 싶다면 RefreshUI()를 호출할 수 있으나,
        // 보상 화면으로 넘어가므로 필수는 아닐 수 있습니다.
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

    // 상점에서 룬 구매 시 호출될 범용 함수
    public void AddRuneToDeck(string runeIdentifier)
    {
        // 1. 모든 룬 목록(allRunesList)에서 ID나 이름에 맞는 RuneSO 찾기
        // 여기서는 runeIdentifier가 displayName과 일치한다고 가정합니다.
        RuneSO runeToAdd = runeDefinitions.FirstOrDefault(r => r.displayName == runeIdentifier);

        if (runeToAdd != null)
        {
            // 2. 플레이어의 덱(playerDeck)에 추가
            // 가지고 계신 덱 리스트 변수명에 맞게 'playerDeck'을 수정하세요.
            runeDefinitions.Add(runeToAdd);
            Debug.Log($"[RuneDeckManager] {runeToAdd.displayName}을(를) 덱에 추가했습니다.");

            // 3. 변경된 덱 상태 저장 (필요 시)
            // SaveDeckState(); 
        }
        else
        {
            Debug.LogError($"[RuneDeckManager] ID(이름) '{runeIdentifier}'에 해당하는 룬을 찾을 수 없습니다.");
        }
    }

    //슬롯 정보 접근 함수 추가
    /// <summary>
    /// 지정된 인덱스의 중앙 슬롯에 있는 RuneSO를 반환합니다.
    /// </summary>
    public RuneSO GetRuneInSelection(int index)
    {
        if (selections != null && index >= 0 && index < selectionCount)
        {
            return selections[index];
        }
        return null;
    }


    /// <summary>
    /// 외부에서 플레이어의 패(selections)에 특정 룬을 강제로 추가합니다.
    /// 패가 가득 차 있으면 실패합니다.
    /// </summary>
    /// <param name="runeToAdd">패에 추가할 룬의 ScriptableObject</param>
    /// <returns>추가 성공 시 true, 실패 시 false</returns>
    public bool AddRuneToHand(RuneSO runeToAdd)
    {
        // 패가 가득 찼는지 확인
        if (selectionCount >= selections.Count)
        {
            Debug.LogWarning("[RDM.AddRuneToHand] 패가 가득 차 있어 룬을 추가할 수 없습니다.");
            return false;
        }

        // 추가할 룬이 유효한지 확인
        if (runeToAdd == null)
        {
            Debug.LogError("[RDM.AddRuneToHand] 추가하려는 룬(runeToAdd)이 null입니다.");
            return false;
        }

        // 패의 다음 빈 자리에 룬을 추가
        selections[selectionCount] = runeToAdd;
        // 패에 들어온 룬의 수를 1 증가
        selectionCount++;

        Debug.Log($"[RDM.AddRuneToHand] '{runeToAdd.displayName}' 룬이 패에 강제로 추가되었습니다.");

        // UI를 새로고침하여 화면에 즉시 반영
        RefreshUI();

        return true;
    }
    /// <summary>
    /// 플레이어의 패(selections)에 있는 모든 패널티 룬을 즉시 제거(파괴)합니다.
    /// 이 룬들은 묘지로 가지 않습니다.
    /// </summary>
    public void RemoveAllPenaltyRunesFromHand()
    {
        if (selectionCount == 0) return; // 패가 비어있으면 실행 안함

        // 1. 패널티 룬이 아닌 일반 룬만 담을 새 리스트를 만듭니다.
        List<RuneSO> newSelections = new List<RuneSO>();
        int removedCount = 0;

        // 2. 현재 패를 순회하며 일반 룬만 새 리스트에 추가합니다.
        for (int i = 0; i < selectionCount; i++)
        {
            RuneSO currentRune = selections[i];
            if (currentRune != null && currentRune.runeType != RuneType.Penalty)
            {
                newSelections.Add(currentRune);
            }
            else if (currentRune != null)
            {
                removedCount++;
            }
        }

        // 3. 패널티 룬이 1개 이상 제거되었다면 패를 업데이트합니다.
        if (removedCount > 0)
        {
            Debug.Log($"[RDM.RemoveAllPenaltyRunesFromHand] 패에서 {removedCount}개의 패널티 룬을 파괴했습니다.");

            // 4. 새 리스트를 최대 크기(5)에 맞게 빈 슬롯으로 채웁니다.
            while (newSelections.Count < 5)
            {
                newSelections.Add(null);
            }

            // 5. 기존 패(selections)를 새로운 패로 교체하고, 룬 개수도 업데이트합니다.
            selections = newSelections;
            selectionCount -= removedCount;

            // 6. 변경 사항을 UI에 반영합니다.
            RefreshUI();
        }
    }
}

