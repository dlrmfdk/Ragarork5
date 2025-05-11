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

    [Header("룬 개수")]
    public int initialCount = 0;

    [Header("기본룬 여부 (보상룬과 구별)")]
    public bool isBasic = true;    // ← 이걸로 기본룬인지 구분

    [Header("효과 SO")]
    public BaseRuneEffectSO effectSO;
}
