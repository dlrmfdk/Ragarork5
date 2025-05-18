using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

//치트 UI 랭킹 게임오버
public class GameManager : MonoBehaviour
{
    public static GameManager Inst { get; private set; }
    private void Awake() => Inst = this;

    

    [SerializeField] GameObject nextBt;

    // 보상 패널을 이미 보여줬는지 여부를 체크하기 위한 플래그
    private bool rewardShown = false;




    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        InputCheatKey();
#endif

        // 모든 적이 처치되었으면 다음 버튼 활성화
        if (EnemySpawner.Instance.SpawnedEnemies.Count == 0)
        {
            nextBt.SetActive(true);

            // 보상 UI가 아직 호출되지 않았다면 보상 UI를 표시
            if (!rewardShown)
            {
                //RewardManager.Instance.ShowRewardPanel();
                rewardShown = true;
            }
        }
    }
    void InputCheatKey() //개발자용 치트
    {
      

        if (Input.GetKeyDown(KeyCode.Alpha3)) //3번            
            TurnManager.Inst.EndTurn(); //엔드 턴 호출

    }
    public void StartGame()
    {
        //StartCoroutine(TurnManager.Inst.StartGameCo());
    }


    
}
