using UnityEngine;

public enum RuneColor
{
    Red,
    Blue,
    White,
    Yellow,
}

public enum RuneRarity
{
   Common,
    Rare,
    Legend
}

[CreateAssetMenu(fileName = "RuneSO", menuName = "Runes/RuneSO")]

public class RuneSO : ScriptableObject
{
    [Header("·é »ö»ó")]
    public RuneColor color;

    [Header("·é Èñ±Íµµ")]
    public RuneRarity rarity;

    [Header("·é ÀÌ¸§")]
    public string displayName;

    [Header("·é ¾ÆÀÌÄÜ")]
    public Sprite icon;

    [Header("µ¦ ±¸¼º Á¤º¸")]
    public bool isBasicRune = false; // ÀÌ ·éÀÌ ±âº» ·éÀÎÁö ¿©ºÎ
    public int initialDeckCount = 0; // ±âº» ·éÀÏ °æ¿ì, ½ÃÀÛ µ¦¿¡ Æ÷ÇÔµÉ °³¼ö (º¸»ó ·éÀº 0)

    [Header("È¿°ú SO")]
    public BaseRuneEffectSO effectSO;
}
