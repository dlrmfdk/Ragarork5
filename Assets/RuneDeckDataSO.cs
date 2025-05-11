using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 덱 데이터를 저장/로드하기 위한 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "RuneDeckDataSO", menuName = "Runes/RuneDeckDataSO")]
public class RuneDeckDataSO : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string runeID;  // RuneSO.name
        public int count;      // 덱 내 보유 개수
    }

    // 덱에 저장된 룬 정보 리스트
    public List<Entry> entries = new List<Entry>();
}
