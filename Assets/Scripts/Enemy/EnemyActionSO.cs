// EnemyActionSO.cs
using UnityEngine;



[CreateAssetMenu(fileName = "NewEnemyAction", menuName = "Enemies/Enemy Action")]
public class EnemyActionSO : ScriptableObject
{
    [Header("행동 정보")]
    public EnemyActionType actionType; // 행동 타입 (공격, 방어, 힘모으기 등)

    [Header("수치 설정")]
    [Tooltip("공격력 배율. 예: 2.5배 피해는 2.5 입력")]
    public float damageMultiplier = 1.0f;

    [Tooltip("방어력 배율. 예: 1배 방어도는 1.0 입력")]
    public float defenseMultiplier = 1.0f;

    [Tooltip("타격 횟수 (연속 내려찍기용)")]
    public int hitCount = 1;

    [Header("UI 정보")]
    public Sprite intentIcon;
    public bool showDamageValue = true;
}