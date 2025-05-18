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
    public string displayName;  // 화면에 표시할 이름
    public string iconPath;     // Resources.Load<Sprite>(iconPath) 경로
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
    public List<DeckEntry> entries = new List<DeckEntry>();
}
