// TurnManager.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Inst { get; private set; }

    public bool isLoading { get; private set; }
    public bool myTurn { get; private set; }
    public static event Action<bool> OnTurnStarted;

    [Header("적 스포너")]
    [SerializeField] private EnemySpawner enemySpawner;

    void Awake()
    {
        if (Inst == null) Inst = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        Debug.Log("[TurnManager.GameLoop] 새 전투 사이클 시작.");

        // 1. 새 전투를 위해 덱 준비
        if (RuneDeckManager.Instance != null)
        {
            RuneDeckManager.Instance.PrepareDeckForNewBattle();
        }
        else
        {
            Debug.LogError("[TurnManager.GameLoop] RuneDeckManager.Instance가 null입니다! 덱을 준비할 수 없습니다.");
            yield break;
        }

        // 2. 현재 전투에 맞는 적 스폰
        if (enemySpawner != null)
        {
            enemySpawner.SpawnRandomEnemies(); // 적 스폰 메서드 호출
        }
        else
        {
            Debug.LogError("[TurnManager.GameLoop] EnemySpawner가 할당되지 않았습니다!");
            yield break;
        }

        //3.적 스폰 직후, 각 적의 첫 행동을 결정
        if (enemySpawner != null && enemySpawner.SpawnedEnemies != null)
        {
            foreach (var enemy in enemySpawner.SpawnedEnemies)
            {
                enemy.ChooseNextAction();
            }
        }

        // 4.전투 시작 직전 UI 갱신
        // 이 시점에는 해당 씬의 UIManager가 준비되어 OnUIManagerReady 이벤트가 발생했고,
        // RuneDeckManager.HandleUIManagerReady가 호출되어 isUIManagerReady가 true로 설정되었을 것으로 기대합니다.
        if (RuneDeckManager.Instance != null && RuneDeckManager.Instance.isUIManagerReady && UIManager.Instance != null)
        {
            Debug.Log("[TurnManager.GameLoop] 전투 시작 전 RefreshUI 호출합니다.");
            RuneDeckManager.Instance.RefreshUI(); // 덱 카운트 등 최신 정보로 UI 갱신
            UIManager.Instance.ShowRuneUI();      // 전투 UI 패널들이 보이도록 확실히 처리
        }
        else
        {
            Debug.LogWarning("[TurnManager.GameLoop] 전투 시작 전 RefreshUI 호출 시도 실패: RuneDeckManager.Instance가 null이거나 UIManager가 아직 준비되지 않음.");
        }

        // 5. 전투 내 턴 반복
        while (true)
        {
            yield return StartCoroutine(PlayerTurn());
            if (IsBattleOver())
            {
                HandleBattleEnd(true); // 플레이어 승리 (예시)
                yield break;
            }

            yield return StartCoroutine(EnemyTurn());
            if (IsBattleOver()) // 예: 플레이어 체력 확인
            {
                HandleBattleEnd(false); // 플레이어 패배 (예시)
                yield break;
            }
        }
    }

    private IEnumerator PlayerTurn()
    {
        Debug.Log("[PlayerTurn] 시작");
        // 턴 시작 시 플레이어의 방어도를 초기화합니다.
        if (Player.Instance != null)
        {
            Player.Instance.ResetDefense();
        }

        isLoading = true;
        myTurn = true;
        OnTurnStarted?.Invoke(myTurn);

        if (RuneDeckManager.Instance != null && UIManager.Instance != null && RuneDeckManager.Instance.isUIManagerReady)
        {
            RuneDeckManager.Instance.RefillFlaggedColorsFromDiscard(); // 내부에서 RefreshUI 호출 가능성 있음
            // 추가적인 RefreshUI 호출이 필요하다면 여기에, 또는 GameLoop에서 한 것으로 충분할 수 있음.
            // RuneDeckManager.Instance.RefreshUI(); 
            UIManager.Instance.ShowRuneUI(); // RuneDeckPanel, CentralSlotPanel 활성화
        }
        else
        {
            Debug.LogWarning("[PlayerTurn] RuneDeckManager 또는 UIManager가 준비되지 않아 UI 관련 작업을 완전히 수행할 수 없습니다.");
        }

        while (myTurn)
        {
            yield return null;
        }
        Debug.Log("[PlayerTurn] myTurn=false, 적 턴으로 넘어갑니다");
        isLoading = false;
    }

    
    private IEnumerator EnemyTurn()
    {
        isLoading = true;
        myTurn = false;
        OnTurnStarted?.Invoke(myTurn);

        if (UIManager.Instance != null && RuneDeckManager.Instance != null && RuneDeckManager.Instance.isUIManagerReady)
        {
            UIManager.Instance.HideRuneUI();
        }

        if (enemySpawner != null && enemySpawner.SpawnedEnemies != null)
        {
            var currentEnemies = new List<Enemy>(enemySpawner.SpawnedEnemies);
            foreach (var e in currentEnemies)
            {
                if (e != null && e.gameObject.activeInHierarchy && e.currentHealth > 0)
                {
                    yield return StartCoroutine(e.PerformTurn());
                }
            }
        }
        isLoading = false;
    }

    public void EndTurn()
    {
        if (RuneDeckManager.Instance != null)
        {
            // 기존의 Remove... 함수 대신 새로운 Process... 함수를 호출합니다.
            RuneDeckManager.Instance.ProcessAndRemovePenaltyRunes();
        }

        Debug.Log("[TurnManager] EndTurn() 호출됨, myTurn 이전값=" + myTurn);
        if (!myTurn) return;
        myTurn = false;

        if (RuneDeckManager.Instance != null)
        {
            RuneDeckManager.Instance.CheckAndFlagEmptyColorsForRefill();
        }
        Debug.Log("[TurnManager] myTurn 설정 후값=" + myTurn + ", CheckAndFlagEmptyColorsForRefill() 호출 완료.");
    }

    private bool IsBattleOver()
    {
        if (enemySpawner != null && enemySpawner.SpawnedEnemies != null &&
            !enemySpawner.SpawnedEnemies.Any(e => e != null && e.gameObject.activeInHierarchy && e.currentHealth > 0))
        {
            Debug.Log("[TurnManager.IsBattleOver] 모든 적 사망 감지.");
            return true;
        }
        // if (Player.Instance != null && Player.Instance.currentHealth <= 0) { /* 플레이어 사망 처리 */ return true; }
        return false;
    }

    private void HandleBattleEnd(bool playerWon)
    {
        Debug.Log($"[TurnManager.HandleBattleEnd] 전투 종료. 플레이어 승리: {playerWon}");
        isLoading = true; // 추가 입력 방지

        if (UIManager.Instance != null && RuneDeckManager.Instance != null && RuneDeckManager.Instance.isUIManagerReady)
        {
            UIManager.Instance.HideRuneUI();
        }

        if (playerWon)
        {
            // ★★★ 전투 종료 후, 보상 전 덱 상태 정리 (묘지 -> 덱 카운트로) ★★★
            if (RuneDeckManager.Instance != null)
            {
                RuneDeckManager.Instance.ConsolidateDeckPostBattle();
            }
            else
            {
                Debug.LogError("[TurnManager.HandleBattleEnd] RuneDeckManager.Instance가 null입니다. 전투 후 덱 정리를 할 수 없습니다.");
            }

            // 이제 보상 UI 표시 요청
            if (RewardManager.Instance != null && GameManager.Inst != null && !GameManager.Inst.rewardShownPublic)
            {
                Debug.Log("[TurnManager.HandleBattleEnd] 보상 UI 표시 요청.");
                RewardManager.Instance.ShowRewardPanel();
                GameManager.Inst.SetRewardShown(true); // GameManager에 보상 표시되었음을 알림
            }
            else if (GameManager.Inst != null && GameManager.Inst.nextBt != null)
            {
                Debug.Log("[TurnManager.HandleBattleEnd] 다음 진행 버튼(nextBt) 활성화 시도.");
                GameManager.Inst.nextBt.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[TurnManager.HandleBattleEnd] RewardManager 또는 GameManager의 참조가 없어 보상/다음 단계 처리를 못했습니다.");
            }
        }
        else // 플레이어 패배
        {
            Debug.Log("[TurnManager.HandleBattleEnd] 플레이어 패배. 게임 오버 처리.");
            // SceneManager.LoadScene("GameOverScene"); // 예시
        }
        // 이 GameLoop 코루틴은 여기서 종료되므로, 씬 전환은 RewardManager의 버튼이나 GameManager에서 처리
    }
}