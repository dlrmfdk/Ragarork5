using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Action", menuName = "Enemies/Enemy Action")]
public class EnemyActionSO : ScriptableObject
{
    public EnemyActionType actionType;
    [Header("공격 데미지")]
    public int value; // 공격 데미지, 방어도 수치 등
    [Header("사용횟수")]
    public int hitCount = 1; 

    [Header("UI 표시용")]
    public Sprite intentIcon; // 이 행동을 나타낼 아이콘 (예: 검, 방패 모양)
    public string intentFormat = "{0}"; // UI에 표시될 텍스트 형식 (예: "{0}", "{0}x{1}")
}