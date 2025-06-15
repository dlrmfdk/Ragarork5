// EnemyActionSO.cs
using UnityEngine;

// 이 enum은 이미 Enemy.cs에 있을 수 있습니다. 없다면 여기에 두세요.
// public enum EnemyActionType { Attack, Defend, Buff, Debuff }

[CreateAssetMenu(fileName = "NewEnemyAction", menuName = "Enemies/Enemy Action")]
public class EnemyActionSO : ScriptableObject
{
    [Header("행동 정보")]
    public EnemyActionType actionType; // 행동 타입 (공격, 방어 등)
    public int hitCount = 1;           // 타격 횟수 (예: 6x2 공격의 '2'에 해당)

    [Header("UI 정보")]
    public Sprite intentIcon;          // 이 행동을 표시할 아이콘 (검, 방패 등)
    public bool showDamageValue = true; // UI에 데미지/방어도 수치를 표시할지 여부
}