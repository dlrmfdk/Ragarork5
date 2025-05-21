using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Inst { get; private set; }

    /// <summary>턴 진행 중(true)에는 입력을 막습니다.</summary>
    public bool isLoading { get; private set; }

    /// <summary>현재 플레이어 턴 여부</summary>
    public bool myTurn { get; private set; }

    /// <summary>플레이어 턴이 시작될 때마다 호출됩니다. 파라미터는 myTurn 플래그입니다.</summary>
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
        // 게임 루프 시작
        StartCoroutine(GameLoop());
    }

    /// <summary>
    /// 무한 루프: 플레이어 턴 → 적 턴 → 플레이어 턴 → ... 무한으로 돌려도되나?
    /// </summary>
    private IEnumerator GameLoop()
    {
        // 처음엔 적 스폰
        enemySpawner.SpawnRandomEnemies();

        while (true)
        {

            RuneDeckManager.Instance.ResetDeckToDefault();
            // 플레이어 턴
            yield return StartCoroutine(PlayerTurn());
            // 적 턴
            yield return StartCoroutine(EnemyTurn());

        }
    }
    private IEnumerator PlayerTurn()
    {
        Debug.Log("[PlayerTurn] 시작");
        // 1) 턴 진입 플래그 세팅
        isLoading = true;    // 게임 루프 차단용 플래그
        myTurn = true;    // 플레이어 입력 허용 플래그

        // 2) 덱이 비어 있는 색상은 묘지에서 자동 보충
        RuneDeckManager.Instance.RefillEmptyColorsFromDiscard();

        // 3) 덱 UI 갱신 및 표시
        RuneDeckManager.Instance.RefreshUI();
        UIManager.Instance.ShowRuneUI();

        // 4) 실제 플레이어 입력 대기
        //    EndTurn() 호출 시 myTurn=false 로 설정되도록 되어 있음
        while (myTurn)
        {
            yield return null;
        }
        Debug.Log("[PlayerTurn] myTurn=false, 적 턴으로 넘어갑니다");
        isLoading = false;
    }


    /// <summary>
    /// 적 턴: 모든 적이 PerformTurn을 마치면 곧바로 종료됩니다.
    /// </summary>
    private IEnumerator EnemyTurn()
    {
        isLoading = true;
        myTurn = false;
 

        // → 여기서 덱 UI 숨기기
        UIManager.Instance.HideRuneUI();

        var enemies = new List<Enemy>(enemySpawner.SpawnedEnemies);
        foreach (var e in enemies)
            if (e != null)
                yield return StartCoroutine(e.PerformTurn());

        // 적 턴 종료 후 자동으로 플레이어 턴으로…
        isLoading = false;
    }
    /// <summary>
    /// 외부(End Turn 버튼 등)에서 호출하세요.
    /// </summary>
    public void EndTurn()
    {
        Debug.Log("[TurnManager] EndTurn() 호출됨, myTurn 이전값=" + myTurn);
        if (!myTurn) return;     // 이미 적 턴일 땐 무시
        myTurn = false;
        Debug.Log("[TurnManager] myTurn 설정 후값=" + myTurn);
           
    }
}
