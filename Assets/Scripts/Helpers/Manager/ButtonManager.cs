using S3MG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public void onNextClicked()
    {
        SceneManager.LoadScene("MapScene");
        MapGenerator.instance.activeMap();
        MapGenerator.instance.toNextNode();
        
    }

    public void onNextClickedBoss()
    {   // 1. 현재 MapGenerator 인스턴스가 있는지 확인하고 파괴합니다.
        if (MapGenerator.instance != null)
        {
            Debug.Log("기존 MapGenerator 인스턴스를 파괴합니다.");
            // MapGenerator 게임 오브젝트 자체를 파괴합니다.
            // (Awake 로직에 따라 연결된 Canvas도 함께 파괴될 것입니다)
            Destroy(MapGenerator.instance.gameObject);
        }
        else
        {
            Debug.LogWarning("MapGenerator 인스턴스가 이미 null입니다.");
        }

        // 2. MapScene2를 로드합니다.
        // (MapScene2에 있는 새로운 MapGenerator가 Awake()를 통해 초기화될 것입니다)
        SceneManager.LoadScene("MapScene2");

    }
}
