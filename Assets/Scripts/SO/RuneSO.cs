using UnityEngine;

[CreateAssetMenu(fileName = "RuneSO", menuName = "Runes/RuneSO")]
public class RuneSO : ScriptableObject
{
    [Header("·é »ö»ó")]
    public RuneColor color;

    [Header("·é ÀÌ¸§")]
    public string displayName;

    [Header("·é ¾ÆÀÌÄÜ")]
    public Sprite icon;

    [Header("·é °³¼ö")]
    public int initialCount = 0;

    [Header("È¿°ú SO")]
    public BaseRuneEffectSO effectSO;
}
