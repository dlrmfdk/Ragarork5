// EventSO.cs (확률 기능이 추가된 최종 버전)
using UnityEngine;
using System.Collections.Generic;

// 결과의 종류 (기존과 동일)
public enum EventOutcomeType
{
    GainHealth,
    LoseHealth,
    GainMaxHealth,
    GainGold,
    LoseGold,
    AddRune,
    UpgradeRandomRune,
    Nothing
}

// 하나의 '결과'를 정의하는 클래스 (기존과 동일)
[System.Serializable]
public class EventOutcome
{
    public string outcomeMessage;
    public EventOutcomeType type;
    public int amount;
    public RuneSO rune;
}

// ▼▼▼ [새로 추가] 확률을 가진 결과를 정의하는 클래스 ▼▼▼
[System.Serializable]
public class ProbabilisticOutcome
{
    [Tooltip("이 결과가 선택될 가중치. 다른 결과들과의 상대적인 비율로 확률이 결정됩니다. (예: 50, 50 -> 50%)")]
    public int weight; // 가중치 (확률)
    public EventOutcome outcome; // 실제 일어날 결과
}

// ▼▼▼ [수정] 하나의 '선택지'를 정의하는 클래스 ▼▼▼
[System.Serializable]
public class EventChoice
{
    [Tooltip("버튼에 표시될 선택지 텍스트")]
    public string choiceDescription;

    [Tooltip("이 선택지를 고르면 '항상' 발생하는 결과 목록")]
    public List<EventOutcome> guaranteedOutcomes; // 확정 결과

    [Tooltip("이 선택지를 고르면 '확률적'으로 발생하는 결과 목록")]
    public List<ProbabilisticOutcome> randomOutcomes; // 확률 결과
}

[CreateAssetMenu(fileName = "New Event", menuName = "Event/EventSO")]
public class EventSO : ScriptableObject
{
    [Header("이벤트 기본 정보")]
    public string eventName;
    [TextArea(5, 10)]
    public string eventDescription;
    public Sprite eventImage;

    [Header("선택지 목록")]
    public List<EventChoice> choices;
}