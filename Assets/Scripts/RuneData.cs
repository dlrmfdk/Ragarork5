using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// JSON 파싱용: 룬 데이터베이스 모델
/// (Resources/RunesDatabase.json) 파싱 시 사용
/// </summary>
[Serializable]
public class RuneData
{
    public string id;           // 고유 ID (예: "RedBasicRune")
    public string color;        // 룬 색상 (예: "Red", "Blue")
    public string rarity;         // 룬 희귀도
    public string displayName;  // 화면에 표시할 이름
    //public string iconPath;     // Resources.Load<Sprite>(iconPath) 경로
    public int initialCount; // 기본 덱에 포함될 개수
    public string effectID;     // BaseRuneEffectSO.name 과 매핑
}

[Serializable]
public class RuneDatabase
{
    public List<RuneData> runes = new List<RuneData>();
}

/// <summary>
/// 덱 상태 저장용 모델
/// (persistentDataPath/DeckState.json) 로드/저장 시 사용
/// </summary>
[Serializable]
public class DeckEntry
{
    public string runeID;  // RuneData.id 또는 JSON database ID
    public int count;   // 덱에 보유 중인 개수
}
[Serializable]
public class DeckState
{
    // public List<DeckEntry> entries = new List<DeckEntry>(); // 이 줄은 삭제하거나 주석 처리
    public List<RuneInstance> playerDeck = new List<RuneInstance>(); // 플레이어 덱
    public List<RuneInstance> discardPile = new List<RuneInstance>(); // 묘지
}

/// <summary>
/// 플레이어가 소유한 개별 룬의 인스턴스 데이터.
/// (예: '피해량 6짜리 기본 빨강 룬')
/// </summary>
[Serializable]
public class RuneInstance
{
    public string runeId; // 이 룬의 종류 (예: "RedBasicRune")
    public int value;     // 이 룬에 부여된 고유 수치 (예: 6)

    // runeId를 이용해 원본 RuneSO 데이터를 찾아오는 편의용 프로퍼티
    [NonSerialized] private RuneSO _so;
    public RuneSO SO
    {
        get
        {
            if (_so == null && RuneDeckManager.Instance != null)
            {
                _so = RuneDeckManager.Instance.runeDefinitions.Find(r => r.name == runeId);
            }
            return _so;
        }
    }

    // 기본 생성자 (JSON 역직렬화를 위해 필요)
    public RuneInstance() { }

    // 데이터를 직접 넣어 생성할 때 사용하는 생성자
    public RuneInstance(string id, int val)
    {
        runeId = id;
        value = val;
    }
}