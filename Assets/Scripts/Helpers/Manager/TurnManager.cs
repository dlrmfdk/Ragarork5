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

            // 3. 적 턴을 시작하기 전, 1초간 '연출'을 위해 대기합니다.
            yield return new WaitForSeconds(1.0f); // (이 시간을 0.5f ~ 1.5f 사이로 조절하세요)

            yield return StartCoroutine(EnemyTurn());
            // ▼▼▼ 이 부분이 핵심 수정 사항입니다 ▼▼▼

            // 적 턴 직후 승리 확인

            if (IsBattleOver())

            {

                // 적 턴이 끝났을 때 적이 죽은 것은 '플레이어 승리'입니다.

                HandleBattleEnd(true);

                yield break;

            }



            // 적 턴 직후 플레이어 사망 확인

            if (Player.Instance.CurrentHealth <= 0)

            {

                HandleBattleEnd(false); // 플레이어 패배

                yield break;

            }

            // ▲▲▲ 수정 완료 ▲▲▲
        }
    }

    private IEnumerator PlayerTurn()
    {
        Debug.Log("[PlayerTurn] 시작");
        // ▼▼▼ 이 부분을 수정합니다 ▼▼▼
        // 턴 시작 시 플레이어의 방어도를 처리합니다 (유지 또는 초기화).
        if (Player.Instance != null)
        {
            Player.Instance.ProcessTurnStartDefense(); // ResetDefense 대신 이 함수 호출
        }
        // ▲▲▲ 수정 완료 ▲▲▲

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
            // ▼▼▼ [핵심 수정] 리스트 복사본 사용 및 루프 내 사망 처리 ▼▼▼
            var enemiesPerformingTurn = new List<Enemy>(enemySpawner.SpawnedEnemies); // 턴 시작 시점의 적 목록 복사

            foreach (var enemy in enemiesPerformingTurn)
            {
                // 1. 적이 이미 죽었거나 비활성화 상태면 건너<0xEB><0x9C><0x84>
                if (enemy == null || !enemy.gameObject.activeInHierarchy || enemy.currentHealth <= 0)
                {
                    continue;
                }

                // 2. 적의 행동 코루틴 실행
                yield return StartCoroutine(enemy.PerformTurn());

                // 3. [중요] 적이 방금 행동(특히 상태이상 데미지)으로 죽었는지 확인
                if (enemy != null && enemy.currentHealth <= 0)
                {
                    Debug.Log($"[EnemyTurn] {enemy.name}이(가) 자신의 턴 행동 중 사망 감지. DieSequence 완료 대기.");
                    // 적의 DieSequence 코루틴이 완전히 끝날 때까지 기다립니다.
                    // (DieSequence 내부에서 리스트 제거 및 오브젝트 파괴가 일어남)
                    // -> Enemy 스크립트가 DieSequence 코루틴 핸들을 반환하도록 수정하거나,
                    // -> 충분한 시간(애니메이션 시간 이상)을 기다려 줍니다. 여기서는 후자를 사용.
                    //    (DieSequence의 애니메이션 대기 시간 + 약간의 여유)
                    yield return new WaitForSeconds(1.5f); // 예: DieSequence 애니메이션 시간이 1초라면 1.5초 대기

                    // 4. [중요] 한 명이 죽었으므로, 전투가 끝났는지 '즉시' 확인
                    if (IsBattleOver())
                    {
                        Debug.Log("[EnemyTurn] 루프 중 전투 종료 감지됨.");
                        // HandleBattleEnd는 GameLoop에서 처리하므로 여기서는 루프만 탈출
                        isLoading = false; // 로딩 상태 해제
                        yield break; // EnemyTurn 코루틴 종료
                    }
                }
            }
            // ▲▲▲ 수정 완료 ▲▲▲
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