// GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement; // SceneManager 사용

public class GameManager : MonoBehaviour
{
    public static GameManager Inst { get; private set; }

    [SerializeField] public GameObject nextBt; // 전투 후 다음으로 가는 버튼
    private bool rewardShown = false;
    public bool rewardShownPublic => rewardShown; // 외부 읽기용 접근자

    void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
            // DontDestroyOnLoad(gameObject); // GameManager를 게임 전체 세션 동안 유지하려면 필요합니다.
            // 예를 들어 타이틀 씬에서 생성되고 다른 씬으로 계속 이어질 때.
            // 각 씬(타이틀, 맵, 전투)에 GameManager가 따로 있다면 이 줄은 필요 없습니다.
            // 현재는 씬마다 GameManager가 있을 수 있다고 가정하고 주석 처리합니다.
            // 만약 하나의 GameManager만 사용한다면 주석을 해제하고,
            // 다른 씬에서 중복 생성되지 않도록 아래 else if에서 Destroy 처리합니다.
        }
        else if (Inst != this)
        {
            Debug.LogWarning($"[GameManager] 중복된 GameManager 인스턴스('{this.gameObject.name}')가 있어 파괴합니다. 기존 인스턴스: '{Inst.gameObject.name}'");
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// 새 게임을 시작하는 메서드. 타이틀 화면의 "새 게임 시작" 버튼 등에서 호출됩니다.
    /// </summary>
    public void StartNewGame()
    {
        Debug.Log("[GameManager] StartNewGame 호출됨. 새 게임을 시작합니다.");

        if (RuneDeckManager.Instance != null)
        {
            // 1. 이전 게임의 저장된 덱 상태 파일 삭제
            RuneDeckManager.Instance.DeleteSavedDeckStateFile();

            // 2. 덱을 기본 상태로 리셋 (RuneSO에 정의된 isBasicRune, initialDeckCount 기준)
            RuneDeckManager.Instance.ResetDeckToDefault(); // 이 메서드는 내부적으로 SaveDeckState도 호출합니다.
            Debug.Log("[GameManager] 룬 덱이 기본 상태로 리셋되었고, 저장 파일이 삭제되었습니다.");
        }
        else
        {
            Debug.LogError("[GameManager] RuneDeckManager.Instance를 찾을 수 없습니다! 덱 초기화 실패.");
        }

        // 3. 보상 관련 상태 등 다른 게임 상태 변수 초기화
        rewardShown = false;
        // Player.Instance.ResetPlayerStats(); // 예시: 플레이어 스탯 초기화

        // 4. 게임의 첫 번째 실제 플레이 씬 로드 (예: "MapScene")
        Debug.Log("[GameManager] 첫 번째 게임 씬(MapScene)으로 이동합니다.");
        SceneManager.LoadScene("MapScene"); // 또는 실제 게임 시작 씬 이름으로 변경
    }

    // 전투 종료 후 보상 UI가 표시되었음을 설정하는 메서드 (TurnManager 등에서 호출 가능)
    public void SetRewardShown(bool status)
    {
        rewardShown = status;
        if (status && nextBt != null) // 보상이 표시/처리되면 다음 버튼 자동 활성화 (선택적 로직)
        {
            // nextBt.SetActive(true); // 이 로직은 전투 승리 시 TurnManager.HandleBattleEnd에서 직접 제어하는 것이 나을 수 있음
        }
    }


    void Update()
    {
#if UNITY_EDITOR // 개발 중에만 작동
        InputCheatKey();
#endif

        // 모든 적이 처치되었는지 확인하는 로직은 TurnManager.IsBattleOver()로 이동하고,
        // 전투 종료 처리는 TurnManager.HandleBattleEnd()에서 담당하는 것이 더 적절합니다.
        // GameManager.Update()는 게임 전체 상태에 따른 범용적인 업데이트에 집중하는 것이 좋습니다.
        // 현재 EnemySpawner.Instance.SpawnedEnemies.Count == 0 조건은 TurnManager.IsBattleOver()에서 이미 사용 중입니다.
        // nextBt 활성화 및 RewardManager 호출도 TurnManager.HandleBattleEnd()에서 처리하도록 이전했습니다.
    }

    void InputCheatKey() //개발자용 치트
    {
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (TurnManager.Inst != null && TurnManager.Inst.myTurn) // 플레이어 턴일 때만 작동하도록
            {
                TurnManager.Inst.EndTurn();
            }
        }
    }
}