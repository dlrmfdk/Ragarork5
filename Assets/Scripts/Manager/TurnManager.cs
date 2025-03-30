using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Inst { get; private set; }
    void Awake() => Inst = this;

    [Header("Develop")] //테스트 헤더
    [SerializeField] [Tooltip("시작 턴 모드를 정합니다")] ETurnMode eTurnMode;
    [SerializeField][Tooltip("시작 카드 개수를 정함")] int  startCardCount;

    [Header("Porperties")] //일반 
    public bool isLoading; //게임 끝나서 isLoading true로 하면 카드와 엔티티 클릭방지
    public bool myTurn;
    private bool firstTurn; //게임 시작

    WaitForSeconds delay02 = new WaitForSeconds(0.2f);
    WaitForSeconds delay07 = new WaitForSeconds(0.7f);

    //(event)Action
    public static Action<bool> OnAddCard; //치트를 통한 gamemanager 병렬 호출을 위해 event 생략
    public static event Action<bool> OnTurnStarted; //턴 시작 이벤트

    enum ETurnMode {My,Enemy} //나 또는 적의 턴

    [SerializeField] private EnemySpawner enemySpawner; // NEW: EnemySpawner 참조 추가


    void GameSetup()
    {
        switch (eTurnMode)
        {
            case ETurnMode.My:
                myTurn = true;
                break;
            case ETurnMode.Enemy:
                myTurn = false;
                break;

        }

        }



    public IEnumerator StartGameCo()
    {
        GameSetup();

        isLoading = true;

        // 일반 스테이지 랜덤 적 스폰 (NEW: 게임 시작 시 적 스폰)
        enemySpawner.SpawnRandomEnemies(); 

        // Start the first turn
        yield return StartCoroutine(StartedTurnCo());

        isLoading = false;
    }

    IEnumerator StartedTurnCo() { //턴 시작
        isLoading = true; 
        if(myTurn)
        {
            GameManager.Inst.Notification("나의 턴"); //턴 시작될때

            Player.Instance.RefillMana(); // 플레이어 턴 시작 시 마나 리필

            for (int i = 0; i < startCardCount; i++) // startCardCount만큼 카드 드로우
            {
                yield return delay02;
                OnAddCard?.Invoke(myTurn);
            }
            if (firstTurn) //맨 처음 턴
            {
                firstTurn = false;
                // 추가 첫 턴 특수 행동 (필요하면 추가)                      
              

            }
        }
        else // NEW: 적의 턴일 경우
        {
            GameManager.Inst.Notification("적의 턴"); // 턴 시작 알림
            yield return StartCoroutine(EnemyTurnCo()); // 적의 턴 처리
        }


        isLoading = false;
        
        OnTurnStarted?.Invoke(myTurn);

    }


    IEnumerator EnemyTurnCo()
    {
        isLoading = true;

        // SpawnedEnemies의 복사본을 만들어 순회 (리스트 변경 문제 방지)
        List<Enemy> enemies = new List<Enemy>(enemySpawner.SpawnedEnemies);
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                yield return StartCoroutine(enemy.PerformTurn());
                yield return delay02;
            }
        }

        isLoading = false;
        myTurn = true;
        StartCoroutine(StartedTurnCo());
    }



    public void EndTurn()
    {
        if (isLoading) return; // 로딩 중일 때 턴 종료 방지

        // Discard all cards to the graveyard
        CardManager.Inst.SendAllCardsToGraveyard();


        myTurn = false; // 턴을 적에게로 변경

        StartCoroutine(StartedTurnCo());
      

    }

}
