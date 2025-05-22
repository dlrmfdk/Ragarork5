using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonEffect : MonoBehaviour
{
    
    //effectPrefab
    [SerializeField] private GameObject effectPrefab; // 이펙트 프리팹을 연결합니다.
    

    //버튼을 누르면 특정 이펙트를 출력하는 스크립트
    public void OnButtonClick()
    {
        // 이펙트 출력
        Debug.Log("Button clicked! Effect triggered.");
        // 이펙트 관련 코드 추가
        Instantiate(effectPrefab, transform.position, Quaternion.identity);
    }
}
