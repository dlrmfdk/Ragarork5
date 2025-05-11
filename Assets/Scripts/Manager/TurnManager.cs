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

    /// <summary>
    /// 플레이어 턴: 알림을 띄우고, EndTurn() 호출을 기다립니다.
    /// </summary>
    private IEnumerator PlayerTurn()
    {
        isLoading = true;
        myTurn = true;
        GameManager.Inst.Notification("나의 턴");
        OnTurnStarted?.Invoke(true);

        // 플레이어가 EndTurn()을 부르면 myTurn이 false가 됩니다.
        //플레이어가 턴을 끝내겠다고 선언(EndTurn 호출)하기 전까지 이 코루틴을 종료시키지 말라
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
        GameManager.Inst.Notification("적의 턴");

        // 복사본으로 순회하여 원본 리스트 수정 방지
        var enemies = new List<Enemy>(enemySpawner.SpawnedEnemies);
        foreach (var e in enemies)
        {
            if (e != null)
                yield return StartCoroutine(e.PerformTurn());
        }

        // 적 턴 끝나면 즉시 플레이어 턴으로 돌아갑니다.
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
