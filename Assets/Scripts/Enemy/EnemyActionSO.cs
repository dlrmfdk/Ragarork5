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

    // ▼▼▼ [이 부분을 추가하세요] ▼▼▼
    [Header("애니메이션")]
    [Tooltip("이 행동을 할 때 재생할 애니메이션의 이름")]
    public string animationName = "attack"; // 기본값을 "attack"으로 설정
    // ▲▲▲ 추가 완료 ▲▲▲

    // ▼▼▼ [수정] 이 변수들을 여기에 '추가'합니다 ▼▼▼
    [Header("의도 툴팁 정보")]
    [Tooltip("툴팁에 표시될 제목 (예: 공격)")]
    public string tooltipTitle = "공격";

    [Tooltip("툴팁에 표시될 설명 (예: {0}의 피해를 줍니다.)\n{0}은 계산된 피해량/방어도로 자동 대체됩니다.")]
    [TextArea(2, 4)]
    public string tooltipDescriptionFormat = "{0}의 피해를 줍니다.";
    // ▲▲▲ 변수 추가 완료 ▲▲▲

}