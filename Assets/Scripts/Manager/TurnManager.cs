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
    /// 무한 루프: 플레이어 턴 → 적 턴 → 플레이어 턴 → ...
    /// </summary>
    private IEnumerator GameLoop()
    {
        // 처음엔 적 스폰
        enemySpawner.SpawnRandomEnemies();

        while (true)
        {
            yield return StartCoroutine(PlayerTurn());
            yield return StartCoroutine(EnemyTurn());
        }
    }
    private IEnumerator PlayerTurn()
    {
        isLoading = true;
        myTurn = true;

        //RuneDeckManager.Instance.LoadDeckState();
        RuneDeckManager.Instance.RefreshUI();
        UIManager.Instance.ShowRuneUI();

        // 기존 대기 로직: 플레이어가 EndTurn() 호출할 때까지
        while (myTurn)
            yield return null;

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
        // 플레이어가 자신의 턴 중에 언제든 EndTurn()을 부르면
            // 바로 myTurn을 false로 바꿔주고,
           // isLoading 체크는 제거합니다.
           if (!myTurn) return;     // 이미 적 턴일 땐 무시
         myTurn = false;
    }
}
