using UnityEngine;

[CreateAssetMenu(fileName = "RuneSO", menuName = "Runes/RuneSO")]
public class RuneSO : ScriptableObject
{
    [Header("룬 색상")]
    public RuneColor color;

    [Header("룬 이름")]
    public string displayName;

    [Header("룬 아이콘")]
    public Sprite icon;

    [Header("덱 구성 정보")]
    public bool isBasicRune = false; // 이 룬이 기본 룬인지 여부
    public int initialDeckCount = 0; // 기본 룬일 경우, 시작 덱에 포함될 개수 (보상 룬은 0)

    [Header("효과 SO")]
    public BaseRuneEffectSO effectSO;
}
