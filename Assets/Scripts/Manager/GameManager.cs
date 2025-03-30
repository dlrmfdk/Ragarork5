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

    

    [SerializeField] NotificationPanel NotificationPanel;
    [SerializeField] GameObject nextBt;

  
    void Start()
    {
        StartGame();
        
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        InputCheatKey();
#endif

        
        if (EnemySpawner.Instance.SpawnedEnemies.Count == 0)
        {
            nextBt.SetActive(true);
        }
    }
    void InputCheatKey() //개발자용 치트
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) //1번 누르면
            //print(PopItem().name);
            TurnManager.OnAddCard?.Invoke(true);

        if (Input.GetKeyDown(KeyCode.Alpha3)) //3번            
            TurnManager.Inst.EndTurn(); //엔드 턴 호출

    }
    public void StartGame()
    {
        StartCoroutine(TurnManager.Inst.StartGameCo());
    }
    public void Notification(string message)
    {
        NotificationPanel.Show(message);
    }

    
}
