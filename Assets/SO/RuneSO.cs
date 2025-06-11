using UnityEngine;

public enum RuneColor
{
    Red,
    Blue,
    White,
    Yellow,
    Gray // 디버프 룬을 위한 색상
}

public enum RuneRarity
{
   Common,
    Rare,
    Legend
}

public enum RuneType
{
    Normal, // 일반 룬
    Penalty // 패널티 룬
}


[CreateAssetMenu(fileName = "RuneSO", menuName = "Runes/RuneSO")]

public class RuneSO : ScriptableObject
{
    [Header("룬 타입")]
    public RuneType runeType = RuneType.Normal; // 기본값은 Normal로 설정

    [Header("룬 색상")]
    public RuneColor color;

    [Header("룬 희귀도")]
    public RuneRarity rarity;

    [Header("룬 이름")]
    public string displayName;

    [Header("룬 설명")]
    [TextArea(3, 10)] // Inspector에서 여러 줄로 편하게 입력할 수 있게 해줍니다.
    public string description;

    [Header("룬 아이콘")]
    public Sprite icon;

    [Header("덱 구성 정보")]
    public bool isBasicRune = false; // 이 룬이 기본 룬인지 여부
    public int initialDeckCount = 0; // 기본 룬일 경우, 시작 덱에 포함될 개수 (보상 룬은 0)

    [Header("효과 SO")]
    public BaseRuneEffectSO effectSO;
}
